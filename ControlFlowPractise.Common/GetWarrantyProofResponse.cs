using Newtonsoft.Json;
using System;

namespace ControlFlowPractise.Common
{
    public class GetWarrantyProofResponse
    {
        public bool IsSuccess { get; set; }

        public string OrderId { get; set; }

        public string? WarrantyCaseId { get; set; }

        public Guid? RequestId { get; set; }

        public string? WarrantyProof { get; set; }

        public FailureType? FailureType { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsNotFound { get; set; }

        public string? FailureMessage { get; set; }

        public GetWarrantyProofResponse(string orderId)
        {
            OrderId = orderId;
        }
    }
}
