using System;
using System.Collections.Generic;
using System.Text;

namespace ControlFlowPractise.ExternalParty
{
    public enum WarrantyRequestType
    {
        Verify,
        Commit
    }

    public enum WarrantyRequestAction
    {
        Cancel
    }

    // combinations are
    // (Verify, null) and WarrantyCaseId == null => create warranty case
    // (Verify, null)
    // (Verify, Cancel)
    // (Commit, null)

    public class WarrantyRequest
    {
        public WarrantyRequest(string transactionDate)
        {
            TransactionDate = transactionDate;
        }
        public WarrantyRequestType RequestType { get; set; }
        public WarrantyRequestAction? Action { get; set; }
        public string? WarrantyCaseId { get; set; }
        public string TransactionDate { get; set; }
    }
}
