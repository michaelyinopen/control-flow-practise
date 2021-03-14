using ControlFlowPractise.BudgetData.Models;
using ControlFlowPractise.ComprehensiveData.Models;
using System.Collections.Generic;

namespace ControlFlowPractise.Core.Tests.WarrantyServiceTestSetups
{
    public class TestSetup
    {
        public List<WarrantyCaseVerificationTestSetup> WarrantyCaseVerificationTestSetups { get; set; } =
            new List<WarrantyCaseVerificationTestSetup>();
        public List<WarrantyProof> WarrantyProofs { get; set; } = new List<WarrantyProof>();
        public List<ExternalPartyRequest> ExternalPartyRequests { get; set; } = new List<ExternalPartyRequest>();
        public List<ExternalPartyResponse> ExternalPartyResponses { get; set; } = new List<ExternalPartyResponse>();
    }

    public class WarrantyCaseVerificationTestSetup
    {
        public int InsertOrder { get; set; }
        public WarrantyCaseVerification WarrantyCaseVerification { get; set; }
        public WarrantyCaseVerificationTestSetup(WarrantyCaseVerification warrantyCaseVerification)
        {
            WarrantyCaseVerification = warrantyCaseVerification;
        }
    }
}
