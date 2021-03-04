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
        // Calls PerformVerifyAction
        // Saves warrantyCaseVerification in ComprehensiveData, regardless of PerformVerifyAction is success or failure
        //
        // todo handle cases where PerformVerifyAction is successful, but ConformanceIndicator/CaseStatus doe not meet requirement for success
        // e.g. Commit but CaseStatus remains at Certified
        // e.g. Cancel but CaseStatus remains at Claimed
        public async Task<Result<VerifyWarrantyCaseResponse, IFailure>> Verify(
            VerifyWarrantyCaseRequest request)
        {
            var requestId = Guid.NewGuid();
            var performVerifyActionResult = await PerformVerifyAction(request, requestId);
            if (performVerifyActionResult.IsSuccess)
            {
                var verifyWarrantyCaseResponse = performVerifyActionResult.Success!;
                var warrantyCaseVerification = new WarrantyCaseVerification(request.OrderId)
                {
                    Operation = request.Operation,
                    RequestId = requestId,
                    CalledExternalParty = true,
                    CalledWithResponse = true,
                    ResponseHasNoError = true,
                    ConvertedResponse = JsonConvert.SerializeObject(verifyWarrantyCaseResponse)
                };
                var saveWarrantyCaseVerification =
                    await ComprehensiveDataWrapper.SaveWarrantyCaseVerification(warrantyCaseVerification);
                if (!saveWarrantyCaseVerification.IsSuccess)
                {
                    return new Result<VerifyWarrantyCaseResponse, IFailure>(
                        new SaveWarrantyCaseVerificationFailure(
                            saveWarrantyCaseVerification.Failure!.Message,
                            calledExternalParty: true));
                }
                return new Result<VerifyWarrantyCaseResponse, IFailure>(verifyWarrantyCaseResponse);
            }
            else
            {
                var performVerifyActionFailure = performVerifyActionResult.Failure!;
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
                    // FailureType
                    // FailureMessage
                };
                var saveWarrantyCaseVerification =
                    await ComprehensiveDataWrapper.SaveWarrantyCaseVerification(warrantyCaseVerification);
                if (!saveWarrantyCaseVerification.IsSuccess)
                {
                    return new Result<VerifyWarrantyCaseResponse, IFailure>(
                        new SaveWarrantyCaseVerificationFailure(
                            saveWarrantyCaseVerification.Failure!.Message,
                            calledExternalParty: calledExternalParty));
                }
                return new Result<VerifyWarrantyCaseResponse, IFailure>(performVerifyActionFailure);
            }
        }

        // success means called Thrid Party, saved raw request and raw response, and returns a converted response
        // anything after SaveExternalPartyResponse in BudgetDatabase should be pure (non-deterministic and no side-effect)
        internal async Task<Result<VerifyWarrantyCaseResponse, IFailure>> PerformVerifyAction(
            VerifyWarrantyCaseRequest request,
            Guid requestId)
        {
            var validateRequest = RequestValidator.Validate(request, requestId);
            if (!validateRequest.IsSuccess)
                return new Result<VerifyWarrantyCaseResponse, IFailure>(validateRequest.Failure!);

            var build = RequestBuilder.Build(request, requestId);
            if (!build.IsSuccess)
                return new Result<VerifyWarrantyCaseResponse, IFailure>(build.Failure!);
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
                return new Result<VerifyWarrantyCaseResponse, IFailure>(saveWarrantyRequest.Failure!);

            var call = await ExternalPartyWrapper.Call(warrantyRequest);
            if (!call.IsSuccess)
                return new Result<VerifyWarrantyCaseResponse, IFailure>(call.Failure!);
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
                return new Result<VerifyWarrantyCaseResponse, IFailure>(saveWarrantyRequest.Failure!);

            var validateResponse = ResponseValidator.Validate(rawResponse);
            if (!validateResponse.IsSuccess)
                return new Result<VerifyWarrantyCaseResponse, IFailure>(validateResponse.Failure!);

            var convertResponse = ResponseConverter.Convert(rawResponse);
            if (!convertResponse.IsSuccess)
                return new Result<VerifyWarrantyCaseResponse, IFailure>(convertResponse.Failure!);
            var convertedResponse = convertResponse.Success!;

            // save warranty proof

            return new Result<VerifyWarrantyCaseResponse, IFailure>(convertedResponse);
        }

        public async Task<Result<VerifyWarrantyCaseResponse, IFailure>> GetVerificationResult(string orderId)
        {
            var warrantyCaseVerificationResult = await ComprehensiveDataWrapper.GetCurrentWarrantyCaseVerification(orderId);
            if (!warrantyCaseVerificationResult.IsSuccess)
                return new Result<VerifyWarrantyCaseResponse, IFailure>(warrantyCaseVerificationResult.Failure!);
            var warrantyCaseVerification = warrantyCaseVerificationResult.Success!;

            try
            {
                var verifyWarrantyCaseResponse = JsonConvert.DeserializeObject<VerifyWarrantyCaseResponse>(
                    warrantyCaseVerification.ConvertedResponse!);
                return new Result<VerifyWarrantyCaseResponse, IFailure>(verifyWarrantyCaseResponse);
            }
            catch (JsonException)
            {
                return new Result<VerifyWarrantyCaseResponse, IFailure>(
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
