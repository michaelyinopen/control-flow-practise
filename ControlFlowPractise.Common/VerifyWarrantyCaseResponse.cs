using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace ControlFlowPractise.Common
{
    public class VerifyWarrantyCaseResponse
    {
        public WarrantyCaseOperation Operation { get; set; }

        public string OrderId { get; set; }

        public string WarrantyCaseId { get; set; }

        public WarrantyCaseStatus WarrantyCaseStatus { get; set; }

        public bool Conformance { get; set; }

        public decimal? WarrantyEstimatedAmount { get; set; }

        public decimal? WarrantyAmount { get; set; }

        public List<WarrantyConformanceMessage> ConformanceMessages { get; set; } =
            new List<WarrantyConformanceMessage>();

        public VerifyWarrantyCaseResponse(string orderId, string warrantyCaseId)
        {
            OrderId = orderId;
            WarrantyCaseId = warrantyCaseId;
        }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum WarrantyCaseStatus
    {
        WaitingForClaim,
        Claimed,
        Certified,
        Committed,
        Completed,
        Cancelled
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum WarrantyConformanceLevel
    {
        Information,
        Warning,
        Error
    }

    public class WarrantyConformanceMessage
    {
        public WarrantyConformanceMessage(string message)
        {
            Message = message;
        }
        public WarrantyConformanceLevel Level { get; set; }
        public string Message { get; set; }
    }
}
