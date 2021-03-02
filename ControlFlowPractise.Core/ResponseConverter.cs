using ControlFlowPractise.Common;
using ControlFlowPractise.Common.ControlFlow;
using ControlFlowPractise.ExternalParty;
using System;
using System.Collections.Generic;
using System.Text;

namespace ControlFlowPractise.Core
{
    // Builds VerifyWarrantyCaseResponse (as http response)
    // from WarrantyResponse (response from External Party)
    // can be more(e.g. function?)
    // this is a converter that has no internal state
    internal class ResponseConverter
    {
        public Result<VerifyWarrantyCaseResponse, IFailure> Convert(
            WarrantyResponse warrantyResponse)
        {
            throw new NotImplementedException();
        }
    }
}
