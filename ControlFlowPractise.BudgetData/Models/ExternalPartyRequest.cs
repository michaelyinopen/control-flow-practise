using ControlFlowPractise.Common;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ControlFlowPractise.BudgetData.Models
{
    public class ExternalPartyRequest
    {
        public int Id { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime DateTime { get; set; } // add index? and combined index?

        public string OrderId { get; set; } // add index? and combined index?

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
