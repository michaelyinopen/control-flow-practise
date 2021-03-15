using ControlFlowPractise.BudgetData.Models;
using ControlFlowPractise.Common;
using ControlFlowPractise.Common.ControlFlow;
using ControlFlowPractise.ComprehensiveData.Models;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ControlFlowPractise.Core
{
    public interface IWarrantyService
    {
        Task<VerifyWarrantyCaseResponse> Verify(VerifyWarrantyCaseRequest request);
        Task<GetCurrentWarrantyCaseVerificationResponse> GetCurrentWarrantyCaseVerification(string orderId);
        Task<GetWarrantyProofResponse> GetWarrantyProof(string orderId);
    }

    public class WarrantyService : IWarrantyService
    {
        public WarrantyService(
            FailureClassification failureClassification,
            IRequestIdGenerator requestIdGenerator,
            ComprehensiveDataWrapper comprehensiveDataWrapper,
            BudgetDataWrapper budgetDataWrapper,
            RequestValidator requestValidator,
            RequestBuilder requestBuilder,
            ExternalPartyWrapper externalPartyWrapper,
            ResponseValidator responseValidator,
            ResponseConverter responseConverter)
        {
            FailureClassification = failureClassification;
            RequestIdGenerator = requestIdGenerator;
            ComprehensiveDataWrapper = comprehensiveDataWrapper;
            BudgetDataWrapper = budgetDataWrapper;
            RequestValidator = requestValidator;
            RequestBuilder = requestBuilder;
            ExternalPartyWrapper = externalPartyWrapper;
            ResponseValidator = responseValidator;
            ResponseConverter = responseConverter;
        }

        private FailureClassification FailureClassification { get; }
        private IRequestIdGenerator RequestIdGenerator { get; }
        private ComprehensiveDataWrapper ComprehensiveDataWrapper { get; }
        private BudgetDataWrapper BudgetDataWrapper { get; }
        private RequestValidator RequestValidator { get; }
        private RequestBuilder RequestBuilder { get; }
        private ExternalPartyWrapper ExternalPartyWrapper { get; }
        private ResponseValidator ResponseValidator { get; }
        private ResponseConverter ResponseConverter { get; }

        #region Verify
        public async Task<VerifyWarrantyCaseResponse> Verify(
            VerifyWarrantyCaseRequest request)
        {
            var preCommitVerifyResult = await PreCommitVerify(request);
            if (!preCommitVerifyResult.IsSuccess)
            {
                var preCommitVerifyWarrantyCaseResponse = BuidVerifyWarrantyCaseResponse(
                    preCommitVerifyResult,
                    preSavedResponse: null);
                return preCommitVerifyWarrantyCaseResponse;
            }
            var preSavedResponse = preCommitVerifyResult.Success; // null if not commit

            var requestId = RequestIdGenerator.GenerateRequestId();
            var operation = request.Operation;

            var performVerifyActionResult = await PerformVerifyAction(request, operation, requestId);
            var saveResult = await SaveWarrantyCaseResponse(request, operation, requestId, performVerifyActionResult);
            var successfulConditionResult = SatisfySuccessfulCondition(
                operation,
                isPreCommitVerify: false,
                saveResult);
            var verifyWarrantyCaseResponse = BuidVerifyWarrantyCaseResponse(
                    successfulConditionResult,
                    preSavedResponse: preSavedResponse);
            return verifyWarrantyCaseResponse;
        }

        // if the request is not Commit, returns Result with IsSuccess: true and Success: null
        internal async Task<Result<WarrantyCaseResponse, IFailure>> PreCommitVerify(
            VerifyWarrantyCaseRequest request)
        {
            if (request.Operation != WarrantyCaseOperation.Commit)
                return Result<WarrantyCaseResponse, IFailure>.SuccessNull();

            var requestId = RequestIdGenerator.GenerateRequestId();

            var performVerifyActionResult = await PerformVerifyAction(request, WarrantyCaseOperation.Verify, requestId);
            var saveResult = await SaveWarrantyCaseResponse(request, WarrantyCaseOperation.Verify, requestId, performVerifyActionResult);
            var successfulConditionResult = SatisfySuccessfulCondition(
                WarrantyCaseOperation.Verify,
                isPreCommitVerify: true,
                saveResult);

            if (successfulConditionResult.IsSuccess)
            {
                return successfulConditionResult;
            }
            else
            {
                var failure = successfulConditionResult.Failure!;
                return new Result<WarrantyCaseResponse, IFailure>(
                    new VerifyBeforeCommitFailure(
                        message: $"Pre-commit verification failed RequestId: `{requestId}`, FailureType: `{failure.FailureType}`, FailureMessage: `{failure.Message}`.",
                        warrantyCaseResponse: failure is SuccessfulConditionFailure successfulConditionFailure
                            ? successfulConditionFailure.WarrantyCaseResponse
                            : null));
            }
        }

        // success means called Thrid Party, saved raw request and raw response, (and saved warrantyProof), and returns a converted response
        // ---
        // anything here after SaveExternalPartyResponse in BudgetDatabase should be pure (non-deterministic and no side-effect)
        // so that the saved ExternalPartyResponse can be a source of truth
        internal async Task<Result<WarrantyCaseResponse, IFailure>> PerformVerifyAction(
            VerifyWarrantyCaseRequest request,
            WarrantyCaseOperation operation,
            Guid requestId)
        {
            var validateRequestResult = RequestValidator.Validate(request, operation, requestId);
            if (!validateRequestResult.IsSuccess)
            {
                return new Result<WarrantyCaseResponse, IFailure>(validateRequestResult.Failure!);
            }

            var build = RequestBuilder.Build(request, operation, requestId);
            if (!build.IsSuccess)
                return new Result<WarrantyCaseResponse, IFailure>(build.Failure!);
            var warrantyRequest = build.Success!;

            var saveWarrantyRequest = await BudgetDataWrapper.SaveExternalPartyRequest(
                new ExternalPartyRequest(
                    orderId: request.OrderId,
                    request: JsonConvert.SerializeObject(warrantyRequest))
                {
                    Operation = operation,
                    RequestId = requestId,
                });
            if (!saveWarrantyRequest.IsSuccess)
                return new Result<WarrantyCaseResponse, IFailure>(saveWarrantyRequest.Failure!);

            var call = await ExternalPartyWrapper.Call(warrantyRequest);
            if (!call.IsSuccess)
                return new Result<WarrantyCaseResponse, IFailure>(call.Failure!);
            var rawResponse = call.Success!;

            var saveWarrantyResponse = await BudgetDataWrapper.SaveExternalPartyResponse(
                new ExternalPartyResponse(
                    orderId: request.OrderId,
                    response: JsonConvert.SerializeObject(rawResponse))
                {
                    Operation = operation,
                    RequestId = requestId,
                });
            if (!saveWarrantyResponse.IsSuccess)
                return new Result<WarrantyCaseResponse, IFailure>(saveWarrantyRequest.Failure!);

            var validateResponse = ResponseValidator.Validate(request, operation, requestId, rawResponse);
            if (!validateResponse.IsSuccess)
                return new Result<WarrantyCaseResponse, IFailure>(validateResponse.Failure!);

            var convertResponse = ResponseConverter.Convert(request, operation, requestId, rawResponse);
            if (!convertResponse.IsSuccess)
                return new Result<WarrantyCaseResponse, IFailure>(convertResponse.Failure!);
            var convertedResponse = convertResponse.Success!;

            if (operation == WarrantyCaseOperation.Commit
                && convertedResponse.WarrantyCaseStatus == WarrantyCaseStatus.Committed)
            {
                var saveWarrantyProof = await ComprehensiveDataWrapper.SaveWarrantyProof(
                    new WarrantyProof(
                        orderId: request.OrderId,
                        warrantyCaseId: convertedResponse.WarrantyCaseId,
                        proof: rawResponse.Body!.OrderReports.Single().WarrantyProof!)
                    {
                        RequestId = requestId,
                    });
                if (!saveWarrantyProof.IsSuccess)
                    return new Result<WarrantyCaseResponse, IFailure>(saveWarrantyProof.Failure!);
            }

            return new Result<WarrantyCaseResponse, IFailure>(convertedResponse);
        }

        // Saves warrantyCaseVerification in ComprehensiveData, regardless of PerformVerifyAction is success or failure
        internal async Task<Result<WarrantyCaseResponse, IFailure>> SaveWarrantyCaseResponse(
            VerifyWarrantyCaseRequest request,
            WarrantyCaseOperation operation,
            Guid requestId,
            Result<WarrantyCaseResponse, IFailure> sourceResult)
        {
            if (sourceResult.IsSuccess)
            {
                var warrantyCaseResponse = sourceResult.Success!;
                var warrantyCaseVerification = new WarrantyCaseVerification(request.OrderId)
                {
                    Operation = operation,
                    WarrantyCaseStatus = warrantyCaseResponse.WarrantyCaseStatus,
                    RequestId = requestId,
                    WarrantyCaseId = warrantyCaseResponse.WarrantyCaseId,
                    CalledExternalParty = true,
                    CalledWithResponse = true,
                    ResponseHasNoError = true,
                    ConvertedResponse = JsonConvert.SerializeObject(warrantyCaseResponse)
                };
                var saveWarrantyCaseVerification =
                    await ComprehensiveDataWrapper.SaveWarrantyCaseVerification(warrantyCaseVerification);
                if (!saveWarrantyCaseVerification.IsSuccess)
                {
                    return new Result<WarrantyCaseResponse, IFailure>(
                        new SaveWarrantyCaseVerificationFailure(
                            saveWarrantyCaseVerification.Failure!.Message));
                }
                return new Result<WarrantyCaseResponse, IFailure>(warrantyCaseResponse);
            }
            else
            {
                var performVerifyActionFailure = sourceResult.Failure!;
                bool calledExternalParty = FailureClassification.CalledExternalParty(performVerifyActionFailure);
                bool? calledWithResponse = FailureClassification.CalledWithResponse(performVerifyActionFailure);
                bool? responseHasNoError = FailureClassification.ResponseHasNoError(performVerifyActionFailure);
                var warrantyCaseVerification = new WarrantyCaseVerification(request.OrderId)
                {
                    Operation = operation,
                    RequestId = requestId,
                    WarrantyCaseId = request.WarrantyCaseId,
                    CalledExternalParty = calledExternalParty,
                    CalledWithResponse = calledWithResponse,
                    ResponseHasNoError = responseHasNoError,
                    FailureType = performVerifyActionFailure.FailureType,
                    FailureMessage = performVerifyActionFailure.Message
                };
                var saveWarrantyCaseVerification =
                    await ComprehensiveDataWrapper.SaveWarrantyCaseVerification(warrantyCaseVerification);
                if (!saveWarrantyCaseVerification.IsSuccess)
                {
                    return new Result<WarrantyCaseResponse, IFailure>(
                        new SaveWarrantyCaseVerificationFailure(
                            saveWarrantyCaseVerification.Failure!.Message));
                }
                return new Result<WarrantyCaseResponse, IFailure>(performVerifyActionFailure);
            }
        }

        internal Result<WarrantyCaseResponse, IFailure> SatisfySuccessfulCondition(
            WarrantyCaseOperation operation,
            bool isPreCommitVerify,
            Result<WarrantyCaseResponse, IFailure> responseResult)
        {
            if (!responseResult.IsSuccess)
                return responseResult;

            var response = responseResult.Success!;
            var successResult = new Result<WarrantyCaseResponse, IFailure>(response);
            return operation switch
            {
                WarrantyCaseOperation.Create => successResult,
                WarrantyCaseOperation.Verify when isPreCommitVerify =>
                    response.WarrantyCaseStatus == WarrantyCaseStatus.Certified
                        || response.WarrantyCaseStatus == WarrantyCaseStatus.Committed
                        || response.WarrantyCaseStatus == WarrantyCaseStatus.Completed
                        ? successResult
                        : new Result<WarrantyCaseResponse, IFailure>(
                            new SuccessfulConditionFailure(
                                message: "Pre-commit Verify operation did not have response of WarrantyCaseStatus Certified, Committed or Completed",
                                warrantyCaseResponse: response)),
                WarrantyCaseOperation.Verify => successResult,
                WarrantyCaseOperation.Commit =>
                    response.WarrantyCaseStatus == WarrantyCaseStatus.Committed
                        || response.WarrantyCaseStatus == WarrantyCaseStatus.Completed
                        ? successResult
                        : new Result<WarrantyCaseResponse, IFailure>(
                            new SuccessfulConditionFailure(
                                message: "Commit operation did not have response of WarrantyCaseStatus Committed or Completed",
                                warrantyCaseResponse: response)),
                WarrantyCaseOperation.Cancel =>
                    response.WarrantyCaseStatus == WarrantyCaseStatus.Cancelled
                        ? successResult
                        : new Result<WarrantyCaseResponse, IFailure>(
                            new SuccessfulConditionFailure(
                                message: "Cancel operation did not have response of WarrantyCaseStatus Cancelled",
                                warrantyCaseResponse: response)),
                _ => throw new InvalidOperationException()
            };
        }

        internal VerifyWarrantyCaseResponse BuidVerifyWarrantyCaseResponse(
            Result<WarrantyCaseResponse, IFailure> sourceResult,
            WarrantyCaseResponse? preSavedResponse)
        {
            if (sourceResult.IsSuccess)
            {
                var warrantyCaseResponse = sourceResult.Success!;
                return new VerifyWarrantyCaseResponse
                {
                    IsSuccess = true,
                    WarrantyCaseResponse = warrantyCaseResponse,
                    FailureType = null,
                    FailureMessage = null,
                };
            }
            else
            {
                var failure = sourceResult.Failure!;
                return new VerifyWarrantyCaseResponse
                {
                    IsSuccess = false,
                    WarrantyCaseResponse =
                        failure is SuccessfulConditionFailure successfulConditionFailure
                        ? successfulConditionFailure.WarrantyCaseResponse
                        : failure is VerifyBeforeCommitFailure verifyBeforeCommitFailure
                        ? verifyBeforeCommitFailure.WarrantyCaseResponse
                        : preSavedResponse,
                    FailureType = failure.FailureType,
                    FailureMessage = failure.Message
                };
            }
        }
        #endregion Verify

        public async Task<GetCurrentWarrantyCaseVerificationResponse> GetCurrentWarrantyCaseVerification(string orderId)
        {
            var warrantyCaseVerificationResult = await ComprehensiveDataWrapper.GetCurrentWarrantyCaseVerification(orderId);
            if (!warrantyCaseVerificationResult.IsSuccess)
            {
                var failure = warrantyCaseVerificationResult.Failure!;
                return new GetCurrentWarrantyCaseVerificationResponse
                {
                    IsSuccess = false,
                    FailureType = failure.FailureType,
                    IsNotFound = failure is IIsNotFound isNotFoundFailure
                        ? isNotFoundFailure.IsNotFound
                        : null,
                    FailureMessage = failure.Message
                };
            }
            var warrantyCaseVerification = warrantyCaseVerificationResult.Success!;

            try
            {
                var warrantyCaseResponse = JsonConvert.DeserializeObject<WarrantyCaseResponse>(
                    warrantyCaseVerification.ConvertedResponse!);
                return new GetCurrentWarrantyCaseVerificationResponse
                {
                    IsSuccess = true,
                    WarrantyCaseResponse = warrantyCaseResponse
                };
            }
            catch (JsonException)
            {
                return new GetCurrentWarrantyCaseVerificationResponse
                {
                    IsSuccess = false,
                    FailureType = FailureType.GetWarrantyCaseVerificationFailure,
                    FailureMessage =
                        $"VerificationResult of OrderId: `{orderId}` cannot be deserialized from response of WarrantyCaseVerification of RequestId: `{warrantyCaseVerification.RequestId}`."
                };
            }
        }

        public async Task<GetWarrantyProofResponse> GetWarrantyProof(string orderId)
        {
            var getWarrantyProofResult = await GetWarrantyProofResult(orderId);
            if (!getWarrantyProofResult.IsSuccess)
            {
                var failure = getWarrantyProofResult.Failure!;
                return new GetWarrantyProofResponse(orderId)
                {
                    IsSuccess = false,
                    FailureType = failure.FailureType,
                    IsNotFound = failure is IIsNotFound isNotFoundFailure
                        ? isNotFoundFailure.IsNotFound
                        : null,
                    RequestId = (failure as GetWarrantyProofFailure)?.RequestId,
                    FailureMessage = failure.Message,
                };
            }
            var warrantyProofResponse = getWarrantyProofResult.Success!;
            return warrantyProofResponse;
        }

        internal async Task<Result<GetWarrantyProofResponse, IFailure>> GetWarrantyProofResult(string orderId)
        {
            var latestWarrantyCaseCommitResult = await ComprehensiveDataWrapper.GetLatestWarrantyCaseCommit(orderId);
            if (!latestWarrantyCaseCommitResult.IsSuccess)
                return new Result<GetWarrantyProofResponse, IFailure>(latestWarrantyCaseCommitResult.Failure!);
            var latestWarrantyCaseCommit = latestWarrantyCaseCommitResult.Success!;

            var getWarrantyProofResult = await ComprehensiveDataWrapper.GetWarrantyProof(
                orderId: orderId,
                requestId: latestWarrantyCaseCommit.RequestId);
            if (!getWarrantyProofResult.IsSuccess)
                return new Result<GetWarrantyProofResponse, IFailure>(getWarrantyProofResult.Failure!);
            var warrantyProof = getWarrantyProofResult.Success!;

            return new Result<GetWarrantyProofResponse, IFailure>(
                new GetWarrantyProofResponse(orderId)
                {
                    IsSuccess = true,
                    WarrantyCaseId = latestWarrantyCaseCommit.WarrantyCaseId,
                    RequestId = latestWarrantyCaseCommit.RequestId,
                    WarrantyProof = warrantyProof.Proof
                });
        }
    }
}
