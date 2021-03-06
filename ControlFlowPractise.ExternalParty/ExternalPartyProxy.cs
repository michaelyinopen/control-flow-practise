﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ControlFlowPractise.ExternalParty
{
    public interface IExternalPartyProxy
    {
        // returns, or throws a NetworkException
        public Task<WarrantyResponse> Call(WarrantyRequest request);
    }

    public class ExternalPartyProxy : IExternalPartyProxy
    {
        public async Task<WarrantyResponse> Call(WarrantyRequest request)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch
            {
                throw new NetworkException("Exception when calling External Party.");
            }
        }
    }
}
