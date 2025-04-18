using Microsoft.EntityFrameworkCore;

namespace RideWild.Models
{
    public class AdventureWorkDataContext : DbContext
    {
        public AdventureWorkDataContext(DbContextOptions<AdventureWorkDataContext> options)
            : base(options)
        {
        }

        public DbSet<CustomerData> CustomerData { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Calls the base class implementation of OnModelCreating to ensure any configurations
            // defined in the base DbContext class are applied.
            base.OnModelCreating(modelBuilder);

            // Configures the CustomerData entity to have an index on the Email property.
            // This helps improve query performance when searching by Email.
            modelBuilder.Entity<CustomerData>()
                // Specifies that the index on the Email property must enforce uniqueness,
                // ensuring no two records in the database can have the same Email value.
                .HasIndex(c => c.Email)
                .IsUnique();
        }

    }
}
