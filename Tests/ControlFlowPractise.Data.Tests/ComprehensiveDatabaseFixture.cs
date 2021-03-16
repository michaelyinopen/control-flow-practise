using ControlFlowPractise.ComprehensiveData;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Xunit;

namespace ControlFlowPractise.Data.Tests
{
    public class ComprehensiveDatabaseFixture : IAsyncLifetime
    {
        public DbContextOptions<ComprehensiveDataDbContext> DbContextOptions { get; private set; }

        public ComprehensiveDatabaseFixture()
        {
            DbContextOptions = new DbContextOptionsBuilder<ComprehensiveDataDbContext>()
                .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=ControlFlowPractise.TestComprehensiveDataDb")
                .Options;
        }

        public async Task InitializeAsync()
        {
            using var context = new ComprehensiveDataDbContext(DbContextOptions);
            await context.Database.EnsureDeletedAsync();
            await context.Database.MigrateAsync();
        }

        public async Task DisposeAsync()
        {
            using var context = new ComprehensiveDataDbContext(DbContextOptions);
            await context.Database.EnsureDeletedAsync();
        }
    }
}
