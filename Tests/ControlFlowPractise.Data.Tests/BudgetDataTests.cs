using ControlFlowPractise.BudgetData;
using ControlFlowPractise.BudgetData.Models;
using ControlFlowPractise.Common;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ControlFlowPractise.Data.Tests
{
    [Trait("database", "BudgetData")]
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

        [Fact]
        public async Task CanAddExternalPartyRequest()
        {
            using var context = new BudgetDataDbContext(DbContextOptions);
            context.ExternalPartyRequest.Add(new ExternalPartyRequest(
                orderId: "1",
                request: "{}")
            {
                Operation = WarrantyCaseOperation.Create,
                RequestId = Guid.NewGuid()
            });
            await context.SaveChangesAsync();
        }

        [Fact]
        public async Task CanCountExternalPartyRequest()
        {
            using var context = new BudgetDataDbContext(DbContextOptions);
            var count = await context.ExternalPartyRequest.CountAsync();
        }

        [Fact]
        public async Task CanReadExternalPartyRequestByOrderIdAndRequestId()
        {
            string orderId = "2";
            Guid requestId = Guid.NewGuid();
            using (var addingContext = new BudgetDataDbContext(DbContextOptions))
            {
                addingContext.ExternalPartyRequest.Add(new ExternalPartyRequest(
                    orderId,
                    request: "{}")
                {
                    Operation = WarrantyCaseOperation.Verify,
                    RequestId = requestId
                });
                await addingContext.SaveChangesAsync();
            }
            using (var readingContext = new BudgetDataDbContext(DbContextOptions))
            {
                var actual = await readingContext.ExternalPartyRequest
                    .Where(req => req.OrderId == orderId)
                    .Where(req => req.RequestId == requestId)
                    .FirstOrDefaultAsync();
                Assert.Equal(orderId, actual.OrderId);
                Assert.Equal(requestId, actual.RequestId);
                Assert.Equal(WarrantyCaseOperation.Verify, actual.Operation);
                Assert.Equal(DateTimeKind.Utc, actual.DateTime.Kind);
            }
        }

        [Fact]
        public async Task CanAddExternalPartyResponse()
        {
            using var context = new BudgetDataDbContext(DbContextOptions);
            context.ExternalPartyResponse.Add(new ExternalPartyResponse(
                orderId: "1",
                response: "{}")
            {
                Operation = WarrantyCaseOperation.Create,
                RequestId = Guid.NewGuid()
            });
            await context.SaveChangesAsync();
        }

        [Fact]
        public async Task CanCountExternalPartyResponse()
        {
            using var context = new BudgetDataDbContext(DbContextOptions);
            var count = await context.ExternalPartyResponse.CountAsync();
        }

        [Fact]
        public async Task CanReadAddedExternalPartyResponse()
        {
            string orderId = "2";
            Guid requestId = Guid.NewGuid();
            using (var addingContext = new BudgetDataDbContext(DbContextOptions))
            {
                addingContext.ExternalPartyResponse.Add(new ExternalPartyResponse(
                    orderId,
                    response: "{}")
                {
                    Operation = WarrantyCaseOperation.Verify,
                    RequestId = requestId
                });
                await addingContext.SaveChangesAsync();
            }
            using (var readingContext = new BudgetDataDbContext(DbContextOptions))
            {
                var actual = await readingContext.ExternalPartyResponse
                    .Where(req => req.OrderId == orderId)
                    .Where(req => req.RequestId == requestId)
                    .FirstOrDefaultAsync();
                Assert.Equal(orderId, actual.OrderId);
                Assert.Equal(requestId, actual.RequestId);
                Assert.Equal(WarrantyCaseOperation.Verify, actual.Operation);
                Assert.Equal(DateTimeKind.Utc, actual.DateTime.Kind);
            }
        }
    }
}
