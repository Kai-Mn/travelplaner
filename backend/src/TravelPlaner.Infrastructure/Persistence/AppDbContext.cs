using Microsoft.EntityFrameworkCore;
using TravelPlaner.Domain.Entities;
using TravelPlaner.Domain.ValueObjects;

namespace TravelPlaner.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Journey> Journeys => Set<Journey>();
    public DbSet<Image> Images => Set<Image>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<JourneyLocation> JourneyLocations => Set<JourneyLocation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Email).IsRequired().HasMaxLength(320);
            e.Property(u => u.PasswordHash).IsRequired();
            e.HasMany(u => u.Locations).WithOne(l => l.User)
                .HasForeignKey(l => l.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(u => u.Journeys).WithOne(j => j.User)
                .HasForeignKey(j => j.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Location>(e =>
        {
            e.HasKey(l => l.Id);
            e.Property(l => l.Name).IsRequired().HasMaxLength(200);
            e.Property(l => l.Description).HasMaxLength(2000);
            e.OwnsOne(l => l.Coordinates, c =>
            {
                c.Property(x => x.Latitude).HasColumnName("Latitude").IsRequired();
                c.Property(x => x.Longitude).HasColumnName("Longitude").IsRequired();
            });
            e.HasMany(l => l.Images).WithOne(i => i.Location)
                .HasForeignKey(i => i.LocationId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(l => l.Tags).WithOne(t => t.Location)
                .HasForeignKey(t => t.LocationId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Journey>(e =>
        {
            e.HasKey(j => j.Id);
            e.Property(j => j.Name).IsRequired().HasMaxLength(200);
            e.Property(j => j.Description).HasMaxLength(2000);
        });

        modelBuilder.Entity<JourneyLocation>(e =>
        {
            e.HasKey(jl => new { jl.JourneyId, jl.LocationId });
            e.HasOne(jl => jl.Journey).WithMany(j => j.JourneyLocations)
                .HasForeignKey(jl => jl.JourneyId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(jl => jl.Location).WithMany(l => l.JourneyLocations)
                .HasForeignKey(jl => jl.LocationId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Image>(e =>
        {
            e.HasKey(i => i.Id);
            e.Property(i => i.FilePath).IsRequired();
        });

        modelBuilder.Entity<Tag>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.Name).IsRequired().HasMaxLength(100);
        });
    }
}
