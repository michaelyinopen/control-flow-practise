using System;
using System.Collections.Generic;
using System.Text;

namespace ControlFlowPractise.ExternalParty
{
    public enum CaseStatus
    {
        WaitingForClaim = 0,
        Claimed = 1,
        Certified = 2,
        Committed = 3,
        Completed = 4,
        Cancelled = 5
    }

    public enum ConformanceLevel
    {
        Information = 0,
        Warning = 1,
        Error = 2
    }

    public class ConformanceMessage
    {
        public ConformanceMessage(string message)
        {
            Message = message;
        }
        public ConformanceLevel Level { get; set; }
        public string Message { get; set; }
    }

    public class OrderReport
    {
        public OrderReport(string orderId, string conformanceIndicator)
        {
            OrderId = orderId;
            ConformanceIndicator = conformanceIndicator;
        }
        public string OrderId { get; set; }
        public string ConformanceIndicator { get; set; } // YES or NO
        public decimal? WarrantyEstimatedAmount { get; set; }
        // When ConformanceIndicator is YES, WarrantyAmount is the final quote
        public decimal? WarrantyAmount { get; set; }
        public List<ConformanceMessage> ConformanceMessages { get; set; } = new List<ConformanceMessage>();
    }

    public class WarrantyResponse
    {
        public WarrantyResponse(string conformanceIndicator)
        {
            ConformanceIndicator = conformanceIndicator;
        }
        public CaseStatus CaseStatus { get; set; }
        public string ConformanceIndicator { get; set; } // YES or NO
        public List<OrderReport> OrderReports { get; set; } = new List<OrderReport>();
    }
}
