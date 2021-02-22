using System;
using System.Collections.Generic;
using System.Text;

namespace ControlFlowPractise.Common
{
    public enum WarrantyCaseOperation
    {
        Create,
        Verify,
        Commit,
        Cancel
    }

    public class VerifyWarrantyCaseRequest
    {
        public WarrantyCaseOperation Operation { get; set; }
        public string? WarrantyCaseId { get; set; }
        public DateTime? TransactionDateTime { get; set; }
        public string? OrderId { get; set; }
        public string? ProductId { get; set; }
        public string? Specification { get; set; }
        public string? PurchaserFirstName { get; set; }
        public string? PurchaserLastName { get; set; }
        public string? PurchaserEmail { get; set; }
        public string? VendorFirstName { get; set; }
        public string? VendorLastName { get; set; }
        public string? VendorEmail { get; set; }
        public string? VendorPhoneNumber { get; set; }
        public string? OrderTrackingNumber { get; set; } // only needed for commit
    }
}
