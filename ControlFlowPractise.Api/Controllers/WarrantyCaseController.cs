using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ControlFlowPractise.Common;
using ControlFlowPractise.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ControlFlowPractise.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WarrantyCaseController : ControllerBase
    {
        private readonly IWarrantyService _warrantyService;

        public WarrantyCaseController(
            IWarrantyService warrantyService)
        {
            _warrantyService = warrantyService;
        }

        [HttpGet]
        public async Task<GetCurrentWarrantyCaseVerificationResponse> Get(string orderId)
        {
            return await _warrantyService.GetCurrentWarrantyCaseVerification(orderId);
        }

        //[HttpPost]
        //public async Task<VerifyWarrantyCaseResponse> Get(string orderId)
        //{
        //    return await _warrantyService.GetCurrentWarrantyCaseVerification(orderId);
        //}

        [HttpGet("warranty-proof")]
        public async Task<GetWarrantyProofResponse> GetWarrantyProof(string orderId)
        {
            return await _warrantyService.GetWarrantyProof(orderId);
        }
    }
}
