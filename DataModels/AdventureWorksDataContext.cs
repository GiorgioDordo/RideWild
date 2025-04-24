using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace RideWild.DataModels;
// commento
// new comment
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

    public virtual DbSet<LogError> LogErrors { get; set; }

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

        modelBuilder.Entity<LogError>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("logerror_id_primary");

            entity.ToTable("LogError");

            entity.Property(e => e.ActionName).HasMaxLength(50);
            entity.Property(e => e.ClassName).HasMaxLength(255);
            entity.Property(e => e.Message).HasColumnType("text");
            entity.Property(e => e.MethodName).HasMaxLength(50);
            entity.Property(e => e.Time).HasColumnType("datetime");
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
