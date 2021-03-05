using ControlFlowPractise.Common;
using System;

namespace ControlFlowPractise.ComprehensiveData.Models
{
    public class WarrantyCaseVerification
    {
        public int Id { get; set; }

        public DateTime DateTime { get; set; } // add index?

        public string OrderId { get; set; } // add index? and combined index?

        public string? WarrantyCaseId { get; set; } // add index?

        public WarrantyCaseOperation Operation { get; set; } // add index?

        public WarrantyCaseStatus? WarrantyCaseStatus { get; set; } // add index?

        public Guid RequestId { get; set; }

        public bool CalledExternalParty { get; set; }

        public bool? CalledWithResponse { get; set; }

        // add index? and combined index?
        public bool? ResponseHasNoError { get; set; } // can still be not conformant and have ConformanceMessageError

        public FailureType? FailureType { get; set; } // add index? and combined index?

        public string? FailureMessage { get; set; }

        public string? ConvertedResponse { get; set; }

        public WarrantyCaseVerification(
            string orderId)
        {
            OrderId = orderId;
        }
    }
}
