using ControlFlowPractise.Common;
using System;

namespace ControlFlowPractise.BudgetData.Models
{
    public class ExternalPartyResponse
    {
        public int Id { get; set; }

        public DateTime DateTime { get; set; }

        public string OrderId { get; set; }

        public WarrantyCaseOperation Operation { get; set; }

        public Guid RequestId { get; set; }

        public string Response { get; set; }

        public ExternalPartyResponse(
            string orderId,
            string response)
        {
            OrderId = orderId;
            Response = response;
        }
    }
}
