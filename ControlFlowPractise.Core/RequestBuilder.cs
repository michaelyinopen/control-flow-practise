using ControlFlowPractise.Common;
using ControlFlowPractise.Common.ControlFlow;
using ControlFlowPractise.ExternalParty;
using System;
using System.Collections.Generic;
using System.Text;

namespace ControlFlowPractise.Core
{
    // Builds WarrantyRequest (that gets sent to External Party)
    // from VerifyWarrantyCaseRequest (from http request)
    // can be more(e.g. function?)
    // this is a builder that has no internal state
    internal class RequestBuilder
    {
        public Result<WarrantyRequest, IFailure> Build(
            VerifyWarrantyCaseRequest verifyWarrantyCaseRequest,
            Guid requestId)
        {
            throw new NotImplementedException();
        }
    }
}
