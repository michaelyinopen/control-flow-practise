using ControlFlowPractise.Common;
using System;

namespace ControlFlowPractise.ComprehensiveData.Models
{
    public class WarrantyCaseVerification
    {
        public int Id { get; set; }

        public DateTime DateTime { get; set; }

        public string OrderId { get; set; }

        public string? WarrantyCaseId { get; set; }

        public WarrantyCaseOperation Operation { get; set; }

        public WarrantyCaseStatus? WarrantyCaseStatus { get; set; }

        public Guid RequestId { get; set; }

        public bool CalledExternalParty { get; set; }

        public bool? CalledWithResponse { get; set; }

        public bool? ResponseHasNoError { get; set; } // can still be not conformant and have ConformanceMessageError

        public FailureType? FailureType { get; set; }

        public string? FailureMessage { get; set; }

        public string? ConvertedResponse { get; set; }

        public WarrantyCaseVerification(
            string orderId)
        {
            OrderId = orderId;
        }
    }
}
