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

        // pre-commit verify
        //
        // request validation error
        // saves built request in budget database
        // calls external party
        // returns networkfailure
        // saves raw response in budget database
        //
        // validate response
        // fail because WarrantyResponseErrors
        // fail because other validation error
        // success
        // Operation: Create, Verify, Commit, Cancel
        // Commit sends OrderTrackingNumber
        //
        // saves warrantyProof if successful commit
        //
        // saves VerifyWarrantyCaseResponse in comprehensive database
        // success
        // failure
        //
        // saves VerifyWarrantyCaseResponse failure
        //
        // check SatisfySuccessfulCondition
        // isSuccess
        // isSuccess = false but has WarrantyCaseResponse
        // isSuccess = false and no WarrantyCaseResponse
        #region Verify

        // If success
        // BudgetDatabase ExternalPartyRequest
        // External Party Called
        // BudgetDatabase ExternalPartyResponse
        // ComprehensiveDatabase WarrantyCaseVerification
        // response
        //
        // If failure (e.g. validation of request error)
        // ComprehensiveDatabase WarrantyCaseVerification
        // response
        [Trait("accessibility", "public")]
        [Trait("database", "BudgetData")]
        [Trait("database", "ComprehensiveData")]
        [Trait("external-service", "actual")]
        [Theory]
        [MemberData(nameof(VerifyCreateTestData))]
        public async Task VerifyCreate(
            VerifyWarrantyCaseRequest request)
        {
            var warrantyService = GetWarrantyService();
            var actual = await warrantyService.Verify(request);

        }

        public static IEnumerable<object[]> VerifyCreateTestData()
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
        }

        // If success
        //
        // If failure (e.g. external party response has error)
        // BudgetDatabase ExternalPartyRequest
        // External Party Called
        // BudgetDatabase ExternalPartyResponse
        // ComprehensiveDatabase WarrantyCaseVerification
        // response
        [Trait("accessibility", "public")]
        [Trait("database", "BudgetData")]
        [Trait("database", "ComprehensiveData")]
        [Trait("external-service", "actual")]
        [Theory]
        [MemberData(nameof(VerifyVerifyTestData))]
        public async Task VerifyVerify(
            VerifyWarrantyCaseRequest request)
        {
            var warrantyService = GetWarrantyService();
            //var actual = await warrantyService.Verify();

            // 
        }

        public static IEnumerable<object[]> VerifyVerifyTestData()
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
        }

        // If success
        // BudgetDatabase ExternalPartyRequest x2
        // External Party Called x2
        // BudgetDatabase ExternalPartyResponse x2
        // ComprehensiveDatabase WarrantyCaseVerification x2
        // response
        //
        // If failure (e.g. pre-commit validation did not meet successful condition)
        // BudgetDatabase ExternalPartyRequest
        // External Party Called
        // BudgetDatabase ExternalPartyResponse
        // ComprehensiveDatabase WarrantyCaseVerification
        // response
        //
        // If failure (e.g. Commit did not meet successful condition)
        // BudgetDatabase ExternalPartyRequest x2
        // External Party Called x2
        // BudgetDatabase ExternalPartyResponse x2
        // ComprehensiveDatabase WarrantyCaseVerification x2
        // response
        [Trait("accessibility", "public")]
        [Trait("database", "BudgetData")]
        [Trait("database", "ComprehensiveData")]
        [Trait("external-service", "actual")]
        [Theory]
        [MemberData(nameof(VerifyCommitTestData))]
        public async Task VerifyCommit(
            VerifyWarrantyCaseRequest request)
        {
            var warrantyService = GetWarrantyService();
            //var actual = await warrantyService.Verify();

            // 
        }

        public static IEnumerable<object[]> VerifyCommitTestData()
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
        }

        // If success
        //
        // If failure (e.g. external party network error)
        // BudgetDatabase ExternalPartyRequest
        // External Party Called
        // ComprehensiveDatabase WarrantyCaseVerification
        // response
        [Trait("accessibility", "public")]
        [Trait("database", "BudgetData")]
        [Trait("database", "ComprehensiveData")]
        [Trait("external-service", "actual")]
        [Theory]
        [MemberData(nameof(VerifyCancelTestData))]
        public async Task VerifyCancel(
            VerifyWarrantyCaseRequest request)
        {
            var warrantyService = GetWarrantyService();
            //var actual = await warrantyService.Verify();

            // 
        }

        public static IEnumerable<object[]> VerifyCancelTestData()
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
        }
        #endregion Verify

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
