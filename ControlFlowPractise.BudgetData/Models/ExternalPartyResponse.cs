using ControlFlowPractise.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ControlFlowPractise.BudgetData.Models
{
    public class ExternalPartyResponse
    {
        public int Id { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime DateTime { get; set; } // add index? and combined index?

        public string OrderId { get; set; } // add index? and combined index?

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
