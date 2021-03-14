using ControlFlowPractise.ComprehensiveData.Models;
using ControlFlowPractise.ExternalParty;
using System.Collections.Generic;

namespace ControlFlowPractise.Core.Tests.WarrantyServiceTestSetups
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

    public class ExternalPartyCall
    {
        public WarrantyRequest ExpectedRequest { get; set; }

        public bool Throws { get; set; }

        public WarrantyResponse? Response { get; set; }

        public ExternalPartyCall(
            WarrantyRequest expectedRequest,
            bool throws,
            WarrantyResponse? response)
        {
            ExpectedRequest = expectedRequest;
            Throws = throws;
            Response = response;
        }
    }
}
