using ControlFlowPractise.Common;
using ControlFlowPractise.ComprehensiveData;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ControlFlowPractise.Data.Tests
{
    [Trait("database", "ComprehensiveData")]
    public class ComprehensiveDataTests : IClassFixture<ComprehensiveDatabaseFixture>
    {
        public DbContextOptions<ComprehensiveDataDbContext> DbContextOptions { get; }

        public ComprehensiveDataTests(ComprehensiveDatabaseFixture fixture)
        {
            DbContextOptions = fixture.DbContextOptions;
        }

        [Fact]
        public void CanCreateAndDropDatabase()
        {
            using var context = new ComprehensiveDataDbContext(DbContextOptions);
        }
    }
}
