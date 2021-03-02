using ControlFlowPractise.Common;
using ControlFlowPractise.Common.ControlFlow;
using ControlFlowPractise.ExternalParty;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlFlowPractise.Core
{
    internal class ExternalPartyWrapper
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

                if (warrantyResponse.Header.WarrantyResponseErrors
                    .FirstOrDefault(e => e.Type == WarrantyResponseErrorType.ServiceNotAvailable)
                    is WarrantyResponseError serviceNotAvailableError)
                {
                    return new Result<WarrantyResponse, IFailure>(
                        new ServiceNotAvailableFailure(serviceNotAvailableError.Message));
                }

                if (warrantyResponse.Header.WarrantyResponseErrors
                    .FirstOrDefault(e => e.Type == WarrantyResponseErrorType.InvalidRequest)
                    is WarrantyResponseError invalidRequestError)
                {
                    return new Result<WarrantyResponse, IFailure>(
                        new InvalidRequestFailure(invalidRequestError.Message));
                }

                if (warrantyResponse.Header.WarrantyResponseErrors
                    .FirstOrDefault(e => e.Type == WarrantyResponseErrorType.InternalError)
                    is WarrantyResponseError warrantyServiceInternalError)
                {
                    return new Result<WarrantyResponse, IFailure>(
                        new WarrantyServiceInternalErrorFailure(warrantyServiceInternalError.Message));
                }

                if (!warrantyResponse.Header.WarrantyResponseErrors.Any())
                    return new Result<WarrantyResponse, IFailure>(warrantyResponse);

                throw new InvalidOperationException();
            }
            catch (NetworkException e)
            {
                return new Result<WarrantyResponse, IFailure>(new NetworkFailure(e.Message));
            }
        }
    }
}
