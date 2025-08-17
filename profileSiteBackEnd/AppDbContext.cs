using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using profileSiteBackEnd.Models;

namespace profileSiteBackEnd
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> opts) : base(opts) { }

        public DbSet<Project> Projects => Set<Project>();
        public DbSet<Post> Posts => Set<Post>();
        public DbSet<Experience> Experiences => Set<Experience>();
        public DbSet<Education> Educations => Set<Education>();
        public DbSet<Certification> Certifications => Set<Certification>();
        public DbSet<Profile> Profiles => Set<Profile>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            //Keys
            b.Entity<Project>().HasKey(x => x.Slug);
            b.Entity<Post>().HasKey(x => x.Slug);

            //Add simple integer keys for items that didnt have one
            b.Entity<Experience>().HasKey(x => x.Id);
            b.Entity<Education>().HasKey(x => x.Id);
            b.Entity<Certification>().HasKey(x => x.Id);
            b.Entity<Profile>().HasKey(x => x.Id);

            //Map List<string> as json text columns
            var json = new JsonSerializerOptions(JsonSerializerDefaults.Web);

            ValueConverter<List<string>, string> listConv =
                new(v => JsonSerializer.Serialize(v ?? new(), json),
                    v => string.IsNullOrWhiteSpace(v) ? new() : JsonSerializer.Deserialize<List<string>>(v, json) ?? new());

            b.Entity<Project>().Property(x => x.Tech).HasConversion(listConv);
            b.Entity<Project>().Property(x => x.Highlights).HasConversion(listConv);
            b.Entity<Experience>().Property(x => x.Highlights).HasConversion(listConv);
            b.Entity<Experience>().Property(x => x.Tech).HasConversion(listConv);
            b.Entity<Education>().Property(x => x.Details).HasConversion(listConv);

            // Single row Profile convenience
            b.Entity<Profile>().Property<int>("Id").HasDefaultValue(1);
        }
    }
}
