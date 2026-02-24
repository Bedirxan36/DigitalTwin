using Microsoft.EntityFrameworkCore;
using DigitalTwin.Models;
using System;
using System.IO;
using System.Linq;

namespace DigitalTwin.Data;

public class DigitalTwinDbContext : DbContext
{
    public DbSet<ActivityLog> ActivityLogs { get; set; }
    public DbSet<FocusSession> FocusSessions { get; set; }
    public DbSet<AppTag> AppTags { get; set; }
    public DbSet<DailySummary> DailySummaries { get; set; }
    public DbSet<Recommendation> Recommendations { get; set; }
    public DbSet<UserSettings> UserSettings { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "DigitalTwin",
            "digitaltwin.db"
        );
        
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
        optionsBuilder.UseSqlite($"Data Source={dbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ActivityLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.ProcessName);
            entity.Property(e => e.WindowTitle).HasMaxLength(500);
            entity.Property(e => e.ProcessName).HasMaxLength(200);
            entity.Property(e => e.Domain).HasMaxLength(200);
        });

        modelBuilder.Entity<FocusSession>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.StartTime);
            entity.Property(e => e.Goal).HasMaxLength(500);
            entity.HasMany(e => e.Activities)
                  .WithOne(a => a.FocusSession)
                  .HasForeignKey(a => a.FocusSessionId);
        });

        modelBuilder.Entity<AppTag>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ProcessName);
            entity.Property(e => e.ProcessName).HasMaxLength(200);
            entity.Property(e => e.Domain).HasMaxLength(200);
        });

        modelBuilder.Entity<DailySummary>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Date).IsUnique();
        });

        modelBuilder.Entity<Recommendation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.GeneratedAt);
            entity.Property(e => e.Title).HasMaxLength(200);
        });

        modelBuilder.Entity<UserSettings>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Key).IsUnique();
            entity.Property(e => e.Key).HasMaxLength(100);
        });
    }
}
