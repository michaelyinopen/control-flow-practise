using ControlFlowPractise.Common;
using ControlFlowPractise.Common.ControlFlow;
using ControlFlowPractise.ExternalParty;
using System;
using System.Collections.Generic;
using System.Text;

namespace ControlFlowPractise.Core
{
    // validates WarrantyResponse (response from External Party)
    internal class ResponseValidator
    {
        public Result<Unit, ResponseValidationFailure> Validate(
            WarrantyResponse warrantyResponse)
        {
            throw new NotImplementedException();
        }
    }
}
