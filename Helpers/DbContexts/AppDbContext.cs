using ExpenseSplitter.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExpenseSplitter.Helpers.DbContexts
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Participant> Participants { get; set; } = null!;
        public DbSet<Expense> Expenses { get; set; } = null!;
        public DbSet<ExpenseShare> ExpenseShares { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            #region Participant Configuration
            modelBuilder.Entity<Participant>(entity =>
            {
                entity.ToTable("Participants");
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Name).IsRequired().HasMaxLength(100);
            });
            #endregion

            #region Expense Configuration
            modelBuilder.Entity<Expense>(entity =>
            {
                entity.ToTable("Expenses");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Amount).HasPrecision(18, 2);

                entity.HasOne(e => e.Payer)
                      .WithMany(p => p.PaidExpenses)
                      .HasForeignKey(e => e.PayerId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
            #endregion

            #region ExpenseShare Configuration
            modelBuilder.Entity<ExpenseShare>(entity =>
            {
                entity.ToTable("ExpenseShares");
                entity.HasKey(s => s.Id);
                entity.Property(s => s.ShareAmount).HasPrecision(18, 2);

                entity.HasOne(s => s.Expense)
                      .WithMany(e => e.ExpenseShares)
                      .HasForeignKey(s => s.ExpenseId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(s => s.Participant)
                      .WithMany(p => p.ExpenseShares)
                      .HasForeignKey(s => s.ParticipantId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
            #endregion
        }
    }
}
