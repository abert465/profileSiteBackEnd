using Microsoft.EntityFrameworkCore;
using profileSiteBackEnd;
using profileSiteBackEnd.Models;
using Xunit;

namespace profileSiteBackEnd.Tests;

/// <summary>
/// The availability badge is the one seeded scalar the admin panel owns after
/// first seed. Everything else in SampleData is copy that should be refreshed
/// from source on every seed run; this is state a person sets through the UI.
///
/// These run against a real temp SQLite file rather than the in-memory provider
/// because DbSeeder starts with MigrateAsync, which in-memory does not support.
/// </summary>
public class DbSeederAvailabilityTests : IDisposable
{
    private readonly string _dbPath =
        Path.Combine(Path.GetTempPath(), $"profilesite-seedtest-{Guid.NewGuid():N}.db");

    private AppDbContext NewContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={_dbPath}")
            .Options);

    [Fact]
    public async Task FirstSeed_TakesTheBadgeFromSampleData()
    {
        await using (var db = NewContext())
            await new DbSeeder(db).SeedAsync();

        await using var verify = NewContext();
        var profile = await verify.Profiles.FirstAsync();

        var seeded = SampleData.GetProfile();
        Assert.Equal(seeded.AvailabilityNote, profile.AvailabilityNote);
        Assert.Equal(seeded.AvailabilityVisible, profile.AvailabilityVisible);
    }

    /// <summary>
    /// The regression this file exists for. Switch the badge off after landing a
    /// role, deploy any unrelated content change — reseed.ps1 runs on every
    /// deploy — and a seeder that copied this scalar would turn the badge back
    /// on. The site would resume advertising an active job search, silently, in
    /// front of the new employer.
    /// </summary>
    [Fact]
    public async Task Reseeding_DoesNotReopenAHiddenBadge()
    {
        await using (var db = NewContext())
            await new DbSeeder(db).SeedAsync();

        await using (var db = NewContext())
        {
            var profile = await db.Profiles.FirstAsync();
            profile.AvailabilityVisible = false;
            profile.AvailabilityNote = "";
            await db.SaveChangesAsync();
        }

        await using (var db = NewContext())
            await new DbSeeder(db).SeedAsync();

        await using var verify = NewContext();
        var after = await verify.Profiles.FirstAsync();

        Assert.False(after.AvailabilityVisible);
        Assert.Equal("", after.AvailabilityNote);
    }

    /// <summary>
    /// Wording edited through the admin panel survives a seed run too, so the
    /// badge can say something other than the sample text without a deploy
    /// quietly reverting it.
    /// </summary>
    [Fact]
    public async Task Reseeding_KeepsAnEditedBadgeNote()
    {
        await using (var db = NewContext())
            await new DbSeeder(db).SeedAsync();

        const string edited = "Open to staff-level .NET roles, remote";
        await using (var db = NewContext())
        {
            var profile = await db.Profiles.FirstAsync();
            profile.AvailabilityNote = edited;
            await db.SaveChangesAsync();
        }

        await using (var db = NewContext())
            await new DbSeeder(db).SeedAsync();

        await using var verify = NewContext();
        Assert.Equal(edited, (await verify.Profiles.FirstAsync()).AvailabilityNote);
    }

    /// <summary>
    /// Guards the boundary: the rest of the profile is still seed-owned, so a
    /// future change cannot quietly freeze all of it the way availability is
    /// frozen here.
    /// </summary>
    [Fact]
    public async Task Reseeding_StillRefreshesOrdinaryProfileCopy()
    {
        await using (var db = NewContext())
            await new DbSeeder(db).SeedAsync();

        await using (var db = NewContext())
        {
            var profile = await db.Profiles.FirstAsync();
            profile.Tagline = "stale tagline that should be overwritten";
            await db.SaveChangesAsync();
        }

        await using (var db = NewContext())
            await new DbSeeder(db).SeedAsync();

        await using var verify = NewContext();
        Assert.Equal(SampleData.GetProfile().Tagline, (await verify.Profiles.FirstAsync()).Tagline);
    }

    public void Dispose()
    {
        // SQLite keeps a pooled connection to the file; drop it before deleting.
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        foreach (var path in new[] { _dbPath, $"{_dbPath}-wal", $"{_dbPath}-shm" })
        {
            try { if (File.Exists(path)) File.Delete(path); } catch { /* temp file */ }
        }
        GC.SuppressFinalize(this);
    }
}
