using ControlFlowPractise.BudgetData.Models;
using ControlFlowPractise.Common;
using ControlFlowPractise.Common.ControlFlow;
using ControlFlowPractise.ComprehensiveData.Models;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace ControlFlowPractise.Core
{
    // add extension method for DI registration of this service
    public class WarrantyService
    {
        internal WarrantyService(
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

        // success means called Thrid Party, saved raw request and raw response, and returns a converted response
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

            // save warranty proof

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
                    RequestId = requestId,
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
                    if (response.WarrantyCaseStatus == WarrantyCaseStatus.Committed
                        || response.WarrantyCaseStatus == WarrantyCaseStatus.Completed)
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

        // todo add type for GetCurrentWarrantyCaseVerificationResponse
        public async Task<Result<WarrantyCaseResponse, GetWarrantyCaseVerificationFailure>> GetCurrentWarrantyCaseVerification(string orderId)
        {
            var warrantyCaseVerificationResult = await ComprehensiveDataWrapper.GetCurrentWarrantyCaseVerification(orderId);
            if (!warrantyCaseVerificationResult.IsSuccess)
                return new Result<WarrantyCaseResponse, GetWarrantyCaseVerificationFailure>(warrantyCaseVerificationResult.Failure!);
            var warrantyCaseVerification = warrantyCaseVerificationResult.Success!;

            try
            {
                var warrantyCaseResponse = JsonConvert.DeserializeObject<WarrantyCaseResponse>(
                    warrantyCaseVerification.ConvertedResponse!);
                return new Result<WarrantyCaseResponse, GetWarrantyCaseVerificationFailure>(warrantyCaseResponse);
            }
            catch (JsonException)
            {
                return new Result<WarrantyCaseResponse, GetWarrantyCaseVerificationFailure>(
                    new GetWarrantyCaseVerificationFailure(
                        $"VerificationResult of OrderId: `{orderId}` has cannot be deserialized from response of WarrantyCaseVerification of RequestId: `{warrantyCaseVerification.RequestId}`.",
                        isNotFound: false));
            }
        }

        public async Task<Result<WarrantyProof, IFailure>> GetWarrantyProof(string orderId)
        {
            throw new NotImplementedException();
        }
    }
}
