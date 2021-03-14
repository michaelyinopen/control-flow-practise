using ControlFlowPractise.BudgetData;
using ControlFlowPractise.BudgetData.Models;
using ControlFlowPractise.Common;
using ControlFlowPractise.ComprehensiveData;
using ControlFlowPractise.ComprehensiveData.Models;
using ControlFlowPractise.Core.Tests.WarrantyServiceTestSetups;
using ControlFlowPractise.ExternalParty;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ControlFlowPractise.Core.Tests
{
    public class WarrantyServiceTests : IClassFixture<WarrantyServiceTestFixture>, IDisposable
    {
        private IServiceScope ServiceScope { get; }
        private IServiceProvider ScopedServiceProvider { get; }
        public Func<IServiceCollection> GetServices { get; }

        public WarrantyServiceTests(WarrantyServiceTestFixture fixture)
        {
            var serviceProvider = fixture.ServiceProvider;
            ServiceScope = serviceProvider.CreateScope();
            ScopedServiceProvider = ServiceScope.ServiceProvider;

            GetServices = fixture.GetServices;
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

        [Trait("accessibility", "internal")]
        [Fact]
        public void CanCreateWarrantyService_WithInjectedMock()
        {
            var services = GetServices();

            var mockedExternalPartyProxy = new Mock<IExternalPartyProxy>();
            mockedExternalPartyProxy
                .Setup(m => m.Call(It.IsAny<WarrantyRequest>()))
                .ReturnsAsync(new WarrantyResponse(new WarrantyResponseHeader()));

            services.AddScoped(_ => mockedExternalPartyProxy.Object);

            var serviceProvider = services.BuildServiceProvider();
            using var serviceScope = serviceProvider.CreateScope();
            var warrantyService = serviceScope.ServiceProvider.GetRequiredService<IWarrantyService>();
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
        [Trait("accessibility", "public")]
        [Trait("database", "BudgetData")]
        [Trait("database", "ComprehensiveData")]
        [Trait("external-service", "moq-mocked")]
        [Theory]
        [MemberData(nameof(VerifyTestData))]
        public async Task Verify(
            VerifyWarrantyCaseRequest request,
            IList<Guid> requestIds,
            IList<ExternalPartyCall> externalPartyCalls,
            VerifyWarrantyCaseResponse expectedResponse,
            int expectedWarrantyCaseVerificationCount,
            IList<WarrantyCaseVerification> expectedWarrantyCaseVerifications,
            int expectedWarrantyProofCount,
            IList<WarrantyProof?> expectedWarrantyProofs, // corresponds to requestIds
            int expectedExternalPartyRequestCount,
            IList<ExternalPartyRequest?> expectedExternalPartyRequests, // corresponds to requestIds
            int expectedExternalPartyResponseCount,
            IList<ExternalPartyResponse?> expectedExternalPartyResponses) // corresponds to requestIds
        {
            var services = GetServices();

            var mockedRequestIdGenerator = new Mock<IRequestIdGenerator>(MockBehavior.Strict);
            var mockedRequestIdGeneratorSetupSequence = mockedRequestIdGenerator
                .SetupSequence(m => m.GenerateRequestId());
            foreach (var requestId in requestIds)
            {
                mockedRequestIdGeneratorSetupSequence.Returns(requestId);
            }
            services.AddSingleton(_ => mockedRequestIdGenerator.Object);

            List<WarrantyRequest> actualExternalPartyCallRequests = new List<WarrantyRequest>();

            var mockedExternalPartyProxy = new Mock<IExternalPartyProxy>(MockBehavior.Strict);
            foreach (var externalPartyCall in externalPartyCalls)
            {
                var mockedExternalPartyProxySetup = mockedExternalPartyProxy
                        .Setup(m => m.Call(It.IsAny<WarrantyRequest>()))
                        .Callback<WarrantyRequest>(warrantyRequest => actualExternalPartyCallRequests.Add(warrantyRequest));
                if (externalPartyCall.Throws)
                    mockedExternalPartyProxySetup.Throws(new NetworkException());
                else
                    mockedExternalPartyProxySetup.ReturnsAsync(externalPartyCall.Response!);
            }
            services.AddScoped(_ => mockedExternalPartyProxy.Object);

            var serviceProvider = services.BuildServiceProvider();
            using var serviceScope = serviceProvider.CreateScope();
            var warrantyService = serviceScope.ServiceProvider.GetRequiredService<IWarrantyService>();

            var actual = await warrantyService.Verify(request);

            // assert
            Assert.Equal(expectedResponse.IsSuccess, actual.IsSuccess);
            actual.WarrantyCaseResponse.Should().BeEquivalentTo(expectedResponse.WarrantyCaseResponse);
            Assert.Equal(expectedResponse.FailureType, actual.FailureType);
            Assert.Equal(expectedResponse.FailureMessage is null, actual.FailureMessage is null);

            mockedExternalPartyProxy.Verify(
                m => m.Call(It.IsAny<WarrantyRequest>()),
                Times.Exactly(externalPartyCalls.Count));
            actualExternalPartyCallRequests.Should().BeEquivalentTo(externalPartyCalls.Select(c => c.ExpectedRequest).ToList());

            // assert saved data
            using (var comprehensiveDbContext = serviceScope.ServiceProvider.GetRequiredService<ComprehensiveDataDbContext>())
            {
                var actualWarrantyCaseVerificationCount = comprehensiveDbContext.WarrantyCaseVerification
                    .Where(v => v.OrderId == request.OrderId)
                    .Count();
                Assert.Equal(expectedWarrantyCaseVerificationCount, actualWarrantyCaseVerificationCount);

                foreach ((WarrantyCaseVerification expectedWarrantyCaseVerification, int i) in
                    expectedWarrantyCaseVerifications.Select((y, i) => (y, i)))
                {
                    var actualWarrantyCaseVerification = comprehensiveDbContext.WarrantyCaseVerification
                        .Where(v => v.OrderId == request.OrderId)
                        .OrderByDescending(v => v.DateTime)
                        .Skip(i)
                        .First();
                    actualWarrantyCaseVerification.Should().BeEquivalentTo(
                        expectedWarrantyCaseVerification,
                        opt => opt
                            .Excluding(v => v.Id)
                            .Excluding(v => v.DateTime)
                            .Excluding(v => v.FailureMessage)
                            .Excluding(v => v.ConvertedResponse));
                    var expectedConvertedResponse = JsonConvert.DeserializeObject<WarrantyCaseResponse?>(expectedWarrantyCaseVerification.ConvertedResponse ?? "");
                    var actualConvertedResponse = JsonConvert.DeserializeObject<WarrantyCaseResponse?>(actualWarrantyCaseVerification.ConvertedResponse ?? "");
                    actualConvertedResponse.Should().BeEquivalentTo(expectedConvertedResponse);
                }

                var actualWarrantyProofCount = comprehensiveDbContext.WarrantyProof
                    .Where(p => p.OrderId == request.OrderId)
                    .Count();
                Assert.Equal(actualWarrantyProofCount, expectedWarrantyProofCount);

                foreach ((WarrantyProof? expectedWarrantyProof, int i) in
                    expectedWarrantyProofs.Select((y, i) => (y, i)))
                {
                    var actualWarrantyProof = comprehensiveDbContext.WarrantyProof
                        .Where(p => p.RequestId == requestIds.Last())
                        .SingleOrDefault();
                    actualWarrantyProof.Should().BeEquivalentTo(
                        expectedWarrantyProof,
                        opt => opt
                            .Excluding(req => req.Id)
                            .Excluding(req => req.DateTime));
                }
            }

            using (var budgetDbContext = serviceScope.ServiceProvider.GetRequiredService<BudgetDataDbContext>())
            {
                var actualExternalPartyRequestCount = budgetDbContext.ExternalPartyRequest
                    .Where(req => req.OrderId == request.OrderId)
                    .Count();
                Assert.Equal(expectedExternalPartyRequestCount, actualExternalPartyRequestCount);

                foreach ((ExternalPartyRequest? expectedExternalPartyRequest, int i) in
                    expectedExternalPartyRequests.Select((y, i) => (y, i)))
                {
                    var actualExternalPartyRequest = budgetDbContext.ExternalPartyRequest
                        .Where(req => req.OrderId == request.OrderId && req.RequestId == requestIds[i])
                        .SingleOrDefault();
                    actualExternalPartyRequest.Should().BeEquivalentTo(
                        expectedExternalPartyRequest,
                        opt => opt
                            .Excluding(req => req.Id)
                            .Excluding(req => req.DateTime)
                            .Excluding(req => req.Request));
                    var expectedRawRequest = JsonConvert.DeserializeObject<WarrantyResponse?>(expectedExternalPartyRequest.Request ?? "");
                    var actualRawRequest = JsonConvert.DeserializeObject<WarrantyResponse?>(actualExternalPartyRequest.Request ?? "");
                    actualRawRequest.Should().BeEquivalentTo(expectedRawRequest);
                }

                var actualExternalPartyResponseCount = budgetDbContext.ExternalPartyResponse
                    .Where(res => res.OrderId == request.OrderId)
                    .Count();
                Assert.Equal(expectedExternalPartyResponseCount, actualExternalPartyResponseCount);

                foreach ((ExternalPartyResponse? expectedExternalPartyResponse, int i) in
                    expectedExternalPartyResponses.Select((y, i) => (y, i)))
                {
                    var actualExternalPartyResponse = budgetDbContext.ExternalPartyResponse
                        .Where(req => req.OrderId == request.OrderId && req.RequestId == requestIds[i])
                        .SingleOrDefault();
                    actualExternalPartyResponse.Should().BeEquivalentTo(
                        expectedExternalPartyResponse,
                        opt => opt
                            .Excluding(res => res.Id)
                            .Excluding(res => res.DateTime)
                            .Excluding(res => res.Response));
                    var expectedRawResponse = JsonConvert.DeserializeObject<WarrantyResponse?>(expectedExternalPartyResponse.Response ?? "");
                    var actualRawResponse = JsonConvert.DeserializeObject<WarrantyResponse?>(actualExternalPartyResponse.Response ?? "");
                    actualRawResponse.Should().BeEquivalentTo(expectedRawResponse);
                }
            }
        }

        public static IEnumerable<object?[]> VerifyTestData()
        {
            var assembly = Assembly.GetExecutingAssembly();
            List<VerifyTestCaseData> testCaseDatas;
            using (var stream = assembly.GetManifestResourceStream("ControlFlowPractise.Core.Tests.WarrantyServiceTestSetups.VerifySetups.json")!)
            using (var reader = new StreamReader(stream))
            {
                JsonSerializer serializer = new JsonSerializer();
                testCaseDatas = (List<VerifyTestCaseData>)serializer.Deserialize(reader, typeof(List<VerifyTestCaseData>))!;
            }
            foreach (var testCaseData in testCaseDatas)
            {
                yield return new object?[]
                {
                    testCaseData.Request,
                    testCaseData.RequestIds,
                    testCaseData.ExternalPartyCalls,
                    testCaseData.ExpectedResponse,
                    testCaseData.ExpectedWarrantyCaseVerificationCount,
                    testCaseData.ExpectedWarrantyCaseVerifications,
                    testCaseData.ExpectedWarrantyProofCount,
                    testCaseData.ExpectedWarrantyProofs,
                    testCaseData.ExpectedExternalPartyRequestCount,
                    testCaseData.ExpectedExternalPartyRequests,
                    testCaseData.ExpectedExternalPartyResponseCount,
                    testCaseData.ExpectedExternalPartyResponses
                };
            }

            // verify-verify-success
            // - BudgetDatabase ExternalPartyRequest
            // - External Party Called
            // - BudgetDatabase ExternalPartyResponse
            // - ComprehensiveDatabase WarrantyCaseVerification
            // - response
            {
                var request = new VerifyWarrantyCaseRequest(orderId: "verify-verify-success")
                {
                    Operation = WarrantyCaseOperation.Verify,
                    WarrantyCaseId = "406",
                    TransactionDateTime = new DateTime(2021, 3, 4, 0, 52, 0, DateTimeKind.Utc),
                    ProductId = "829",
                    PurchaserFirstName = "Jasleen",
                    PurchaserLastName = "Coates",
                    PurchaserEmail = "jasleen.coates@email.com",
                    VendorFirstName = "Izaac",
                    VendorLastName = "Mullen",
                    VendorEmail = "izaac.mullen@email.com",
                    VendorPhoneNumber = "0660 120 68 85"
                };
                var requestId = Guid.Parse("d5e138dc-6703-4484-a588-351fc09a352e");
                var expectedExternalPartyCallRequest = new WarrantyRequest
                {
                    RequestId = requestId,
                    RequestType = WarrantyRequestType.Verify,
                    WarrantyCaseId = "406",
                    TransactionDate = "2021-03-04T11:52:00+11:00",
                    OrderDetails = new List<OrderDetail>
                    {
                        new OrderDetail(
                            orderId: "verify-verify-success",
                            purchaserDetail: new PurchaserDetail(
                                firstName: "Jasleen",
                                lastName: "Coates",
                                email: "jasleen.coates@email.com"),
                            vendorDetail: new VendorDetail(
                                firstName: "Izaac",
                                lastName: "Mullen",
                                email: "izaac.mullen@email.com",
                                phoneNumber: "0660 120 68 85"))
                        {
                            ProductDetails = new List<ProductDetail>
                            {
                                new ProductDetail("829")
                            }
                        }
                    }
                };
                var externalPartyCallResponse = new WarrantyResponse(
                    new WarrantyResponseHeader
                    {
                        RequestId = requestId,
                        ResponseId = Guid.NewGuid(),
                        RequestType = WarrantyRequestType.Verify,
                        WarrantyCaseId = "406"
                    })
                {
                    Body = new WarrantyResponseBody("YES")
                    {
                        CaseStatus = CaseStatus.Claimed,
                        OrderReports = new List<OrderReport>
                        {
                            new OrderReport(
                                orderId: "verify-verify-success",
                                conformanceIndicator: "YES")
                            {
                                WarrantyEstimatedAmount = 89,
                                ConformanceMessages = new List<ConformanceMessage>
                                {
                                    new ConformanceMessage("Please wait until certified.")
                                    {
                                        Level = ConformanceLevel.Information
                                    }
                                }
                            }
                        }
                    }
                };
                var warrantyCaseResponse = new WarrantyCaseResponse(
                    orderId: "verify-verify-success",
                    warrantyCaseId: "406")
                {
                    Operation = WarrantyCaseOperation.Verify,
                    WarrantyCaseStatus = WarrantyCaseStatus.Claimed,
                    WarrantyEstimatedAmount = 89,
                    Conformance = true,
                    ConformanceMessages = new List<WarrantyConformanceMessage>
                    {
                        new WarrantyConformanceMessage("Please wait until certified.")
                        {
                            Level = WarrantyConformanceLevel.Information
                        }
                    }
                };
                var expectedResponse = new VerifyWarrantyCaseResponse
                {
                    IsSuccess = true,
                    WarrantyCaseResponse = warrantyCaseResponse
                };
                var expectedWarrantyCaseVerificationCount = 2;
                var expectedWarrantyCaseVerification = new WarrantyCaseVerification(
                    orderId: "verify-verify-success")
                {
                    WarrantyCaseId = "406",
                    Operation = WarrantyCaseOperation.Verify,
                    WarrantyCaseStatus = WarrantyCaseStatus.Claimed,
                    RequestId = requestId,
                    CalledExternalParty = true,
                    CalledWithResponse = true,
                    ResponseHasNoError = true,
                    ConvertedResponse = JsonConvert.SerializeObject(warrantyCaseResponse)
                };
                var expectedWarrantyProofCount = 0;
                var expectedExternalPartyRequestCount = 2;
                var expectedExternalPartyRequest = new ExternalPartyRequest(
                    orderId: "verify-verify-success",
                    request: JsonConvert.SerializeObject(expectedExternalPartyCallRequest))
                {
                    Operation = WarrantyCaseOperation.Verify,
                    RequestId = requestId,
                };
                var expectedExternalPartyResponseCount = 2;
                var expectedExternalPartyResponse = new ExternalPartyResponse(
                    orderId: "verify-verify-success",
                    response: JsonConvert.SerializeObject(externalPartyCallResponse))
                {
                    Operation = WarrantyCaseOperation.Verify,
                    RequestId = requestId,
                };
                yield return new object?[]
                {
                    request,
                    new List<Guid>{ requestId },
                    new List<(WarrantyRequest ExpectedRequest, bool Throws, WarrantyResponse? Response)>
                    {
                        (expectedExternalPartyCallRequest, false, externalPartyCallResponse)
                    },
                    expectedResponse,
                    expectedWarrantyCaseVerificationCount,
                    new List<WarrantyCaseVerification>{ expectedWarrantyCaseVerification },
                    expectedWarrantyProofCount,
                    new List<WarrantyProof?>{ null },
                    expectedExternalPartyRequestCount,
                    new List<ExternalPartyRequest?>{ expectedExternalPartyRequest },
                    expectedExternalPartyResponseCount,
                    new List<ExternalPartyResponse?>{ expectedExternalPartyResponse }
                };
            }
            // verify-verify-success-NonConformance (updated to wrong data)
            //
            // verify-verify-WarrantyServiceInternalError-failure
            // BudgetDatabase ExternalPartyRequest
            // External Party Called
            // BudgetDatabase ExternalPartyResponse
            // ComprehensiveDatabase WarrantyCaseVerification
            // response
            //
            // verify-commit-success
            // BudgetDatabase ExternalPartyRequest x2
            // External Party Called x2
            // BudgetDatabase ExternalPartyResponse x2
            // ComprehensiveDatabase WarrantyCaseVerification x2
            // response
            //
            // verify-commit-VerifyBeforeCommit-success-condition-failure
            // BudgetDatabase ExternalPartyRequest
            // External Party Called
            // BudgetDatabase ExternalPartyResponse
            // ComprehensiveDatabase WarrantyCaseVerification
            // response
            //
            // verify-commit-success-condition-failure
            // BudgetDatabase ExternalPartyRequest x2
            // External Party Called x2
            // BudgetDatabase ExternalPartyResponse x2
            // ComprehensiveDatabase WarrantyCaseVerification x2
            // response

            // verify-cancel-success
            //
            // verify-cancel-network-failure
            // BudgetDatabase ExternalPartyRequest
            // External Party Called
            // ComprehensiveDatabase WarrantyCaseVerification
            // response
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
