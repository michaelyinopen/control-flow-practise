using ControlFlowPractise.BudgetData;
using Microsoft.EntityFrameworkCore;
using System;

namespace ControlFlowPractise.Data.Tests
{
    public class BudgetDatabaseFixture : IDisposable
    {
        public BudgetDatabaseFixture()
        {
            DbContextOptions = new DbContextOptionsBuilder<BudgetDataDbContext>()
                  .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=ControlFlowPractise.TestBudgetDataDb")
                  .Options;

            using var context = new BudgetDataDbContext(DbContextOptions);
            context.Database.Migrate();
        }

        public void Dispose()
        {
            using var context = new BudgetDataDbContext(DbContextOptions);
            context.Database.EnsureDeleted();
        }

        public DbContextOptions<BudgetDataDbContext> DbContextOptions { get; private set; }
    }
}
