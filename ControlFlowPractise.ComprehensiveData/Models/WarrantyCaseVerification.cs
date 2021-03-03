using ControlFlowPractise.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ControlFlowPractise.ComprehensiveData.Models
{
    public class WarrantyCaseVerification
    {
        public int Id { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime DateTime { get; set; } // add index? // follow budgetdata

        public string OrderId { get; set; } // add index? and combined index?

        public string? WarrantyCaseId { get; set; }

        public WarrantyCaseOperation Operation { get; set; } // make it save as string

        public Guid RequestId { get; set; }

        public bool CalledExternalParty { get; set; }

        public bool? CalledWithResponse { get; set; }

        public bool? ResponseHasNoError { get; set; } // can still be not conformant and have ConformanceMessageError

        public FailureType? FailureType { get; set; } // make it save as string

        public string? FailureMessage { get; set; }

        // serialized object, any useful fields can be extracted and have their own column
        public string? ConvertedResponse { get; set; }

        public WarrantyCaseVerification(
            string orderId)
        {
            OrderId = orderId;
        }
    }
}
