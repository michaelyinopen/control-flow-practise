using ControlFlowPractise.BudgetData;
using ControlFlowPractise.ComprehensiveData;
using ControlFlowPractise.ExternalParty;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace ControlFlowPractise.Core.Tests
{
    public class WarrantyServiceTestFixture : IDisposable
    {
        public IServiceProvider ServiceProvider { get; set; }

        public WarrantyServiceTestFixture()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddDbContextPool<BudgetDataDbContext>(options =>
                options.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=ControlFlowPractise.TestBudgetDataDb"));
            services.AddDbContextPool<ComprehensiveDataDbContext>(options =>
                options.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=ControlFlowPractise.TestComprehensiveDataDb"));

            services.AddScoped<IExternalPartyProxy, ExternalPartyProxy>();

            services.AddWarrantyService();

            ServiceProvider = services.BuildServiceProvider();

            using (var budgetDbContext = ServiceProvider.GetRequiredService<BudgetDataDbContext>())
            {
                budgetDbContext.Database.Migrate();
            }
            using (var comprehensiveDbContext = ServiceProvider.GetRequiredService<ComprehensiveDataDbContext>())
            {
                comprehensiveDbContext.Database.Migrate();
            }
        }

        public void Dispose()
        {
            using (var budgetDbContext = ServiceProvider.GetRequiredService<BudgetDataDbContext>())
            {
                budgetDbContext.Database.EnsureDeleted();
            }
            using (var comprehensiveDbContext = ServiceProvider.GetRequiredService<ComprehensiveDataDbContext>())
            {
                comprehensiveDbContext.Database.EnsureDeleted();
            }
        }
    }
}
