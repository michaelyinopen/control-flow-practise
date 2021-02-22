using System;
using System.Collections.Generic;
using System.Text;

namespace ControlFlowPractise.ExternalParty
{
    public interface IExternalPartyProxy
    {
        // returns, or throws a NetworkException
        public WarrantyResponse Call(WarrantyRequest request);
    }

    public class ExternalPartyProxy
    {
        public WarrantyResponse Call(WarrantyRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
