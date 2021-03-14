using ControlFlowPractise.BudgetData.Models;
using ControlFlowPractise.Common;
using ControlFlowPractise.ComprehensiveData.Models;
using ControlFlowPractise.ExternalParty;
using System;
using System.Collections.Generic;

namespace ControlFlowPractise.Core.Tests.WarrantyServiceTestSetups
{
    // each element in VerifySetups.json will have properties of both TestSetup and VerifyTestCaseData
    public class VerifyTestCaseData
    {
        public string? TargetTest { get; set; }
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

        public VerifyTestCaseData(
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
