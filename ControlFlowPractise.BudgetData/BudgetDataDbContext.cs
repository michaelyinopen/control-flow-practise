using ControlFlowPractise.BudgetData.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace ControlFlowPractise.BudgetData
{
    public class BudgetDataDbContext : DbContext
    {
        public DbSet<ExternalPartyRequest> ExternalPartyRequest => Set<ExternalPartyRequest>();
        public DbSet<ExternalPartyResponse> ExternalPartyResponse => Set<ExternalPartyResponse>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ExternalPartyRequest
            modelBuilder
                .Entity<ExternalPartyRequest>()
                .Property(req => req.DateTime)
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("GETUTCDATE()")
                .HasConversion(
                    v => v,
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            modelBuilder
                .Entity<ExternalPartyRequest>()
                .Property(req => req.Operation)
                .HasConversion<string>();

            modelBuilder
                .Entity<ExternalPartyRequest>()
                .HasIndex(req => new { req.OrderId, req.RequestId })
                .IsUnique();

            // ExternalPartyResponse
            modelBuilder
                .Entity<ExternalPartyResponse>()
                .Property(req => req.DateTime)
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("GETUTCDATE()")
                .HasConversion(
                    v => v,
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            modelBuilder
                .Entity<ExternalPartyResponse>()
                .Property(res => res.Operation)
                .HasConversion<string>();

            modelBuilder
                .Entity<ExternalPartyResponse>()
                .HasIndex(req => new { req.OrderId, req.RequestId })
                .IsUnique();
        }

        public BudgetDataDbContext(DbContextOptions<BudgetDataDbContext> options)
            : base(options)
        {
        }
    }
}
