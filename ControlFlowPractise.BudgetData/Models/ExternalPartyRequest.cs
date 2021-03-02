using ControlFlowPractise.Common;
using System;

namespace ControlFlowPractise.BudgetData.Models
{
    public class ExternalPartyRequest
    {
        public int Id { get; set; }

        public DateTime DateTime { get; set; }

        public string OrderId { get; set; }

        public WarrantyCaseOperation Operation { get; set; }

        public Guid RequestId { get; set; }

        public string Request { get; set; }

        public ExternalPartyRequest(
            string orderId,
            string request)
        {
            OrderId = orderId;
            Request = request;
        }
    }
}
