using ControlFlowPractise.Common;
using ControlFlowPractise.Common.ControlFlow;
using ControlFlowPractise.ComprehensiveData;
using ControlFlowPractise.ComprehensiveData.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ControlFlowPractise.Core
{
    internal class ComprehensiveDataWrapper
    {
        internal ComprehensiveDataWrapper(
            ComprehensiveDataDbContext comprehensiveDataDbContext)
        {
            ComprehensiveDataDbContext = comprehensiveDataDbContext;
        }

        private ComprehensiveDataDbContext ComprehensiveDataDbContext { get; }

        public async Task<Result<Unit, SaveWarrantyCaseVerificationFailure>> SaveWarrantyCaseVerification(
            WarrantyCaseVerification warrantyCaseVerification)
        {
            try
            {
                ComprehensiveDataDbContext.Add(warrantyCaseVerification);
                await ComprehensiveDataDbContext.SaveChangesAsync();
                return new Result<Unit, SaveWarrantyCaseVerificationFailure>(Unit.Value);
            }
            catch (Exception e)
            {
                return new Result<Unit, SaveWarrantyCaseVerificationFailure>(
                    new SaveWarrantyCaseVerificationFailure(e.Message, null));
            }
        }

        // current means the state other parts of application should use
        // current != latest iff there are failed calls
        public async Task<Result<WarrantyCaseVerification, IFailure>> GetCurrentWarrantyCaseVerification(
            string orderId)
        {
            try
            {
                var warrantyCaseVerification = await ComprehensiveDataDbContext.WarrantyCaseVerification
                    .Where(v => v.OrderId == orderId)
                    .OrderByDescending(v => v.DateTime)
                    .FirstOrDefaultAsync();
                if (warrantyCaseVerification is null)
                {
                    return new Result<WarrantyCaseVerification, IFailure>(
                        new WarrantyCaseVerificationNotFoundFailure($"WarrantyCaseVerification of OrderId: `{orderId}` is not found"));
                }
                else
                {
                    return new Result<WarrantyCaseVerification, IFailure>(warrantyCaseVerification);
                }
            }
            catch (Exception e)
            {
                return new Result<WarrantyCaseVerification, IFailure>(
                    new ReadWarrantyCaseVerificationFailure(e.Message));
            }
        }

        // todo add save and get warranty proof
    }
}
