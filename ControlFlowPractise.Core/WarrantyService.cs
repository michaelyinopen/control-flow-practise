using ControlFlowPractise.Common;
using System;
using System.Threading.Tasks;

namespace ControlFlowPractise.Core
{
    public class WarrantyService
    {
        // result type, to handle errors? Monadic?
        public async Task<VerifyWarrantyCaseResponse> Verify(VerifyWarrantyCaseRequest request)
        {
            throw new NotImplementedException();
        }

        public async Task<VerifyWarrantyCaseResponse> GetVerificationResult(string orderId)
        {
            throw new NotImplementedException();
        }
    }
}
