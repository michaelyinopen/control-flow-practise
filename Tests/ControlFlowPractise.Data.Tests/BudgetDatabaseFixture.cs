using ControlFlowPractise.BudgetData;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Xunit;

namespace ControlFlowPractise.Data.Tests
{
    public class BudgetDatabaseFixture : IAsyncLifetime
    {
        public DbContextOptions<BudgetDataDbContext> DbContextOptions { get; private set; }

        public BudgetDatabaseFixture()
        {
            DbContextOptions = new DbContextOptionsBuilder<BudgetDataDbContext>()
                .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=ControlFlowPractise.TestBudgetDataDb")
                .Options;
        }

        public async Task InitializeAsync()
        {
            using var context = new BudgetDataDbContext(DbContextOptions);
            await context.Database.EnsureDeletedAsync();
            await context.Database.MigrateAsync();
        }

        public async Task DisposeAsync()
        {
            using var context = new BudgetDataDbContext(DbContextOptions);
            await context.Database.EnsureDeletedAsync();
        }
    }
}
