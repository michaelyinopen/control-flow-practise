﻿using ControlFlowPractise.BudgetData.Models;
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
            ComprehensiveDataWrapper comprehensiveDataWrapper,
            BudgetDataWrapper budgetDataWrapper,
            RequestValidator requestValidator,
            RequestBuilder requestBuilder,
            ExternalPartyWrapper externalPartyWrapper,
            ResponseValidator responseValidator,
            ResponseConverter responseConverter)
        {
            FailureClassification = failureClassification;
            ComprehensiveDataWrapper = comprehensiveDataWrapper;
            BudgetDataWrapper = budgetDataWrapper;
            RequestValidator = requestValidator;
            RequestBuilder = requestBuilder;
            ExternalPartyWrapper = externalPartyWrapper;
            ResponseValidator = responseValidator;
            ResponseConverter = responseConverter;
        }

        private FailureClassification FailureClassification { get; }
        private ComprehensiveDataWrapper ComprehensiveDataWrapper { get; }
        private BudgetDataWrapper BudgetDataWrapper { get; }
        private RequestValidator RequestValidator { get; }
        private RequestBuilder RequestBuilder { get; }
        private ExternalPartyWrapper ExternalPartyWrapper { get; }
        private ResponseValidator ResponseValidator { get; }
        private ResponseConverter ResponseConverter { get; }

        #region Verify
        // generates requestId
        //
        // todo handle cases where PerformVerifyAction is successful, but ConformanceIndicator/CaseStatus doe not meet requirement for success
        // e.g. Commit but CaseStatus remains at Certified
        // e.g. Cancel but CaseStatus remains at Claimed
        public async Task<VerifyWarrantyCaseResponse> Verify(
            VerifyWarrantyCaseRequest request)
        {
            var requestId = Guid.NewGuid();
            var validateRequestResult = RequestValidator.Validate(request, requestId);
            if (!validateRequestResult.IsSuccess)
            {
                var validationFailure = validateRequestResult.Failure!;
                return new VerifyWarrantyCaseResponse
                {
                    IsSuccess = false,
                    FailureType = validationFailure.FailureType,
                    FailureMessage = validationFailure.Message
                };
            }
            var performVerifyActionResult = await PerformVerifyAction(request, requestId);
            var saveResult = await SaveWarrantyCaseResponse(request, requestId, performVerifyActionResult);
            var verifyWarrantyCaseResponse = BuidVerifyWarrantyCaseResponse(request, saveResult);
            return verifyWarrantyCaseResponse;
        }

        // success means called Thrid Party, saved raw request and raw response, (and saved warrantyProof), and returns a converted response
        // ---
        // anything here after SaveExternalPartyResponse in BudgetDatabase should be pure (non-deterministic and no side-effect)
        // so that the saved ExternalPartyResponse can be a source of truth
        internal async Task<Result<WarrantyCaseResponse, IFailure>> PerformVerifyAction(
            VerifyWarrantyCaseRequest request,
            Guid requestId)
        {
            var build = RequestBuilder.Build(request, requestId);
            if (!build.IsSuccess)
                return new Result<WarrantyCaseResponse, IFailure>(build.Failure!);
            var warrantyRequest = build.Success!;

            var saveWarrantyRequest = await BudgetDataWrapper.SaveExternalPartyRequest(
                new ExternalPartyRequest(
                    orderId: request.OrderId,
                    request: JsonConvert.SerializeObject(warrantyRequest))
                {
                    Operation = request.Operation,
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
                    Operation = request.Operation,
                    RequestId = requestId,
                });
            if (!saveWarrantyResponse.IsSuccess)
                return new Result<WarrantyCaseResponse, IFailure>(saveWarrantyRequest.Failure!);

            var validateResponse = ResponseValidator.Validate(request, requestId, rawResponse);
            if (!validateResponse.IsSuccess)
                return new Result<WarrantyCaseResponse, IFailure>(validateResponse.Failure!);

            var convertResponse = ResponseConverter.Convert(request, requestId, rawResponse);
            if (!convertResponse.IsSuccess)
                return new Result<WarrantyCaseResponse, IFailure>(convertResponse.Failure!);
            var convertedResponse = convertResponse.Success!;

            if (convertedResponse.Operation == WarrantyCaseOperation.Commit
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
            Guid requestId,
            Result<WarrantyCaseResponse, IFailure> sourceResult)
        {
            if (sourceResult.IsSuccess)
            {
                var warrantyCaseResponse = sourceResult.Success!;
                var warrantyCaseVerification = new WarrantyCaseVerification(request.OrderId)
                {
                    Operation = request.Operation,
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
                            saveWarrantyCaseVerification.Failure!.Message,
                            calledExternalParty: true));
                }
                return new Result<WarrantyCaseResponse, IFailure>(warrantyCaseResponse);
            }
            else
            {
                var performVerifyActionFailure = sourceResult.Failure!;
                bool calledExternalParty = FailureClassification.CalledExternalParty(performVerifyActionFailure);
                bool calledWithResponse = FailureClassification.CalledWithResponse(performVerifyActionFailure);
                bool responseHasNoError = FailureClassification.ResponseHasNoError(performVerifyActionFailure);
                var warrantyCaseVerification = new WarrantyCaseVerification(request.OrderId)
                {
                    Operation = request.Operation,
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
                            saveWarrantyCaseVerification.Failure!.Message,
                            calledExternalParty: calledExternalParty));
                }
                return new Result<WarrantyCaseResponse, IFailure>(performVerifyActionFailure);
            }
        }

        internal VerifyWarrantyCaseResponse BuidVerifyWarrantyCaseResponse(
            VerifyWarrantyCaseRequest request,
            Result<WarrantyCaseResponse, IFailure> sourceResult)
        {
            if (sourceResult.IsSuccess)
            {
                var warrantyCaseResponse = sourceResult.Success!;

                bool isSuccess;
                FailureType? failureType;
                string? failureMessage;

                var successfulConditionResult = SatisfySuccessfulCondition(request, warrantyCaseResponse);
                if (successfulConditionResult.IsSuccess)
                {
                    isSuccess = true;
                    failureType = null;
                    failureMessage = null;
                }
                else
                {
                    var successfulConditionFailure = successfulConditionResult.Failure!;
                    isSuccess = false;
                    failureType = FailureType.SuccessfulConditionFailure;
                    failureMessage = successfulConditionFailure.Message;
                }

                return new VerifyWarrantyCaseResponse
                {
                    IsSuccess = isSuccess,
                    WarrantyCaseResponse = warrantyCaseResponse,
                    FailureType = failureType,
                    FailureMessage = failureMessage
                };
            }
            else
            {
                var failure = sourceResult.Failure!;
                return new VerifyWarrantyCaseResponse
                {
                    IsSuccess = false,
                    FailureType = failure.FailureType,
                    IsNotFound = failure is IIsNotFound isNotFoundFailure
                        ? isNotFoundFailure.IsNotFound
                        : null,
                    FailureMessage = failure.Message
                };
            }
        }

        internal Result<Unit, SuccessfulConditionFailure> SatisfySuccessfulCondition(
            VerifyWarrantyCaseRequest request,
            WarrantyCaseResponse response)
        {
            switch (request.Operation)
            {
                case WarrantyCaseOperation.Create:
                case WarrantyCaseOperation.Verify:
                    return new Result<Unit, SuccessfulConditionFailure>(Unit.Value);
                case WarrantyCaseOperation.Commit:
                    if (response.Conformance
                        && (response.WarrantyCaseStatus == WarrantyCaseStatus.Committed
                            || response.WarrantyCaseStatus == WarrantyCaseStatus.Completed))
                    {
                        return new Result<Unit, SuccessfulConditionFailure>(Unit.Value);
                    }
                    else
                    {
                        return new Result<Unit, SuccessfulConditionFailure>(
                            new SuccessfulConditionFailure("Cancel operation did not have resposne of WarrantyCaseStatus Committed or Completed"));
                    }
                case WarrantyCaseOperation.Cancel:
                    if (response.WarrantyCaseStatus == WarrantyCaseStatus.Cancelled)
                    {
                        return new Result<Unit, SuccessfulConditionFailure>(Unit.Value);
                    }
                    else
                    {
                        return new Result<Unit, SuccessfulConditionFailure>(
                            new SuccessfulConditionFailure("Cancel operation did not have resposne of WarrantyCaseStatus Cancelled"));
                    }
                default:
                    throw new InvalidOperationException();
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

            var getWarrantyProofResult = await ComprehensiveDataWrapper.GetWarrantyProof(latestWarrantyCaseCommit.RequestId);
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
