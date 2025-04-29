using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Postgres.Scaffolding;

public partial class MyDbContext : DbContext
{
    public MyDbContext(DbContextOptions<MyDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<Weather> Weathers { get; set; }
    public virtual DbSet<UserSettings> UserSettings { get; set; }
    public virtual DbSet<Plant> Plants { get; set; }
    public virtual DbSet<UserPlant> UserPlants { get; set; }
    public virtual DbSet<Alert> Alerts { get; set; }
    public virtual DbSet<SensorHistory> SensorHistories { get; set; }
    public virtual DbSet<UserDevice> UserDevices { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("meetyourplants");

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId);

            entity.ToTable("User");

            entity.Property(e => e.UserId).HasColumnName("UserId");
            entity.Property(e => e.FirstName).HasColumnName("FirstName");
            entity.Property(e => e.LastName).HasColumnName("LastName");
            entity.Property(e => e.Email).HasColumnName("Email").IsRequired();
            entity.Property(e => e.Birthday).HasColumnName("Birthday");
            entity.Property(e => e.Country).HasColumnName("Country");
            entity.Property(e => e.Hash).HasColumnName("Hash");
            entity.Property(e => e.Salt).HasColumnName("Salt");
            entity.Property(e => e.Role).HasColumnName("Role");
        });

        modelBuilder.Entity<Weather>(entity =>
        {
            entity.HasKey(e => e.UserId);

            entity.ToTable("Weather");

            entity.Property(e => e.UserId).HasColumnName("UserId");
            entity.Property(e => e.City).HasColumnName("City");
            entity.Property(e => e.Country).HasColumnName("Country");

            entity.HasOne(e => e.User)
                .WithOne(u => u.Weather)
                .HasForeignKey<Weather>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserSettings>(entity =>
        {
            entity.HasKey(e => e.UserId);

            entity.ToTable("UserSettings");

            entity.Property(e => e.UserId).HasColumnName("UserId");
            entity.Property(e => e.Celsius).HasColumnName("Celsius");
            entity.Property(e => e.DarkTheme).HasColumnName("DarkTheme");
            entity.Property(e => e.ConfirmDialog).HasColumnName("ConfirmDialog");
            entity.Property(e => e.SecretMode).HasColumnName("SecretMode");
            entity.Property(e => e.WaitTime).HasColumnName("WaitTime");

            entity.HasOne(e => e.User)
                .WithOne(u => u.UserSettings)
                .HasForeignKey<UserSettings>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Plant>(entity =>
        {
            entity.HasKey(e => e.PlantID);

            entity.ToTable("Plant");

            entity.Property(e => e.PlantID).HasColumnName("PlantID");
            entity.Property(e => e.Planted).HasColumnName("Planted");
            entity.Property(e => e.PlantName).HasColumnName("PlantName");
            entity.Property(e => e.PlantType).HasColumnName("PlantType");
            entity.Property(e => e.PlantNotes).HasColumnName("PlantNotes");
            entity.Property(e => e.LastWatered).HasColumnName("LastWatered");
            entity.Property(e => e.WaterEvery).HasColumnName("WaterEvery");
            entity.Property(e => e.IsDead).HasColumnName("IsDead");
        });

        modelBuilder.Entity<UserPlant>(entity =>
        {
            entity.HasKey(e => new { e.UserID, e.PlantID });

            entity.ToTable("UserPlant");

            entity.Property(e => e.UserID).HasColumnName("UserID");
            entity.Property(e => e.PlantID).HasColumnName("PlantID");

            entity.HasOne(e => e.User)
                .WithMany(u => u.UserPlants)
                .HasForeignKey(e => e.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Plant)
                .WithMany(p => p.UserPlants)
                .HasForeignKey(e => e.PlantID)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Alert>(entity =>
        {
            entity.HasKey(e => e.AlertID);

            entity.ToTable("Alert");

            entity.Property(e => e.AlertID).HasColumnName("AlertID");
            entity.Property(e => e.AlertUserId).HasColumnName("AlertUserId");
            entity.Property(e => e.AlertName).HasColumnName("AlertName");
            entity.Property(e => e.AlertDesc).HasColumnName("AlertDesc");
            entity.Property(e => e.AlertTime).HasColumnName("AlertTime");
            entity.Property(e => e.AlertPlant).HasColumnName("AlertPlant");

            entity.HasOne(e => e.User)
                .WithMany(u => u.Alerts)
                .HasForeignKey(e => e.AlertUserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Plant)
                .WithMany(p => p.Alerts)
                .HasForeignKey(e => e.AlertPlant)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<SensorHistory>(entity =>
        {
            entity.HasKey(e => e.SensorHistoryId);

            entity.ToTable("SensorHistory");
            
            entity.Property(e => e.SensorHistoryId).HasColumnName("SensorHistoryId");
            entity.Property(e => e.DeviceId).HasColumnName("DeviceId");
            entity.Property(e => e.Temperature).HasColumnName("Temperature");
            entity.Property(e => e.Humidity).HasColumnName("Humidity");
            entity.Property(e => e.AirPressure).HasColumnName("AirPressure");
            entity.Property(e => e.AirQuality).HasColumnName("AirQuality");
            entity.Property(e => e.Time).HasColumnName("Time");
            
            entity.HasOne(e => e.UserDevice)
                .WithMany(ud => ud.SensorHistories)
                .HasForeignKey(e => e.DeviceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserDevice>(entity =>
        {
            entity.HasKey(e => e.DeviceId);

            entity.ToTable("UserDevice");
            
            entity.Property(e => e.DeviceId).HasColumnName("DeviceId");
            entity.Property(e => e.UserId).HasColumnName("UserId");
            entity.Property(e => e.DeviceName).HasColumnName("DeviceName");
            entity.Property(e => e.DeviceDescription).HasColumnName("DeviceDescription");
            entity.Property(e => e.CreatedAt).HasColumnName("CreatedAt");
            
            entity.HasOne(e => e.User)
                .WithMany(u => u.UserDevices)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasMany(e => e.SensorHistories)
                .WithOne(sh => sh.UserDevice)
                .HasForeignKey(sh => sh.DeviceId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}