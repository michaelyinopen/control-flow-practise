using ControlFlowPractise.ComprehensiveData.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace ControlFlowPractise.ComprehensiveData
{
    public class ComprehensiveDataDbContext : DbContext
    {
        public DbSet<WarrantyCaseVerification> WarrantyCaseVerification => Set<WarrantyCaseVerification>();
        public DbSet<WarrantyProof> WarrantyProof => Set<WarrantyProof>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region WarrantyCaseVerification
            modelBuilder
                .Entity<WarrantyCaseVerification>()
                .Property(w => w.Operation)
                .HasConversion<string>();

            modelBuilder
                .Entity<WarrantyCaseVerification>()
                .Property(w => w.WarrantyCaseStatus)
                .HasConversion<string>();

            modelBuilder
                .Entity<WarrantyCaseVerification>()
                .Property(w => w.FailureType)
                .HasConversion<string>();

            modelBuilder
                .Entity<WarrantyCaseVerification>()
                .Property(w => w.DateTime)
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("GETUTCDATE()")
                .HasConversion(
                    v => v,
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            modelBuilder
                .Entity<WarrantyCaseVerification>()
                .HasIndex(w => new
                {
                    w.OrderId,
                    w.ResponseHasNoError,
                    w.FailureType,
                    w.DateTime
                });

            modelBuilder
                .Entity<WarrantyCaseVerification>()
                .HasIndex(w => new
                {
                    w.OrderId,
                    w.ResponseHasNoError,
                    w.FailureType,
                    w.Operation,
                    w.WarrantyCaseStatus,
                    w.DateTime
                });

            modelBuilder
                .Entity<WarrantyCaseVerification>()
                .HasIndex(w => new
                {
                    w.OrderId,
                    w.WarrantyCaseStatus,
                    w.ResponseHasNoError,
                    w.FailureType,
                    w.WarrantyCaseId,
                    w.Operation,
                });
            #endregion WarrantyCaseVerification

            #region WarrantyProof
            modelBuilder
                .Entity<WarrantyProof>()
                .Property(p => p.DateTime)
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("GETUTCDATE()")
                .HasConversion(
                    v => v,
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            modelBuilder
                .Entity<WarrantyProof>()
                .HasIndex(p => p.RequestId)
                .IsUnique();
            #endregion
        }

        public ComprehensiveDataDbContext(DbContextOptions<ComprehensiveDataDbContext> options)
            : base(options)
        {
        }
    }
}
