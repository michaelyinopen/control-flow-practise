using ControlFlowPractise.BudgetData;
using ControlFlowPractise.ComprehensiveData;
using ControlFlowPractise.Core.Tests.WarrantyServiceTestSetups;
using ControlFlowPractise.ExternalParty;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace ControlFlowPractise.Core.Tests
{
    public class WarrantyServiceTestFixture : IAsyncLifetime
    {
        public IServiceProvider ServiceProvider { get; set; }

        public WarrantyServiceTestFixture()
        {
            var services = GetServices();

            ServiceProvider = services.BuildServiceProvider();
        }

        public IServiceCollection GetServices()
        {
            var services = new ServiceCollection();
            services.AddDbContextPool<BudgetDataDbContext>(options =>
                options.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=ControlFlowPractise.TestBudgetDataDb"));
            services.AddDbContextPool<ComprehensiveDataDbContext>(options =>
                options.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=ControlFlowPractise.TestComprehensiveDataDb"));

            var mockedExternalPartyProxy = new Mock<IExternalPartyProxy>(MockBehavior.Strict);
            services.AddScoped(_ => mockedExternalPartyProxy.Object);

            services.AddWarrantyService();

            return services;
        }

        public async Task InitializeAsync()
        {
            Task createBudgetDatabaseTask = CreateBudgetDatabase(ServiceProvider);
            static async Task CreateBudgetDatabase(IServiceProvider serviceProvider)
            {
                using var budgetDbContext = serviceProvider.GetRequiredService<BudgetDataDbContext>();
                await budgetDbContext.Database.EnsureDeletedAsync();
                await budgetDbContext.Database.MigrateAsync();
            }

            Task createComprehensiveDatabaseTask = CreateComprehensiveDatabase(ServiceProvider);
            static async Task CreateComprehensiveDatabase(IServiceProvider serviceProvider)
            {
                using var comprehensiveDbContext = serviceProvider.GetRequiredService<ComprehensiveDataDbContext>();
                await comprehensiveDbContext.Database.EnsureDeletedAsync();
                await comprehensiveDbContext.Database.MigrateAsync();
            }

            await Task.WhenAll(createBudgetDatabaseTask, createComprehensiveDatabaseTask);

            await InitializeDatabaseWithTestData();
        }

        private async Task InitializeDatabaseWithTestData()
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
            using (var stream = assembly.GetManifestResourceStream("ControlFlowPractise.Core.Tests.WarrantyServiceTestSetups.VerifySetups.json")!)
            using (var reader = new StreamReader(stream))
            {
                JsonSerializer serializer = new JsonSerializer();
                testData.AddRange(
                    (List<TestSetup>)serializer.Deserialize(reader, typeof(List<TestSetup>))!);
            }

            var externalPartyRequests = testData.SelectMany(d => d.ExternalPartyRequests).ToList();
            var externalPartyResponses = testData.SelectMany(d => d.ExternalPartyResponses).ToList();

            using (var budgetDbContext = ServiceProvider.GetRequiredService<BudgetDataDbContext>())
            {
                budgetDbContext.ExternalPartyRequest.AddRange(externalPartyRequests);
                budgetDbContext.ExternalPartyResponse.AddRange(externalPartyResponses);
                budgetDbContext.SaveChanges();
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

        public async Task DisposeAsync()
        {
            using var budgetDbContext = ServiceProvider.GetRequiredService<BudgetDataDbContext>();
            using var comprehensiveDbContext = ServiceProvider.GetRequiredService<ComprehensiveDataDbContext>();
            Task deleteBudgetDatabaseTask = budgetDbContext.Database.EnsureDeletedAsync();
            Task deleteComprehensiveDatabaseTask = comprehensiveDbContext.Database.EnsureDeletedAsync();

            await Task.WhenAll(deleteBudgetDatabaseTask, deleteComprehensiveDatabaseTask);
        }
    }
}
