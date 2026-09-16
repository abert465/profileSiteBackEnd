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
            // Order matters if cascade isn't guaranteed everywhere
            _db.Skills.RemoveRange(_db.Skills);
            _db.Projects.RemoveRange(_db.Projects);
            _db.Posts.RemoveRange(_db.Posts);
            _db.Experiences.RemoveRange(_db.Experiences);
            _db.Educations.RemoveRange(_db.Educations);
            _db.Certifications.RemoveRange(_db.Certifications);
            _db.Testimonials.RemoveRange(_db.Testimonials);
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
            ["testimonials"]=0,
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
            // Availability is deliberately NOT copied here, unlike every other
            // scalar above. It is seeded once on insert and owned by the admin
            // panel afterwards — the same rule Projects already apply to
            // ImageUrl, for the same reason: it is state someone sets through
            // the UI, not copy that should be refreshed from source.
            //
            // The concrete failure this avoids: switch the badge off after
            // landing a role, deploy any unrelated content change, and the seed
            // run flips AvailabilityVisible back to true. The site would go back
            // to advertising an active job search, silently, in front of the new
            // employer. Reseeding must never be able to re-open that.
            //
            // Changing the wording after first seed is an admin-panel edit.

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
        var sampleProjects = SampleData.GetProjects();
        foreach (var p in sampleProjects)
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
                existing.SortOrder   = p.SortOrder;

                // ImageUrl is the one field the admin panel owns: it is set by
                // uploading a file, not by editing text. Seed it when the row has
                // nothing, so a fresh database still renders, but never overwrite
                // an upload someone made through the panel.
                if (string.IsNullOrWhiteSpace(existing.ImageUrl))
                    existing.ImageUrl = p.ImageUrl;
            }
        }

        // Remove rows whose slug is no longer in SampleData (e.g. a project pulled from the portfolio)
        var sampleSlugs = sampleProjects.Select(p => p.Slug).ToHashSet();
        var orphanProjects = await _db.Projects
            .Where(p => !sampleSlugs.Contains(p.Slug))
            .ToListAsync(ct);
        if (orphanProjects.Count > 0)
            _db.Projects.RemoveRange(orphanProjects);

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
                existing.Content   = p.Content;
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
                row.RoleNote  = e.RoleNote;
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

        // Remove rows no longer in SampleData, matching how Projects are handled.
        // Without this, editing an entry's role or start date silently leaves the
        // old row behind rather than replacing it, because the key changes and the
        // upsert above treats the edited version as a brand new entry. Merging two
        // roles into one is exactly that case: it would add the merged entry and
        // keep both originals, so the site would show a longer history than the
        // resume.
        //
        // Note this makes SampleData authoritative for experience: a row added
        // through the admin UI and never added to SampleData will be removed the
        // next time the seeder runs.
        var sampleExpKeys = SampleData.GetExperience().Select(ExpKey).ToHashSet();
        var orphanExperience = expAll.Where(e => !sampleExpKeys.Contains(ExpKey(e))).ToList();
        if (orphanExperience.Count > 0)
            _db.Experiences.RemoveRange(orphanExperience);

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

        // Same orphan problem Projects and Experience already solve. EduKey
        // includes Degree, so renaming a degree — WGU relabelling "Software
        // Development" as "Software Engineering", for instance — reads as a new
        // entry and would leave the old one behind, showing two degrees for one
        // enrollment.
        var sampleEduKeys = SampleData.GetEducation().Select(EduKey).ToHashSet();
        var orphanEducation = eduAll.Where(e => !sampleEduKeys.Contains(EduKey(e))).ToList();
        if (orphanEducation.Count > 0)
            _db.Educations.RemoveRange(orphanEducation);

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

        // CertKey includes the issue date, so correcting a date or an issuer name
        // would otherwise leave the stale certification on the site alongside the
        // corrected one. An expired or misstated credential is worse than none.
        var sampleCertKeys = SampleData.GetCertifications().Select(CertKey).ToHashSet();
        var orphanCertifications = certAll.Where(c => !sampleCertKeys.Contains(CertKey(c))).ToList();
        if (orphanCertifications.Count > 0)
            _db.Certifications.RemoveRange(orphanCertifications);

        // Testimonials deliberately get no orphan sweep: GetTestimonials() is
        // empty by design and real entries come from the admin panel, so a sweep
        // here would delete every genuine testimonial on each seed run.
        // ---- Testimonials ----
        string TestKey(Testimonial t) => $"{t.Name}|{t.Company}|{(t.Date?.ToString("yyyy-MM-dd") ?? "null")}";
        var testAll = await _db.Testimonials.ToListAsync(ct);
        var testIndex = testAll.ToDictionary(TestKey, t => t);
        foreach (var t in SampleData.GetTestimonials())
        {
            if (testIndex.TryGetValue(TestKey(t), out var row))
            {
                row.Name      = t.Name;
                row.Title     = t.Title;
                row.Company   = t.Company;
                row.Content   = t.Content;
                row.Date      = t.Date;
                row.Rating    = t.Rating;
                row.IsVisible = t.IsVisible;
                row.Order     = t.Order;
            }
            else
            {
                _db.Testimonials.Add(t);
                added["testimonials"]++;
            }
        }

        await _db.SaveChangesAsync(ct);
        return new { ok = true, added };
    }
}
