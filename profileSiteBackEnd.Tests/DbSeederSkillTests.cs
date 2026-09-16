using Microsoft.EntityFrameworkCore;
using profileSiteBackEnd;
using profileSiteBackEnd.Models;
using Xunit;

namespace profileSiteBackEnd.Tests;

/// <summary>
/// Skills were the last seeded list with no orphan sweep, which made SampleData
/// write-only: removing a skill left the chip live forever and renaming one left
/// the old spelling behind as a duplicate. These cover the sweep and the pieces
/// around it that are easy to break.
/// </summary>
public class DbSeederSkillTests : IDisposable
{
    private readonly string _dbPath =
        Path.Combine(Path.GetTempPath(), $"profilesite-skilltest-{Guid.NewGuid():N}.db");

    private AppDbContext NewContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={_dbPath}")
            .Options);

    private async Task SeedAsync()
    {
        await using var db = NewContext();
        await new DbSeeder(db).SeedAsync();
    }

    [Fact]
    public async Task Seeding_GivesEverySkillACategory()
    {
        await SeedAsync();

        await using var db = NewContext();
        var uncategorised = await db.Skills
            .Where(s => s.Category == null || s.Category == "")
            .Select(s => s.Name)
            .ToListAsync();

        Assert.Empty(uncategorised);
    }

    [Fact]
    public async Task Seeding_OnlyUsesKnownCategories()
    {
        await SeedAsync();

        await using var db = NewContext();
        var categories = await db.Skills.Select(s => s.Category).Distinct().ToListAsync();

        Assert.All(categories, c => Assert.Contains(c, SampleData.SkillCategories));
    }

    /// <summary>
    /// The regression this file exists for: a skill dropped from SampleData has
    /// to leave the database, or the chip outlives the decision to remove it.
    /// </summary>
    [Fact]
    public async Task Reseeding_RemovesASkillNoLongerInSampleData()
    {
        await SeedAsync();

        await using (var db = NewContext())
        {
            var profileId = await db.Profiles.Select(p => p.Id).FirstAsync();
            db.Skills.Add(new Skill
            {
                ProfileId = profileId,
                Name = "COBOL",
                Category = "Backend & .NET",
                IsVisible = true,
                Order = 999,
            });
            await db.SaveChangesAsync();
        }

        await SeedAsync();

        await using var verify = NewContext();
        Assert.False(await verify.Skills.AnyAsync(s => s.Name == "COBOL"));
    }

    /// <summary>
    /// Hiding a seeded skill stays the no-deploy way to drop one from the page,
    /// so the sweep must not resurrect its visibility.
    /// </summary>
    [Fact]
    public async Task Reseeding_KeepsAHiddenSkillHidden()
    {
        await SeedAsync();

        var name = SampleData.GetSkills()[0].Name;

        await using (var db = NewContext())
        {
            var row = await db.Skills.FirstAsync(s => s.Name == name);
            row.IsVisible = false;
            await db.SaveChangesAsync();
        }

        await SeedAsync();

        await using var verify = NewContext();
        var after = await verify.Skills.FirstAsync(s => s.Name == name);
        Assert.False(after.IsVisible);
    }

    /// <summary>
    /// Category is structure rather than state, so unlike visibility it is
    /// realigned from SampleData on every run.
    /// </summary>
    [Fact]
    public async Task Reseeding_RealignsAnEditedCategory()
    {
        await SeedAsync();

        var seeded = SampleData.GetSkills()[0];

        await using (var db = NewContext())
        {
            var row = await db.Skills.FirstAsync(s => s.Name == seeded.Name);
            row.Category = "Wrong Category";
            await db.SaveChangesAsync();
        }

        await SeedAsync();

        await using var verify = NewContext();
        var after = await verify.Skills.FirstAsync(s => s.Name == seeded.Name);
        Assert.Equal(seeded.Category, after.Category);
    }

    public void Dispose()
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        foreach (var path in new[] { _dbPath, $"{_dbPath}-wal", $"{_dbPath}-shm" })
        {
            try { if (File.Exists(path)) File.Delete(path); } catch { /* temp file */ }
        }
        GC.SuppressFinalize(this);
    }
}
