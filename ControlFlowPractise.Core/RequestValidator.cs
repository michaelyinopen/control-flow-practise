using ControlFlowPractise.Common;
using ControlFlowPractise.Common.ControlFlow;
using FluentValidation;
using System;
using System.Linq;

namespace ControlFlowPractise.Core
{
    public class RequestValidator
    {
        public RequestValidator(IValidator<ValidatableRequest> validator)
        {
            Validator = validator;
        }
        private IValidator<ValidatableRequest> Validator { get; }

        public Result<Unit, RequestValidationFailure> Validate(
            VerifyWarrantyCaseRequest request,
            WarrantyCaseOperation operation,
            Guid requestId)
        {
            var validatableRequest = new ValidatableRequest(
                request,
                operation);

            var validationResult = Validator.Validate(validatableRequest);
            if (validationResult.IsValid)
                return new Result<Unit, RequestValidationFailure>(Unit.Value);

            var errorMessages = string.Join(Environment.NewLine, validationResult.Errors.Select(x => x.ErrorMessage));
            return new Result<Unit, RequestValidationFailure>(
                new RequestValidationFailure(
                    $"Request of RequestId: `{requestId}` failed validation:" + Environment.NewLine + errorMessages));
        }
    }

    public class ValidatableRequest
    {
        public ValidatableRequest(
            VerifyWarrantyCaseRequest request,
            WarrantyCaseOperation operation)
        {
            Request = request;
            Operation = operation;
        }
        public VerifyWarrantyCaseRequest Request { get; }
        public WarrantyCaseOperation Operation { get; }
    }

    public class ValidatableRequestValidator : AbstractValidator<ValidatableRequest>
    {
        public ValidatableRequestValidator()
        {
            RuleFor(v => v.Operation).IsInEnum();
            When(v => v.Operation == WarrantyCaseOperation.Create, () =>
            {
                RuleFor(v => v.Request.WarrantyCaseId).Empty();
                RuleFor(v => v.Request.TransactionDateTime).NotNull();
                RuleFor(v => v.Request.OrderId).NotEmpty();
                RuleFor(v => v.Request.ProductId).NotEmpty();
                HasPurchaser();
                HasVendor();
            });
            When(v => v.Operation == WarrantyCaseOperation.Verify, () =>
            {
                RuleFor(v => v.Request.WarrantyCaseId).NotEmpty();
                RuleFor(v => v.Request.TransactionDateTime).NotNull();
                RuleFor(v => v.Request.OrderId).NotEmpty();
                RuleFor(v => v.Request.ProductId).NotEmpty();
                HasPurchaser();
                HasVendor();
            });
            When(v => v.Operation == WarrantyCaseOperation.Commit, () =>
            {
                RuleFor(v => v.Request.WarrantyCaseId).NotEmpty();
                RuleFor(v => v.Request.TransactionDateTime).NotNull();
                RuleFor(v => v.Request.OrderId).NotEmpty();
                RuleFor(v => v.Request.ProductId).NotEmpty();
                HasPurchaser();
                HasVendor();
                RuleFor(v => v.Request.OrderTrackingNumber).NotEmpty();
            });
            When(v => v.Operation == WarrantyCaseOperation.Cancel, () =>
            {
                RuleFor(v => v.Request.WarrantyCaseId).NotEmpty();
            });
        }

        private void HasPurchaser()
        {
            RuleFor(v => v.Request.PurchaserFirstName).NotEmpty();
            RuleFor(v => v.Request.PurchaserLastName).NotEmpty();
            RuleFor(v => v.Request.PurchaserEmail).NotEmpty();
        }

        private void HasVendor()
        {
            RuleFor(v => v.Request.VendorFirstName).NotEmpty();
            RuleFor(v => v.Request.VendorLastName).NotEmpty();
            RuleFor(v => v.Request.VendorEmail).NotEmpty();
            RuleFor(v => v.Request.VendorPhoneNumber).NotEmpty();
        }
    }
}
