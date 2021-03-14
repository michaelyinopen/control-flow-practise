using ControlFlowPractise.BudgetData.Models;
using ControlFlowPractise.Common;
using ControlFlowPractise.ComprehensiveData.Models;
using ControlFlowPractise.ExternalParty;
using System;
using System.Collections.Generic;

namespace ControlFlowPractise.Core.Tests.WarrantyServiceTestSetups
{
    public class TestSetup
    {
        public string? TargetTest { get; set; }
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

    public class VerifyTestSetup : TestSetup
    {
        public VerifyWarrantyCaseRequest Request { get; set; }
        public List<Guid> RequestIds { get; set; } = new List<Guid>();
        public List<ExternalPartyCall> ExternalPartyCalls { get; set; } = new List<ExternalPartyCall>();
        public VerifyWarrantyCaseResponse ExpectedResponse { get; set; }
        public int ExpectedWarrantyCaseVerificationCount { get; set; }
        public List<WarrantyCaseVerification> ExpectedWarrantyCaseVerifications { get; set; } = new List<WarrantyCaseVerification>();
        public int ExpectedWarrantyProofCount { get; set; }
        public List<WarrantyProof?> ExpectedWarrantyProofs { get; set; } = new List<WarrantyProof?>();
        public int ExpectedExternalPartyRequestCount { get; set; }
        public List<ExternalPartyRequest?> ExpectedExternalPartyRequests { get; set; } = new List<ExternalPartyRequest?>();
        public int ExpectedExternalPartyResponseCount { get; set; }
        public List<ExternalPartyResponse?> ExpectedExternalPartyResponses { get; set; } = new List<ExternalPartyResponse?>();

        public VerifyTestSetup(
            VerifyWarrantyCaseRequest request,
            VerifyWarrantyCaseResponse expectedResponse)
        {
            Request = request;
            ExpectedResponse = expectedResponse;
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
