using Microsoft.EntityFrameworkCore;
using LoyaltyApi.Models;

namespace LoyaltyApi.Data
{
    public class RockDbContext : DbContext
    {
        public RockDbContext(DbContextOptions<RockDbContext> options) : base(options)
        {
        }

        // Parameterless constructor for design-time migrations
        public RockDbContext()
        {
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Use the same SQLite connection string as in Startup.cs
                optionsBuilder.UseSqlite("Data Source=DikaRockDbContext.db");
            }
        }

        public DbSet<CreditPointsTransaction> CreditPointsTransactions { get; set; }

        public DbSet<CreditPointsTransactionDetail> CreditPointsTransactionsDetails { get; set; }

        public DbSet<Restaurant> Restaurants { get; set; }

        public DbSet<Voucher> Vouchers { get; set; }
        public DbSet<Password> Passwords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Restaurant>()
                .HasKey(r => r.RestaurantId);


            modelBuilder.Entity<Voucher>()
                .HasKey(x => x.ShortCode);

            modelBuilder.Entity<Voucher>()
                .Property(v => v.IsUsed)
                .HasDefaultValue(false);

            modelBuilder.Entity<Voucher>()
                .HasIndex(v => new { v.CustomerId, v.RestaurantId });

            modelBuilder.Entity<CreditPointsTransaction>()
                .HasKey(p => p.TransactionId);

            modelBuilder.Entity<CreditPointsTransaction>()
                .HasIndex(p => p.ReceiptId);

            modelBuilder.Entity<CreditPointsTransaction>()
                .HasIndex(p => new { p.RestaurantId, p.CustomerId });

            modelBuilder.Entity<CreditPointsTransaction>()
                .Property(p => p.TransactionId)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<CreditPointsTransactionDetail>()
                .HasKey(d => d.DetailId);

            modelBuilder.Entity<CreditPointsTransactionDetail>()
                .HasIndex(d => d.TransactionId);

            modelBuilder.Entity<CreditPointsTransactionDetail>()
                .Property(d => d.DetailId)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<CreditPointsTransactionDetail>()
                .HasOne(d => d.Transaction)
                .WithMany(p => p.CreditPointsTransactionDetails)
                .HasForeignKey(d => d.TransactionId);

            modelBuilder.Entity<CreditPointsTransaction>()
                .HasMany(p => p.CreditPointsTransactionDetails)
                .WithOne(d => d.Transaction)
                .HasForeignKey(d => d.TransactionId);

            modelBuilder.Entity<CreditPointsTransactionDetail>()
                .HasOne(d => d.EarnTransaction)
                .WithMany()
                .HasForeignKey(d => d.EarnTransactionId);

            modelBuilder.Entity<CreditPointsTransaction>()
                .Property(p => p.TransactionType)
                .HasConversion<string>();

            modelBuilder.Entity<Password>()
            .HasKey(p => new { p.CustomerId, p.RestaurantId });

            modelBuilder.Entity<Password>()
            .Property(p => p.ConfirmedEmail)
            .HasDefaultValue(false);

            base.OnModelCreating(modelBuilder);
        }
    }
}