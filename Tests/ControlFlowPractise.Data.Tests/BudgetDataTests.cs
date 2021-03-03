using ControlFlowPractise.BudgetData;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Xunit;

namespace ControlFlowPractise.Data.Tests
{
    public class BudgetDataTests : IClassFixture<BudgetDatabaseFixture>
    {
        public DbContextOptions<BudgetDataDbContext> DbContextOptions { get; }

        public BudgetDataTests(BudgetDatabaseFixture fixture)
        {
            DbContextOptions = fixture.DbContextOptions;
        }

        [Fact]
        public void CanCreateAndDropDatabase()
        {
            using var context = new BudgetDataDbContext(DbContextOptions);
        }

        // Can add
        // Can read after add
        // - count
        // - has date as utc
        // - operation works
        // x request x response
        [Fact]
        public async Task CanReadExternalPartyRequestCount()
        {
            using var context = new BudgetDataDbContext(DbContextOptions);
            var result = await context.ExternalPartyRequest.CountAsync();
        }
    }
}
