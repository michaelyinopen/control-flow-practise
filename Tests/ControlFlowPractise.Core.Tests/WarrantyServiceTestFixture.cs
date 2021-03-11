using ControlFlowPractise.BudgetData;
using ControlFlowPractise.ComprehensiveData;
using ControlFlowPractise.Core.Tests.WarrantyServiceTestData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ControlFlowPractise.Core.Tests
{
    public class WarrantyServiceTestFixture : IDisposable
    {
        public IServiceProvider ServiceProvider { get; set; }

        public WarrantyServiceTestFixture()
        {
            var services = GetServices();

            ServiceProvider = services.BuildServiceProvider();

            using (var budgetDbContext = ServiceProvider.GetRequiredService<BudgetDataDbContext>())
            {
                budgetDbContext.Database.EnsureDeleted();
                budgetDbContext.Database.Migrate();
            }
            using (var comprehensiveDbContext = ServiceProvider.GetRequiredService<ComprehensiveDataDbContext>())
            {
                comprehensiveDbContext.Database.EnsureDeleted();
                comprehensiveDbContext.Database.Migrate();
            }
            InitializeDatabaseWithTestData();
        }

        public IServiceCollection GetServices()
        {
            var services = new ServiceCollection();
            services.AddDbContextPool<BudgetDataDbContext>(options =>
                options.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=ControlFlowPractise.TestBudgetDataDb"));
            services.AddDbContextPool<ComprehensiveDataDbContext>(options =>
                options.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=ControlFlowPractise.TestComprehensiveDataDb"));

            services.AddWarrantyService();

            return services;
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
