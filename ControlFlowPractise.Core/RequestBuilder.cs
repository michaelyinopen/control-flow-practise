using ControlFlowPractise.Common;
using ControlFlowPractise.Common.ControlFlow;
using ControlFlowPractise.ExternalParty;
using System;
using System.Collections.Generic;

namespace ControlFlowPractise.Core
{
    // Builds WarrantyRequest (that gets sent to External Party)
    // from VerifyWarrantyCaseRequest (from http request)
    // this is a builder that has no internal state
    internal class RequestBuilder
    {
        public Result<WarrantyRequest, RequestConversionFailure> Build(
            VerifyWarrantyCaseRequest verifyWarrantyCaseRequest,
            Guid requestId)
        {
            try
            {
                var (requestType, actionType) = GetRequestTypeActionType(verifyWarrantyCaseRequest.Operation);

                var warrantyRequest = new WarrantyRequest
                {
                    RequestId = requestId,
                    RequestType = requestType,
                    Action = actionType,
                    WarrantyCaseId = verifyWarrantyCaseRequest.WarrantyCaseId,
                    TransactionDate = verifyWarrantyCaseRequest.Operation != WarrantyCaseOperation.Cancel
                        ? GetTransactionDateString(verifyWarrantyCaseRequest.TransactionDateTime)
                        : null,
                    OrderDetails = GetOrderDetails(verifyWarrantyCaseRequest),
                };

                return new Result<WarrantyRequest, RequestConversionFailure>(warrantyRequest);
            }
            catch (Exception e)
            {
                return new Result<WarrantyRequest, RequestConversionFailure>(
                    new RequestConversionFailure($"Cannot build request of RequestId: `{requestId}` because:" + Environment.NewLine + e.Message));
            }
        }

        internal string? GetTransactionDateString(DateTime? sourceDateTime)
        {
            if (!sourceDateTime.HasValue)
                return null;
            var dateTimeOffset = new DateTimeOffset(sourceDateTime.Value, TimeSpan.Zero);
            var tz = TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time");
            DateTimeOffset timeInAusEasternStandardTime = TimeZoneInfo.ConvertTime(dateTimeOffset, tz);
            return timeInAusEasternStandardTime.ToString("yyyy-MM-ddThh:mm:ssK");
        }

        internal (WarrantyRequestType, WarrantyRequestAction?) GetRequestTypeActionType(
            WarrantyCaseOperation operation) =>
            operation switch
            {
                WarrantyCaseOperation.Create => (WarrantyRequestType.Verify, null),
                WarrantyCaseOperation.Verify => (WarrantyRequestType.Verify, null),
                WarrantyCaseOperation.Commit => (WarrantyRequestType.Commit, null),
                WarrantyCaseOperation.Cancel => (WarrantyRequestType.Verify, WarrantyRequestAction.Cancel),
                _ => throw new ArgumentOutOfRangeException()
            };

        internal List<OrderDetail>? GetOrderDetails(
            VerifyWarrantyCaseRequest verifyWarrantyCaseRequest)
        {
            if (verifyWarrantyCaseRequest.Operation == WarrantyCaseOperation.Cancel)
                return null;

            return new List<OrderDetail>
            {
                new OrderDetail(
                    orderId: verifyWarrantyCaseRequest.OrderId,
                    purchaserDetail: new PurchaserDetail(
                        firstName: verifyWarrantyCaseRequest.PurchaserFirstName!,
                        lastName: verifyWarrantyCaseRequest.PurchaserLastName!,
                        email: verifyWarrantyCaseRequest.PurchaserEmail!),
                    vendorDetail: new VendorDetail(
                        firstName: verifyWarrantyCaseRequest.VendorFirstName!,
                        lastName: verifyWarrantyCaseRequest.VendorLastName!,
                        email:verifyWarrantyCaseRequest.VendorEmail!,
                        phoneNumber: verifyWarrantyCaseRequest.VendorPhoneNumber!))
                {
                    ProductDetails = new List<ProductDetail>
                    {
                        new ProductDetail(verifyWarrantyCaseRequest.ProductId!)
                        {
                            Specification =  verifyWarrantyCaseRequest.Specification
                        }
                    },
                    OrderTrackingNumber = verifyWarrantyCaseRequest.Operation == WarrantyCaseOperation.Commit
                        ? verifyWarrantyCaseRequest.OrderTrackingNumber
                        : null
                }
            };
        }
    }
}
