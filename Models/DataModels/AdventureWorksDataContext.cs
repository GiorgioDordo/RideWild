using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace RideWild.Models.DataModels;

public partial class AdventureWorksDataContext : DbContext
{
    public AdventureWorksDataContext()
    {
    }

    public AdventureWorksDataContext(DbContextOptions<AdventureWorksDataContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AuthUser> AuthUsers { get; set; }

    public virtual DbSet<CustomerData> CustomerData { get; set; }

    public virtual DbSet<Log> Logs { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuthUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("authusers_id_primary");

            entity.Property(e => e.EmaiAddress).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.PasswordHash).HasMaxLength(256);
            entity.Property(e => e.PasswordSalt).HasMaxLength(40);

            entity.HasOne(d => d.Role).WithMany(p => p.AuthUsers)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("authusers_roleid_foreign");
        });

        modelBuilder.Entity<CustomerData>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("customerdata_id_primary");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.AddressLine).HasMaxLength(255);
            entity.Property(e => e.EmailAddress).HasMaxLength(50);
            entity.Property(e => e.PasswordHash).HasMaxLength(256);
            entity.Property(e => e.PasswordSalt).HasMaxLength(40);
            entity.Property(e => e.PhoneNumber).HasMaxLength(255);
        });

        modelBuilder.Entity<Log>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("Logs");

            entity.Property(e => e.Message).IsRequired().HasColumnType("nvarchar(max)");
            entity.Property(e => e.MessageTemplate).IsRequired().HasColumnType("nvarchar(max)");
            entity.Property(e => e.Level).HasMaxLength(128);
            entity.Property(e => e.TimeStamp).HasColumnType("datetimeoffset").IsRequired();
            entity.Property(e => e.Exception).HasColumnType("nvarchar(max)");
            entity.Property(e => e.Properties).HasColumnType("nvarchar(max)");
            entity.Property(e => e.LogEvent).HasColumnType("nvarchar(max)");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("role_id_primary");

            entity.ToTable("Role");

            entity.Property(e => e.Name).HasMaxLength(20);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
