using ControlFlowPractise.ComprehensiveData.Models;
using Microsoft.EntityFrameworkCore;

namespace ControlFlowPractise.ComprehensiveData
{
    public class ComprehensiveDataDbContext : DbContext
    {
        public DbSet<WarrantyCaseVerification> WarrantyCaseVerification => Set<WarrantyCaseVerification>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<WarrantyCaseVerification>()
                .Property(req => req.Operation)
                .HasConversion<string>();

            modelBuilder
                .Entity<WarrantyCaseVerification>()
                .Property(req => req.FailureType)
                .HasConversion<string>();
        }

        public ComprehensiveDataDbContext(DbContextOptions<ComprehensiveDataDbContext> options)
            : base(options)
        {
        }
    }
}
