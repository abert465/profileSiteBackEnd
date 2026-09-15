using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using profileSiteBackEnd;
using profileSiteBackEnd.Models;

namespace profileSiteBackEnd.Tests;

// Swaps the real SQLite file DB for an in-memory one, seeded with a single
// Profile row so endpoints that expect one (e.g. /api/profile) have data to return.
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // This factory creates the schema itself via EnsureCreated below, so the
        // app's startup migration would collide with it.
        builder.UseSetting("Database:MigrateOnStartup", "false");

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor is not null) services.Remove(descriptor);

            // Keep the connection open for the lifetime of the factory - an
            // in-memory SQLite DB is dropped once its last connection closes.
            _connection.Open();
            services.AddDbContext<AppDbContext>(o => o.UseSqlite(_connection));

            using var scope = services.BuildServiceProvider().CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
            db.Profiles.Add(new Profile { Name = "Test User", Title = "Test Title", Tagline = "Test" });
            db.SaveChanges();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing) _connection.Dispose();
    }
}
