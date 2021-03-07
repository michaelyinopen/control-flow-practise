using ControlFlowPractise.Common;
using ControlFlowPractise.Common.ControlFlow;
using ControlFlowPractise.ExternalParty;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ControlFlowPractise.Core
{
    public class ExternalPartyWrapper
    {
        public ExternalPartyWrapper(IExternalPartyProxy externalPartyProxy)
        {
            ExternalPartyProxy = externalPartyProxy;
        }

        private IExternalPartyProxy ExternalPartyProxy { get; }

        public async Task<Result<WarrantyResponse, IFailure>> Call(WarrantyRequest request)
        {
            try
            {
                var warrantyResponse = await ExternalPartyProxy.Call(request);
                return new Result<WarrantyResponse, IFailure>(warrantyResponse);
            }
            catch (NetworkException e)
            {
                return new Result<WarrantyResponse, IFailure>(new NetworkFailure(e.Message));
            }
        }
    }
}
