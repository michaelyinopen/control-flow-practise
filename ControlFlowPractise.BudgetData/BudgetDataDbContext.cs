using ControlFlowPractise.BudgetData.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ControlFlowPractise.BudgetData
{
    public class BudgetDataDbContext : DbContext
    {
        public DbSet<ExternalPartyRequest> ExternalPartyRequest => Set<ExternalPartyRequest>();
        public DbSet<ExternalPartyResponse> ExternalPartyResponse => Set<ExternalPartyResponse>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<ExternalPartyRequest>()
                .Property(req => req.Operation)
                .HasConversion<string>();

            modelBuilder
                .Entity<ExternalPartyRequest>()
                .Property(req => req.RequestId)
                .HasConversion<string>();

            modelBuilder
                .Entity<ExternalPartyResponse>()
                .Property(res => res.Operation)
                .HasConversion<string>();

            modelBuilder
                .Entity<ExternalPartyResponse>()
                .Property(req => req.RequestId)
                .HasConversion<string>();
        }

        public BudgetDataDbContext(DbContextOptions<BudgetDataDbContext> options)
            : base(options)
        {
        }
    }
}
