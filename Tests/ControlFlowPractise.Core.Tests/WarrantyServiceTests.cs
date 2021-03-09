using ControlFlowPractise.Common;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ControlFlowPractise.Core.Tests
{
    public class WarrantyServiceTests : IClassFixture<WarrantyServiceTestFixture>, IDisposable
    {
        private IServiceScope ServiceScope { get; }
        private IServiceProvider ScopedServiceProvider { get; }

        public WarrantyServiceTests(WarrantyServiceTestFixture fixture)
        {
            var serviceProvider = fixture.ServiceProvider;
            ServiceScope = serviceProvider.CreateScope();
            ScopedServiceProvider = ServiceScope.ServiceProvider;
        }

        private IWarrantyService GetWarrantyService()
        {
            return ScopedServiceProvider.GetRequiredService<IWarrantyService>();
        }

        [Trait("accessibility", "internal")]
        [Fact]
        public void CanCreateWarrantyService()
        {
            var warrantyService = GetWarrantyService();
        }

        [Trait("accessibility", "public")]
        [Trait("database", "BudgetData")]
        [Trait("database", "ComprehensiveData")]
        [Trait("external-service", "actual")]
        [Fact]
        public async Task Verify()
        {
            var warrantyService = GetWarrantyService();
            // request validation error, immidiately returns, assert response
            // saves built request in budget database
            // calls external party
            // returns networkfailure
            // saves raw response in budget database

            // validate response
            // fail because WarrantyResponseErrors
            // fail because other validation error
            // success
            // Operation: Create, Verify, Commit, Cancel
            // Commit sends OrderTrackingNumber

            // saves warrantyProof if successful commit

            // saves VerifyWarrantyCaseResponse in comprehensive database
            // success
            // failure

            // saves VerifyWarrantyCaseResponse failure

            // check SatisfySuccessfulCondition
            // isSuccess
            // isSuccess = false but has WarrantyCaseResponse
            // isSuccess = false and no WarrantyCaseResponse
        }

        // not found (failure type + IsNotFound)
        // found: not choose other orders (orderId)
        // found: last dateTime
        // found: ResponseHasNoError
        // found: no failure
        // Operation: Create, Verify, Commit, Cancel
        // deserializeResponse: success
        // deserializeResponse: fail
        [Trait("accessibility", "public")]
        [Trait("database", "ComprehensiveData")]
        [Theory]
        [MemberData(nameof(GetCurrentWarrantyCaseVerificationTestData))]
        public async Task GetCurrentWarrantyCaseVerification(string orderId, GetCurrentWarrantyCaseVerificationResponse expected)
        {
            var warrantyService = GetWarrantyService();
            var actual = await warrantyService.GetCurrentWarrantyCaseVerification(orderId);

            Assert.Equal(expected.IsSuccess, actual.IsSuccess);
            Assert.Equal(expected.FailureType, actual.FailureType);
            Assert.Equal(expected.IsNotFound, actual.IsNotFound);
            Assert.Equal(expected.FailureMessage is null, actual.FailureMessage is null);
            actual.WarrantyCaseResponse.Should().BeEquivalentTo(expected.WarrantyCaseResponse);
        }

        public static IEnumerable<object[]> GetCurrentWarrantyCaseVerificationTestData()
        {
            yield return new object[]
            {
                "get-x",
                new GetCurrentWarrantyCaseVerificationResponse
                {
                    IsSuccess = false,
                    FailureType = FailureType.GetWarrantyCaseVerificationFailure,
                    IsNotFound = true,
                    FailureMessage = "Some failure message"
                }
            };
            yield return new object[]
            {
                "get-1",
                new GetCurrentWarrantyCaseVerificationResponse
                {
                    IsSuccess = true,
                    WarrantyCaseResponse = new WarrantyCaseResponse("get-1", "896")
                    {
                        Operation = WarrantyCaseOperation.Verify,
                        WarrantyCaseStatus = WarrantyCaseStatus.Claimed,
                        Conformance = false,
                        WarrantyEstimatedAmount = 50,
                        WarrantyAmount = null,
                        ConformanceMessages = new List<WarrantyConformanceMessage>
                        {
                            new WarrantyConformanceMessage("Please wait until certified.")
                            {
                                Level = WarrantyConformanceLevel.Information
                            }
                        }
                    },
                    FailureType = null,
                    IsNotFound = null,
                    FailureMessage = null
                }
            };
        }

        // WarrantyCaseVerification no Commit found (failure type + IsNotFound)
        // ignore failure commits
        // ignore commits that are not successful (caseStatus did not update to committed)
        // WarrantyCaseVerification not found (failure type + IsNotFound)
        // Warranty Proof found
        // Warranty Proof not found
        [Trait("accessibility", "public")]
        [Trait("database", "ComprehensiveData")]
        [Theory]
        [MemberData(nameof(GetWarrantyProofTestData))]
        public async Task GetWarrantyProof(string orderId, GetWarrantyProofResponse expected)
        {
            var warrantyService = GetWarrantyService();
            var actual = await warrantyService.GetWarrantyProof(orderId);

            Assert.Equal(expected.IsSuccess, actual.IsSuccess);
            Assert.Equal(expected.OrderId, actual.OrderId);
            Assert.Equal(expected.WarrantyCaseId, actual.WarrantyCaseId);
            Assert.Equal(expected.RequestId, actual.RequestId);
            Assert.Equal(expected.WarrantyProof, actual.WarrantyProof);
            Assert.Equal(expected.FailureType, actual.FailureType);
            Assert.Equal(expected.IsNotFound, actual.IsNotFound);
            Assert.Equal(expected.FailureMessage is null, actual.FailureMessage is null);
        }

        public static IEnumerable<object[]> GetWarrantyProofTestData()
        {
            yield return new object[]
            {
                "get-proof-x",
                new GetWarrantyProofResponse("get-proof-x")
                {
                    IsSuccess = false,
                    RequestId = Guid.Parse("7412419a-159e-46f6-8e2f-e124297e20c0"),
                    FailureType = FailureType.GetWarrantyProofFailure,
                    IsNotFound = true,
                    FailureMessage = "Some failure message"
                }
            };
            yield return new object[]
            {
                "get-proof-1",
                new GetWarrantyProofResponse("get-proof-1")
                {
                    IsSuccess = true,
                    WarrantyCaseId = "675",
                    RequestId = Guid.Parse("f377da09-b602-4367-93b4-78e50c712097"),
                    WarrantyProof = "get-proof-1:311720(545)"
                }
            };
        }

        public void Dispose()
        {
            ServiceScope.Dispose();
        }
    }
}
