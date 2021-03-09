using ControlFlowPractise.BudgetData;
using ControlFlowPractise.ComprehensiveData;
using ControlFlowPractise.Core.Tests.WarrantyServiceTestData;
using ControlFlowPractise.ExternalParty;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

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
            InitializeDatabaseWithTestData();
        }

        public void InitializeDatabaseWithTestData()
        {
            List<TestSetup> testData = new List<TestSetup>();
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream("ControlFlowPractise.Core.Tests.WarrantyServiceTestSetups.GetCurrentWarrantyCaseVerificationSetups.json")!)
            using (var reader = new StreamReader(stream))
            {
                JsonSerializer serializer = new JsonSerializer();
                testData.AddRange(
                    (List<TestSetup>)serializer.Deserialize(reader, typeof(List<TestSetup>))!);
            }
            using (var stream = assembly.GetManifestResourceStream("ControlFlowPractise.Core.Tests.WarrantyServiceTestSetups.GetWarrantyProofSetups.json")!)
            using (var reader = new StreamReader(stream))
            {
                JsonSerializer serializer = new JsonSerializer();
                testData.AddRange(
                    (List<TestSetup>)serializer.Deserialize(reader, typeof(List<TestSetup>))!);
            }

            var warrantyCaseVerificationGroups = testData
                .SelectMany(d => d.WarrantyCaseVerificationTestSetups)
                .GroupBy(vd => vd.InsertOrder)
                .OrderBy(g => g.Key)
                .ToList();
            var warrantyProofs = testData.SelectMany(d => d.WarrantyProofs).ToList();

            using (var comprehensiveDbContext = ServiceProvider.GetRequiredService<ComprehensiveDataDbContext>())
            {
                foreach (var group in warrantyCaseVerificationGroups)
                {
                    var warrantyCaseVerifications = group.Select(g => g.WarrantyCaseVerification).ToList();
                    comprehensiveDbContext.WarrantyCaseVerification.AddRange(warrantyCaseVerifications);
                    comprehensiveDbContext.SaveChanges();
                }
                comprehensiveDbContext.AddRange(warrantyProofs);
                comprehensiveDbContext.SaveChanges();
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
