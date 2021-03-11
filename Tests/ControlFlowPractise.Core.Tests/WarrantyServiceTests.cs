using ControlFlowPractise.BudgetData;
using ControlFlowPractise.BudgetData.Models;
using ControlFlowPractise.Common;
using ControlFlowPractise.ComprehensiveData;
using ControlFlowPractise.ComprehensiveData.Models;
using ControlFlowPractise.ExternalParty;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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
        [Trait("external-service", "mocked")]
        [Theory]
        [MemberData(nameof(VerifyCreateTestData))]
        public async Task VerifyCreate(
            VerifyWarrantyCaseRequest request,
            Guid requestId,
            WarrantyRequest? expectedExternalPartyCallRequest,
            WarrantyResponse? externalPartyCallResponse,
            VerifyWarrantyCaseResponse expectedResponse,
            int expectedWarrantyCaseVerificationCount,
            WarrantyCaseVerification? expectedWarrantyCaseVerification,
            int expectedExternalPartyRequestCount,
            ExternalPartyRequest? expectedExternalPartyRequest,
            int expectedExternalPartyResponseCount,
            ExternalPartyResponse? expectedExternalPartyResponse)
        {
            var services = GetServices();

            WarrantyRequest? actualExternalPartyCallRequest = null;

            var mockedExternalPartyProxy = new Mock<IExternalPartyProxy>(MockBehavior.Strict);
            if (!(expectedExternalPartyCallRequest is null))
            {
                mockedExternalPartyProxy
                    .Setup(m => m.Call(It.IsAny<WarrantyRequest>()))
                    .Callback<WarrantyRequest>(warrantyRequest => actualExternalPartyCallRequest = warrantyRequest)
                    .ReturnsAsync(externalPartyCallResponse!);
            }
            services.AddScoped(_ => mockedExternalPartyProxy.Object);

            var mockedRequestIdGenerator = new Mock<IRequestIdGenerator>();
            mockedRequestIdGenerator
                .Setup(m => m.GenerateRequestId())
                .Returns(requestId);
            services.AddSingleton(_ => mockedRequestIdGenerator.Object);

            var serviceProvider = services.BuildServiceProvider();
            using var serviceScope = serviceProvider.CreateScope();
            var warrantyService = serviceScope.ServiceProvider.GetRequiredService<IWarrantyService>();

            var actual = await warrantyService.Verify(request);

            // assert
            Assert.Equal(expectedResponse.IsSuccess, actual.IsSuccess);
            actual.WarrantyCaseResponse.Should().BeEquivalentTo(expectedResponse.WarrantyCaseResponse);
            Assert.Equal(expectedResponse.FailureType, actual.FailureType);
            Assert.Equal(expectedResponse.FailureMessage is null, actual.FailureMessage is null);

            if (!(expectedExternalPartyCallRequest is null))
            {
                mockedExternalPartyProxy.Verify(
                    m => m.Call(It.IsAny<WarrantyRequest>()),
                    Times.Once());
                actualExternalPartyCallRequest.Should().BeEquivalentTo(expectedExternalPartyCallRequest);
            }

            // assert saved data
            using (var comprehensiveDbContext = serviceScope.ServiceProvider.GetRequiredService<ComprehensiveDataDbContext>())
            {
                var actualWarrantyCaseVerificationCount = comprehensiveDbContext.WarrantyCaseVerification
                    .Where(v => v.OrderId == request.OrderId)
                    .Count();
                Assert.Equal(expectedWarrantyCaseVerificationCount, actualWarrantyCaseVerificationCount);

                if (!(expectedWarrantyCaseVerification is null))
                {
                    var actualWarrantyCaseVerification = comprehensiveDbContext.WarrantyCaseVerification
                        .Where(v => v.OrderId == request.OrderId)
                        .OrderByDescending(v => v.DateTime)
                        .First();

                    actualWarrantyCaseVerification.Should().BeEquivalentTo(
                        expectedWarrantyCaseVerification,
                        opt => opt
                            .Excluding(v => v.Id)
                            .Excluding(v => v.DateTime)
                            .Excluding(v => v.FailureMessage));
                }
            }

            using (var budgetDbContext = serviceScope.ServiceProvider.GetRequiredService<BudgetDataDbContext>())
            {
                var actualExternalPartyRequestCount = budgetDbContext.ExternalPartyRequest
                    .Where(req => req.OrderId == request.OrderId)
                    .Count();
                Assert.Equal(expectedExternalPartyRequestCount, actualExternalPartyRequestCount);

                if (!(expectedExternalPartyRequest is null))
                {
                    var actualExternalPartyRequest = budgetDbContext.ExternalPartyRequest
                        .Where(req => req.OrderId == request.OrderId && req.RequestId == requestId)
                        .Single();

                    actualExternalPartyRequest.Should().BeEquivalentTo(
                        expectedExternalPartyRequest,
                        opt => opt
                            .Excluding(req => req.Id)
                            .Excluding(req => req.DateTime));
                }
            }

            using (var budgetDbContext = serviceScope.ServiceProvider.GetRequiredService<BudgetDataDbContext>())
            {
                var actualExternalPartyResponseCount = budgetDbContext.ExternalPartyResponse
                    .Where(res => res.OrderId == request.OrderId)
                    .Count();
                Assert.Equal(expectedExternalPartyResponseCount, actualExternalPartyResponseCount);

                if (!(expectedExternalPartyResponse is null))
                {
                    var actualExternalPartyResponse = budgetDbContext.ExternalPartyResponse
                        .Where(res => res.OrderId == request.OrderId && res.RequestId == requestId)
                        .Single();

                    actualExternalPartyResponse.Should().BeEquivalentTo(
                        expectedExternalPartyResponse,
                        opt => opt
                            .Excluding(res => res.Id)
                            .Excluding(res => res.DateTime));
                }
            }
        }

        public static IEnumerable<object?[]> VerifyCreateTestData()
        {
            // verify-create-success
            {
                var request = new VerifyWarrantyCaseRequest(orderId: "verify-create-success")
                {
                    Operation = WarrantyCaseOperation.Create,
                    TransactionDateTime = new DateTime(2021, 3, 4, 0, 52, 0, DateTimeKind.Utc),
                    ProductId = "369",
                    PurchaserFirstName = "Bradley",
                    PurchaserLastName = "Blair",
                    PurchaserEmail = "bradley.blair@email.com",
                    VendorFirstName = "Emrys",
                    VendorLastName = "Kinney",
                    VendorEmail = "emrys.kinney@email.com",
                    VendorPhoneNumber = "0491 570 156"
                };
                var requestId = Guid.Parse("a8718ab9-ce33-4d21-a9bf-4feb85335562");
                var expectedExternalPartyCallRequest = new WarrantyRequest
                {
                    RequestId = requestId,
                    RequestType = WarrantyRequestType.Verify,
                    TransactionDate = "2021-03-04T11:52:00+11:00",
                    OrderDetails = new List<OrderDetail>
                    {
                        new OrderDetail(
                            orderId: "verify-create-success",
                            purchaserDetail: new PurchaserDetail(
                                firstName: "Bradley",
                                lastName: "Blair",
                                email: "bradley.blair@email.com"),
                            vendorDetail: new VendorDetail(
                                firstName: "Emrys",
                                lastName: "Kinney",
                                email: "emrys.kinney@email.com",
                                phoneNumber: "0491 570 156"))
                        {
                            ProductDetails = new List<ProductDetail>
                            {
                                new ProductDetail("369")
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
                        WarrantyCaseId = "757"
                    })
                {
                    Body = new WarrantyResponseBody("NO")
                    {
                        CaseStatus = CaseStatus.WaitingForClaim,
                        OrderReports = new List<OrderReport>
                        {
                            new OrderReport(
                                orderId: "verify-create-success",
                                conformanceIndicator: "NO")
                            {
                                ConformanceMessages = new List<ConformanceMessage>
                                {
                                    new ConformanceMessage("Please claim.")
                                    {
                                        Level = ConformanceLevel.Information
                                    }
                                }
                            }
                        }
                    }
                };
                var warrantyCaseResponse = new WarrantyCaseResponse(
                    orderId: "verify-create-success",
                    warrantyCaseId: "757")
                {
                    Operation = WarrantyCaseOperation.Create,
                    WarrantyCaseStatus = WarrantyCaseStatus.WaitingForClaim,
                    Conformance = false,
                    ConformanceMessages = new List<WarrantyConformanceMessage>
                    {
                        new WarrantyConformanceMessage("Please claim.")
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
                var expectedWarrantyCaseVerificationCount = 1;
                var expectedWarrantyCaseVerification = new WarrantyCaseVerification(
                    orderId: "verify-create-success")
                {
                    WarrantyCaseId = "757",
                    Operation = WarrantyCaseOperation.Create,
                    WarrantyCaseStatus = WarrantyCaseStatus.WaitingForClaim,
                    RequestId = requestId,
                    CalledExternalParty = true,
                    CalledWithResponse = true,
                    ResponseHasNoError = true,
                    ConvertedResponse = JsonConvert.SerializeObject(warrantyCaseResponse)
                };
                var expectedExternalPartyRequestCount = 1;
                var expectedExternalPartyRequest = new ExternalPartyRequest(
                    orderId: "verify-create-success",
                    request: JsonConvert.SerializeObject(expectedExternalPartyCallRequest))
                {
                    Operation = WarrantyCaseOperation.Create,
                    RequestId = requestId,
                };
                var expectedExternalPartyResponseCount = 1;
                var expectedExternalPartyResponse = new ExternalPartyResponse(
                    orderId: "verify-create-success",
                    response: JsonConvert.SerializeObject(externalPartyCallResponse))
                {
                    Operation = WarrantyCaseOperation.Create,
                    RequestId = requestId,
                };
                yield return new object[]
                {
                    request,
                    requestId,
                    expectedExternalPartyCallRequest,
                    externalPartyCallResponse,
                    expectedResponse,
                    expectedWarrantyCaseVerificationCount,
                    expectedWarrantyCaseVerification,
                    expectedExternalPartyRequestCount,
                    expectedExternalPartyRequest,
                    expectedExternalPartyResponseCount,
                    expectedExternalPartyResponse
                };
            }
            // verify-create-validation-failure
            {
                var request = new VerifyWarrantyCaseRequest(orderId: "verify-create-validation-failure")
                {
                    Operation = WarrantyCaseOperation.Create,
                    TransactionDateTime = new DateTime(2021, 3, 4, 0, 52, 0, DateTimeKind.Utc),
                    ProductId = "527",
                    PurchaserFirstName = null, // missing PurchaserFirstName
                    PurchaserLastName = "Cortes",
                    PurchaserEmail = "katrina.cortes@email.com",
                    VendorFirstName = "Tasneem",
                    VendorLastName = "Frame",
                    VendorEmail = "tasneem.frame@email.com",
                    VendorPhoneNumber = "0688 527 07 91 "
                };
                var requestId = Guid.Parse("d52902b8-f0e5-467c-8adc-fe30e20a51e7");
                WarrantyRequest? expectedExternalPartyCallRequest = null;
                WarrantyResponse? externalPartyCallResponse = null;
                var expectedResponse = new VerifyWarrantyCaseResponse
                {
                    IsSuccess = false,
                    FailureType = FailureType.RequestValidationFailure,
                    FailureMessage = "Some validation error."
                };
                var expectedWarrantyCaseVerificationCount = 1;
                var expectedWarrantyCaseVerification = new WarrantyCaseVerification(
                    orderId: "verify-create-validation-failure")
                {
                    WarrantyCaseId = null,
                    Operation = WarrantyCaseOperation.Create,
                    WarrantyCaseStatus = null,
                    RequestId = requestId,
                    CalledExternalParty = false,
                    CalledWithResponse = null,
                    ResponseHasNoError = null,
                    FailureType = FailureType.RequestValidationFailure,
                    FailureMessage = "Some validation error.",
                    ConvertedResponse = null,

                };
                var expectedExternalPartyRequestCount = 0;
                ExternalPartyRequest? expectedExternalPartyRequest = null;
                var expectedExternalPartyResponseCount = 0;
                ExternalPartyResponse? expectedExternalPartyResponse = null;
                yield return new object?[]
                {
                    request,
                    requestId,
                    expectedExternalPartyCallRequest,
                    externalPartyCallResponse,
                    expectedResponse,
                    expectedWarrantyCaseVerificationCount,
                    expectedWarrantyCaseVerification,
                    expectedExternalPartyRequestCount,
                    expectedExternalPartyRequest,
                    expectedExternalPartyResponseCount,
                    expectedExternalPartyResponse
                };
            }
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
        [Trait("external-service", "mocked")]
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
        [Trait("external-service", "mocked")]
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
        [Trait("external-service", "mocked")]
        [Theory]
        [MemberData(nameof(VerifyCancelTestData))]
        public async Task VerifyCancel(
            VerifyWarrantyCaseRequest request)
        {
            var services = GetServices();

            // injecy mocked external party too services

            var serviceProvider = services.BuildServiceProvider();
            using (var serviceScope = serviceProvider.CreateScope())
            {
                var warrantyService = serviceScope.ServiceProvider.GetRequiredService<IWarrantyService>();
            }
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
