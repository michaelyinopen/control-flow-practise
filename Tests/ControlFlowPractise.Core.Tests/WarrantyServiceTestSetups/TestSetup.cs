using ControlFlowPractise.ComprehensiveData.Models;
using System.Collections.Generic;

namespace ControlFlowPractise.Core.Tests.WarrantyServiceTestData
{
    public class TestSetup
    {
        public string? TargetTest { get; set; }
        public List<WarrantyCaseVerificationTestSetup> WarrantyCaseVerificationTestSetups { get; set; } =
            new List<WarrantyCaseVerificationTestSetup>();
        public List<WarrantyProof> WarrantyProofs { get; set; } = new List<WarrantyProof>();
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
