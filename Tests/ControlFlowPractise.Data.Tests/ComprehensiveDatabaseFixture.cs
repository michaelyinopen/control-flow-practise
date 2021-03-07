using ControlFlowPractise.ComprehensiveData;
using Microsoft.EntityFrameworkCore;
using System;

namespace ControlFlowPractise.Data.Tests
{
    public class ComprehensiveDatabaseFixture : IDisposable
    {
        public ComprehensiveDatabaseFixture()
        {
            DbContextOptions = new DbContextOptionsBuilder<ComprehensiveDataDbContext>()
                .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=ControlFlowPractise.TestComprehensiveDataDb")
                .Options;

            using var context = new ComprehensiveDataDbContext(DbContextOptions);
            context.Database.Migrate();
        }

        public void Dispose()
        {
            using var context = new ComprehensiveDataDbContext(DbContextOptions);
            context.Database.EnsureDeleted();
        }

        public DbContextOptions<ComprehensiveDataDbContext> DbContextOptions { get; private set; }
    }
}
