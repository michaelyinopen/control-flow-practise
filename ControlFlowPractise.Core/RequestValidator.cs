using ControlFlowPractise.Common;
using ControlFlowPractise.Common.ControlFlow;
using FluentValidation;
using System;
using System.Linq;

namespace ControlFlowPractise.Core
{
    public class RequestValidator
    {
        public RequestValidator(IValidator<VerifyWarrantyCaseRequest> validator)
        {
            Validator = validator;
        }
        private IValidator<VerifyWarrantyCaseRequest> Validator { get; }

        public Result<Unit, RequestValidationFailure> Validate(
            VerifyWarrantyCaseRequest verifyWarrantyCaseRequest,
            Guid requestId)
        {
            var validationResult = Validator.Validate(verifyWarrantyCaseRequest);
            if (validationResult.IsValid)
                return new Result<Unit, RequestValidationFailure>(Unit.Value);

            var errorMessages = string.Join(Environment.NewLine, validationResult.Errors.Select(x => x.ErrorMessage));
            return new Result<Unit, RequestValidationFailure>(
                new RequestValidationFailure(
                    $"Request of RequestId: `{requestId}` failed validation:" + Environment.NewLine + errorMessages));
        }
    }

    public class VerifyWarrantyCaseRequestValidator : AbstractValidator<VerifyWarrantyCaseRequest>
    {
        public VerifyWarrantyCaseRequestValidator()
        {
            RuleFor(req => req.Operation).IsInEnum();
            When(req => req.Operation == WarrantyCaseOperation.Create, () => {
                RuleFor(req => req.WarrantyCaseId).Empty();
                RuleFor(req => req.TransactionDateTime).NotNull();
                RuleFor(req => req.OrderId).NotEmpty();
                RuleFor(req => req.ProductId).NotEmpty();
                HasPurchaser();
                HasVendor();
            });
            When(req => req.Operation == WarrantyCaseOperation.Verify, () => {
                RuleFor(req => req.WarrantyCaseId).NotEmpty();
                RuleFor(req => req.TransactionDateTime).NotNull();
                RuleFor(req => req.OrderId).NotEmpty();
                RuleFor(req => req.ProductId).NotEmpty();
                HasPurchaser();
                HasVendor();
            });
            When(req => req.Operation == WarrantyCaseOperation.Commit, () => {
                RuleFor(req => req.WarrantyCaseId).NotEmpty();
                RuleFor(req => req.TransactionDateTime).NotNull();
                RuleFor(req => req.OrderId).NotEmpty();
                RuleFor(req => req.ProductId).NotEmpty();
                HasPurchaser();
                HasVendor();
                RuleFor(req => req.OrderTrackingNumber).NotEmpty();
            });
            When(req => req.Operation == WarrantyCaseOperation.Cancel, () => {
                RuleFor(req => req.WarrantyCaseId).NotEmpty();
            });
        }

        private void HasPurchaser()
        {
            RuleFor(req => req.PurchaserFirstName).NotEmpty();
            RuleFor(req => req.PurchaserLastName).NotEmpty();
            RuleFor(req => req.PurchaserEmail).NotEmpty();
        }

        private void HasVendor()
        {
            RuleFor(req => req.VendorFirstName).NotEmpty();
            RuleFor(req => req.VendorLastName).NotEmpty();
            RuleFor(req => req.VendorEmail).NotEmpty();
            RuleFor(req => req.VendorPhoneNumber).NotEmpty();
        }
    }
}
