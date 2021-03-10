using ControlFlowPractise.Common;
using ControlFlowPractise.Common.ControlFlow;
using ControlFlowPractise.ExternalParty;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ControlFlowPractise.Core
{
    // validates WarrantyResponse (response from External Party)
    // responses with WarrantyResponseError in header will return special failures
    // ---
    // conformanceIndicator NO or
    // ConformanceIndicator/CaseStatus doe not meet requirement for success
    // could still be valid responses
    public class ResponseValidator
    {
        public ResponseValidator(IValidator<ValidatableResponse> validator)
        {
            Validator = validator;
        }
        private IValidator<ValidatableResponse> Validator { get; }

        public Result<Unit, IFailure> Validate(
            VerifyWarrantyCaseRequest request,
            WarrantyCaseOperation operation,
            Guid requestId,
            WarrantyResponse warrantyResponse)
        {
            if (warrantyResponse.Header.WarrantyResponseErrors
                .FirstOrDefault(e => e.Type == WarrantyResponseErrorType.ServiceNotAvailable)
                is WarrantyResponseError serviceNotAvailableError)
            {
                return new Result<Unit, IFailure>(
                    new ServiceNotAvailableFailure(serviceNotAvailableError.Message));
            }

            if (warrantyResponse.Header.WarrantyResponseErrors
                .FirstOrDefault(e => e.Type == WarrantyResponseErrorType.InvalidRequest)
                is WarrantyResponseError invalidRequestError)
            {
                return new Result<Unit, IFailure>(
                    new InvalidRequestFailure(invalidRequestError.Message));
            }

            if (warrantyResponse.Header.WarrantyResponseErrors
                .FirstOrDefault(e => e.Type == WarrantyResponseErrorType.InternalError)
                is WarrantyResponseError warrantyServiceInternalError)
            {
                return new Result<Unit, IFailure>(
                    new WarrantyServiceInternalErrorFailure(warrantyServiceInternalError.Message));
            }

            var validatableResponse = new ValidatableResponse(
                request,
                operation,
                requestId,
                warrantyResponse);
            var validationResult = Validator.Validate(validatableResponse);
            if (validationResult.IsValid)
                return new Result<Unit, IFailure>(Unit.Value);

            var errorMessages = string.Join(Environment.NewLine, validationResult.Errors.Select(x => x.ErrorMessage));
            return new Result<Unit, IFailure>(
                new ResponseValidationFailure(
                    $"Response of RequestId: `{requestId}` failed validation:" + Environment.NewLine + errorMessages));
        }

        public class ValidatableResponse
        {
            public ValidatableResponse(
                VerifyWarrantyCaseRequest request,
                WarrantyCaseOperation operation,
                Guid requestId,
                WarrantyResponse warrantyResponse)
            {
                Request = request;
                Operation = operation;
                RequestId = requestId;
                WarrantyResponse = warrantyResponse;
            }
            public VerifyWarrantyCaseRequest Request { get; }
            public WarrantyCaseOperation Operation { get; }
            public Guid RequestId { get; }
            public WarrantyResponse WarrantyResponse { get; }
        }

        public class ValidatableResponseValidator : AbstractValidator<ValidatableResponse>
        {
            public ValidatableResponseValidator()
            {
                RuleFor(y => y.WarrantyResponse.Header).NotNull();
                When(y => y.WarrantyResponse.Header != null, () =>
                {
                    RuleFor(y => y.WarrantyResponse.Header.RequestId).Equal(y => y.RequestId);
                    RuleFor(y => y.WarrantyResponse.Header.RequestType).Equal(y => OperationRequestTypeMap[y.Operation]);
                    RuleFor(y => y.WarrantyResponse.Header.Action).Equal(y => OperationActionMap[y.Operation]);
                    RuleFor(y => y.WarrantyResponse.Header.WarrantyCaseId).NotEmpty();
                    When(y => y.Operation != WarrantyCaseOperation.Create, () =>
                    {
                        RuleFor(y => y.WarrantyResponse.Header.WarrantyCaseId).Equal(y => y.Request.WarrantyCaseId);
                    });
                    RuleFor(y => y.WarrantyResponse.Header.WarrantyResponseErrors).Empty();
                });

                RuleFor(y => y.WarrantyResponse.Body).NotNull();
                When(y => y.WarrantyResponse.Body != null, () =>
                {
                    RuleFor(y => y.WarrantyResponse.Body!.CaseStatus).IsInEnum();
                    RuleFor(y => y.WarrantyResponse.Body!.ConformanceIndicator)
                        .Must(c => c.Equals("YES") || c.Equals("NO"));
                    When(y => y.WarrantyResponse.Body!.CaseStatus != CaseStatus.Cancelled, () =>
                    {
                        RuleFor(y => y.WarrantyResponse.Body!.OrderReports)
                            .Cascade(CascadeMode.Stop)
                            .NotEmpty()
                            .Must(rs => rs.Count() == 1)
                            .WithMessage("Must have one element in {PropertyName}.")
                            .Must((y, rs) => rs.Single().OrderId == y.Request.OrderId)
                            .WithMessage("OrderReport's OrderId must be the same as the request's.");
                        When(y => y.WarrantyResponse.Body!.OrderReports.Count() == 1, () =>
                        {
                            RuleForEach(y => y.WarrantyResponse.Body!.OrderReports).ChildRules(orderReportRule =>
                            {
                                orderReportRule.RuleFor(r => r.ConformanceIndicator)
                                    .Must(c => c.Equals("YES") || c.Equals("NO"));
                                orderReportRule.RuleForEach(r => r.ConformanceMessages).ChildRules(conformanceMessageRule =>
                                {
                                    conformanceMessageRule.RuleFor(m => m.Level).IsInEnum();
                                    conformanceMessageRule.RuleFor(m => m.Message).NotEmpty();
                                });
                            });
                        });
                        When(y => y.WarrantyResponse.Body!.CaseStatus == CaseStatus.Certified, () =>
                        {
                            RuleForEach(y => y.WarrantyResponse.Body!.OrderReports).ChildRules(orderReportRule =>
                            {
                                orderReportRule.RuleFor(r => r.ConformanceIndicator).Equal("YES");
                                orderReportRule.RuleFor(r => r.WarrantyAmount).Must(a => a.HasValue);
                            });
                        });
                        When(y =>
                            y.WarrantyResponse.Body!.CaseStatus == CaseStatus.Committed
                                || y.WarrantyResponse.Body!.CaseStatus == CaseStatus.Completed,
                            () =>
                        {
                            RuleForEach(y => y.WarrantyResponse.Body!.OrderReports).ChildRules(orderReportRule =>
                            {
                                orderReportRule.RuleFor(r => r.ConformanceIndicator).Equal("YES");
                                orderReportRule.RuleFor(r => r.WarrantyAmount).Must(a => a.HasValue);
                                orderReportRule.RuleFor(r => r.WarrantyProof).NotEmpty();
                            });
                        });
                    });
                });
            }

            private static IReadOnlyDictionary<WarrantyCaseOperation, WarrantyRequestType> OperationRequestTypeMap { get; } =
                new ReadOnlyDictionary<WarrantyCaseOperation, WarrantyRequestType>(
                    new Dictionary<WarrantyCaseOperation, WarrantyRequestType>
                    {
                        { WarrantyCaseOperation.Create, WarrantyRequestType.Verify },
                        { WarrantyCaseOperation.Verify, WarrantyRequestType.Verify },
                        { WarrantyCaseOperation.Commit, WarrantyRequestType.Commit },
                        { WarrantyCaseOperation.Cancel, WarrantyRequestType.Verify },
                    });

            private static IReadOnlyDictionary<WarrantyCaseOperation, WarrantyRequestAction?> OperationActionMap { get; } =
                new ReadOnlyDictionary<WarrantyCaseOperation, WarrantyRequestAction?>(
                    new Dictionary<WarrantyCaseOperation, WarrantyRequestAction?>
                    {
                        { WarrantyCaseOperation.Create, null },
                        { WarrantyCaseOperation.Verify, null },
                        { WarrantyCaseOperation.Commit, null },
                        { WarrantyCaseOperation.Cancel, WarrantyRequestAction.Cancel },
                    });
        }
    }
}
