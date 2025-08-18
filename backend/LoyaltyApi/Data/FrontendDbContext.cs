using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoyaltyApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LoyaltyApi.Data
{
    public class FrontendDbContext : DbContext
    {
        public FrontendDbContext(DbContextOptions<FrontendDbContext> options) : base(options)
        {
        }

        // Parameterless constructor for design-time migrations
        public FrontendDbContext()
        {
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Use the same SQLite connection string as in Startup.cs
                optionsBuilder.UseSqlite("Data Source=DikaFrontend.db");
            }
        }
        public DbSet<Token> Tokens { get; set; }

        public DbSet<CreditPointsTransaction> CreditPointsTransactions { get; set; }

        // Commented out - functionality moved to CreditPointsTransaction
        // public DbSet<CreditPointsTransactionDetail> CreditPointsTransactionsDetails { get; set; }

        public DbSet<Restaurant> Restaurants { get; set; }

        public DbSet<Voucher> Vouchers { get; set; }
        public DbSet<Password> Passwords { get; set; }
        public DbSet<User> Users { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasKey(u => u.Id);

            modelBuilder.Entity<User>()
                .Property(u => u.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.RestaurantId);

            modelBuilder.Entity<Restaurant>()
                .HasKey(r => r.RestaurantId);

            modelBuilder.Entity<Token>()
                .HasKey(x => new { x.CustomerId, x.RestaurantId, x.TokenValue });

            modelBuilder.Entity<Token>()
                .Property(x => x.TokenType)
                .HasConversion<string>();

            modelBuilder.Entity<Voucher>()
                .HasKey(x => x.ShortCode);

            modelBuilder.Entity<Voucher>()
                .Property(v => v.IsUsed)
                .HasDefaultValue(false);

            modelBuilder.Entity<CreditPointsTransaction>()
                .HasKey(p => p.TransactionId);

            modelBuilder.Entity<CreditPointsTransaction>()
                .Property(p => p.TransactionId)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<CreditPointsTransaction>()
                .HasIndex(p => p.ReceiptId);

            // Add index on EarnTransactionId for better performance
            modelBuilder.Entity<CreditPointsTransaction>()
                .HasIndex(p => p.EarnTransactionId);

            // Commented out CreditPointsTransactionDetail configurations
            // modelBuilder.Entity<CreditPointsTransactionDetail>()
            //     .HasKey(d => d.DetailId);
            //
            // modelBuilder.Entity<CreditPointsTransactionDetail>()
            //     .Property(d => d.DetailId)
            //     .ValueGeneratedOnAdd();
            //
            // modelBuilder.Entity<CreditPointsTransactionDetail>()
            //     .HasOne(d => d.Transaction)
            //     .WithMany(p => p.CreditPointsTransactionDetails)
            //     .HasForeignKey(d => d.TransactionId);
            //
            // modelBuilder.Entity<CreditPointsTransaction>()
            //     .HasMany(p => p.CreditPointsTransactionDetails)
            //     .WithOne(d => d.Transaction)
            //     .HasForeignKey(d => d.TransactionId);
            //
            // modelBuilder.Entity<CreditPointsTransactionDetail>()
            //     .HasOne(d => d.EarnTransaction)
            //     .WithMany()
            //     .HasForeignKey(d => d.EarnTransactionId);

            modelBuilder.Entity<CreditPointsTransaction>()
                .Property(p => p.TransactionType)
                .HasConversion<string>(); // Store enum as string in the database

            modelBuilder.Entity<Password>()
            .HasKey(p => new { p.CustomerId, p.RestaurantId });

            modelBuilder.Entity<Password>()
            .Property(p => p.ConfirmedEmail)
            .HasDefaultValue(false);



            // No need for additional index since the composite key already creates one

            base.OnModelCreating(modelBuilder);
        }
    }
}