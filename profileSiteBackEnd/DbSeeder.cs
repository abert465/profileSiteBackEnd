using System.Data;
using Microsoft.EntityFrameworkCore;
using profileSiteBackEnd;
using profileSiteBackEnd.Models;

public class DbSeeder
{
    private readonly AppDbContext _db;
    public DbSeeder(AppDbContext db) => _db = db;

    public async Task<object> SeedAsync(bool reset = false, CancellationToken ct = default)
    {
        await _db.Database.MigrateAsync(ct);

        // Optional: tune SQLite
        if ((_db.Database.ProviderName ?? "").Contains("Sqlite", StringComparison.OrdinalIgnoreCase))
        {
            var conn = _db.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open) await conn.OpenAsync(ct);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "PRAGMA journal_mode=WAL; PRAGMA busy_timeout=5000;";
            await cmd.ExecuteNonQueryAsync(ct);
        }

        if (reset)
        {
            // Order matters if cascade isn’t guaranteed everywhere
            _db.Skills.RemoveRange(_db.Skills);
            _db.Projects.RemoveRange(_db.Projects);
            _db.Posts.RemoveRange(_db.Posts);
            _db.Experiences.RemoveRange(_db.Experiences);
            _db.Educations.RemoveRange(_db.Educations);
            _db.Certifications.RemoveRange(_db.Certifications);
            _db.Profiles.RemoveRange(_db.Profiles);
            await _db.SaveChangesAsync(ct);
        }

        var added = new Dictionary<string, int>
        {
            ["projects"]=0,
            ["posts"]=0,
            ["experience"]=0,
            ["education"]=0,
            ["certifications"]=0,
            ["profile"]=0,
            ["skills"]=0
        };

        // ---- Profile (single row + owned Links) ----
        var seedProfile = SampleData.GetProfile();

        var curProfile = await _db.Profiles
            .Include(p => p.Links)
            .FirstOrDefaultAsync(ct);

        if (curProfile is null)
        {
            _db.Profiles.Add(seedProfile); // owned Links will be saved too
            added["profile"]++;
            await _db.SaveChangesAsync(ct); // need PK for Skills FK
        }
        else
        {
            // Copy scalars
            curProfile.Name     = seedProfile.Name;
            curProfile.Title    = seedProfile.Title;
            curProfile.Tagline  = seedProfile.Tagline;
            curProfile.Summary  = seedProfile.Summary;
            curProfile.Location = seedProfile.Location;
            curProfile.Email    = seedProfile.Email;
            curProfile.Github   = seedProfile.Github;
            curProfile.Linkedin = seedProfile.Linkedin;

            // Replace owned Links (clear & re-add)
            curProfile.Links.Clear();
            foreach (var l in seedProfile.Links)
                curProfile.Links.Add(new Link { Label = l.Label, Url = l.Url });

            await _db.SaveChangesAsync(ct); // ensure persisted before Skills upsert
        }

        // Resolve Profile PK (shadow key "Id" on Profile)
        var profileId = await _db.Profiles
            .Select(p => EF.Property<int>(p, "Id"))
            .FirstAsync(ct);

        // ---- Skills (table) ----
        // Use SampleData profile’s Skills list as initial set (visible, ordered by index)
        var initialSkills = seedProfile.Skills ?? new List<string>();
        Console.WriteLine($"[SEED] Initial skills in SampleData: {initialSkills.Count}");

        var existingSkills = await _db.Skills
            .Where(s => s.ProfileId == profileId)
            .ToListAsync(ct);

        var byName = existingSkills.ToDictionary(s => s.Name, s => s);
        for (int i = 0; i < initialSkills.Count; i++)
        {
            var name = (initialSkills[i] ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(name)) continue;

            if (!byName.TryGetValue(name, out var row))
            {
                _db.Skills.Add(new Skill
                {
                    ProfileId = profileId,
                    Name = name,
                    IsVisible = true,
                    Order = i
                });
                added["skills"]++;
            }
            else
            {
                // keep admin visibility; just align order
                if (row.Order != i) row.Order = i;
            }
        }

        // ---- Projects (keyed by Slug) ----
        foreach (var p in SampleData.GetProjects())
        {
            var existing = await _db.Projects.FindAsync([p.Slug], ct);
            if (existing is null)
            {
                _db.Projects.Add(p);
                added["projects"]++;
            }
            else
            {
                existing.Title       = p.Title;
                existing.Description = p.Description;
                existing.Tech        = new List<string>(p.Tech);
                existing.RepoUrl     = p.RepoUrl;
                existing.LiveUrl     = p.LiveUrl;
                existing.Highlights  = new List<string>(p.Highlights);
            }
        }

        // ---- Posts (keyed by Slug) ----
        foreach (var p in SampleData.GetPosts())
        {
            var existing = await _db.Posts.FindAsync([p.Slug], ct);
            if (existing is null)
            {
                if (p.Published == default) p.Published = DateTime.UtcNow;
                _db.Posts.Add(p);
                added["posts"]++;
            }
            else
            {
                existing.Title     = p.Title;
                existing.Excerpt   = p.Excerpt;
                existing.Published = (p.Published == default) ? existing.Published : p.Published;
            }
        }

        // Natural keys for upserts (avoid dup rows)
        string ExpKey(Experience e) => $"{e.Company}|{e.Role}|{e.Start:yyyy-MM-dd}";
        string EduKey(Education e) => $"{e.School}|{e.Degree}|{e.End:yyyy-MM-dd}";
        string CertKey(Certification c) => $"{c.Name}|{c.Issuer}|{(c.Issued?.ToString("yyyy-MM-dd") ?? "null")}";

        // ---- Experience ----
        var expAll = await _db.Experiences.ToListAsync(ct);
        var expIndex = expAll.ToDictionary(ExpKey, e => e);
        foreach (var e in SampleData.GetExperience())
        {
            if (expIndex.TryGetValue(ExpKey(e), out var row))
            {
                row.Company   = e.Company;
                row.Role      = e.Role;
                row.Location  = e.Location;
                row.Start     = e.Start;
                row.End       = e.End;
                row.Highlights= new List<string>(e.Highlights);
                row.Tech      = new List<string>(e.Tech);
            }
            else
            {
                _db.Experiences.Add(e);
                added["experience"]++;
            }
        }

        // ---- Education ----
        var eduAll = await _db.Educations.ToListAsync(ct);
        var eduIndex = eduAll.ToDictionary(EduKey, e => e);
        foreach (var e in SampleData.GetEducation())
        {
            if (eduIndex.TryGetValue(EduKey(e), out var row))
            {
                row.School  = e.School;
                row.Degree  = e.Degree;
                row.Start   = e.Start;
                row.End     = e.End;
                row.Details = e.Details is null ? null : new List<string>(e.Details);
            }
            else
            {
                _db.Educations.Add(e);
                added["education"]++;
            }
        }

        // ---- Certifications ----
        var certAll = await _db.Certifications.ToListAsync(ct);
        var certIndex = certAll.ToDictionary(CertKey, c => c);
        foreach (var c in SampleData.GetCertifications())
        {
            if (certIndex.TryGetValue(CertKey(c), out var row))
            {
                row.Name    = c.Name;
                row.Issuer  = c.Issuer;
                row.Issued  = c.Issued;
                row.Expires = c.Expires;
            }
            else
            {
                _db.Certifications.Add(c);
                added["certifications"]++;
            }
        }

        await _db.SaveChangesAsync(ct);
        return new { ok = true, added };
    }
}
