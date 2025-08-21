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
        public DbSet<Skill> Skills => Set<Skill>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            //Keys
            b.Entity<Project>().HasKey(x => x.Slug);
            b.Entity<Post>().HasKey(x => x.Slug);

            // Shadow keys so you don't have to edit your model classes right now
            b.Entity<Profile>().Property<int>("Id").ValueGeneratedOnAdd();
            b.Entity<Profile>().HasKey("Id");

            b.Entity<Experience>().Property<int>("Id").ValueGeneratedOnAdd();
            b.Entity<Experience>().HasKey("Id");

            b.Entity<Education>().Property<int>("Id").ValueGeneratedOnAdd();
            b.Entity<Education>().HasKey("Id");

            b.Entity<Certification>().Property<int>("Id").ValueGeneratedOnAdd();
            b.Entity<Certification>().HasKey("Id");

            b.Entity<Profile>().Ignore(p => p.Skills);

            b.Entity<Profile>().OwnsMany(p => p.Links, nav =>
            {
                nav.WithOwner().HasForeignKey("ProfileId");
                nav.Property<int>("Id");               // shadow PK for owned row
                nav.HasKey("Id");
                nav.Property(l => l.Label).HasMaxLength(128);
                nav.Property(l => l.Url).HasMaxLength(2048);
                nav.ToTable("ProfileLinks");           // optional nice table name
            });
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

            b.Entity<Skill>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Name).HasMaxLength(100).IsRequired();
                e.Property(x => x.IsVisible).HasDefaultValue(true);
                e.Property(x => x.Order);
                e.HasIndex(x => new { x.ProfileId, x.Name }).IsUnique();

                e.HasOne(x => x.Profile)
                    .WithMany()
                    .HasForeignKey(x => x.ProfileId)
                    .HasPrincipalKey("Id")
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
