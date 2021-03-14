using System;

namespace ControlFlowPractise.ComprehensiveData.Models
{
    public class WarrantyProof
    {
        public int Id { get; set; }

        public DateTime DateTime { get; set; }

        public string OrderId { get; set; }

        public string WarrantyCaseId { get; set; }

        public Guid RequestId { get; set; }// get by OrderId and RequestId

        public string Proof { get; set; }

        public WarrantyProof(string orderId, string warrantyCaseId, string proof)
        {
            OrderId = orderId;
            WarrantyCaseId = warrantyCaseId;
            Proof = proof;
        }
    }
}
