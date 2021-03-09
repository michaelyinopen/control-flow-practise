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
    public class ComprehensiveDataWrapper
    {
        public ComprehensiveDataWrapper(
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
                ComprehensiveDataDbContext.WarrantyCaseVerification.Add(warrantyCaseVerification);
                await ComprehensiveDataDbContext.SaveChangesAsync();
                return new Result<Unit, SaveWarrantyCaseVerificationFailure>(Unit.Value);
            }
            catch (Exception e)
            {
                return new Result<Unit, SaveWarrantyCaseVerificationFailure>(
                    new SaveWarrantyCaseVerificationFailure(
                        e.Message,
                        calledExternalParty: null));
            }
        }

        // current means the state other parts of application should use
        // current != latest iff there are unsuccessful calls
        public async Task<Result<WarrantyCaseVerification, GetWarrantyCaseVerificationFailure>> GetCurrentWarrantyCaseVerification(
            string orderId)
        {
            try
            {
                var warrantyCaseVerification = await ComprehensiveDataDbContext.WarrantyCaseVerification
                    .Where(v => v.OrderId == orderId)
                    .Where(v => v.ResponseHasNoError == true)
                    .Where(v => v.FailureType == null)
                    .OrderByDescending(v => v.DateTime)
                    .FirstOrDefaultAsync();
                if (warrantyCaseVerification is null)
                {
                    return new Result<WarrantyCaseVerification, GetWarrantyCaseVerificationFailure>(
                        new GetWarrantyCaseVerificationFailure(
                            $"WarrantyCaseVerification of OrderId: `{orderId}` is not found",
                            isNotFound: true));
                }
                return new Result<WarrantyCaseVerification, GetWarrantyCaseVerificationFailure>(warrantyCaseVerification);
            }
            catch (Exception e)
            {
                return new Result<WarrantyCaseVerification, GetWarrantyCaseVerificationFailure>(
                    new GetWarrantyCaseVerificationFailure(
                        e.Message,
                        isNotFound: null));
            }
        }

        // save warranty proof only when (Operation = commit and WarrantyCaseStatus = Committed)
        public async Task<Result<Unit, SaveWarrantyProofFailure>> SaveWarrantyProof(
            WarrantyProof warrantyProof)
        {
            try
            {
                ComprehensiveDataDbContext.WarrantyProof.Add(warrantyProof);
                await ComprehensiveDataDbContext.SaveChangesAsync();
                return new Result<Unit, SaveWarrantyProofFailure>(Unit.Value);
            }
            catch (Exception e)
            {
                return new Result<Unit, SaveWarrantyProofFailure>(
                    new SaveWarrantyProofFailure(e.Message));
            }
        }

        // get latest commit with successful response (Operation = commit and WarrantyCaseStatus = Committed)
        public async Task<Result<WarrantyCaseVerification, IFailure>> GetLatestWarrantyCaseCommit(
            string orderId)
        {
            try
            {
                var warrantyCaseVerification = await ComprehensiveDataDbContext.WarrantyCaseVerification
                    .Where(v => v.OrderId == orderId)
                    .Where(v => v.ResponseHasNoError == true)
                    .Where(v => v.FailureType == null)
                    .Where(v => v.Operation == WarrantyCaseOperation.Commit)
                    .Where(v => v.WarrantyCaseStatus == WarrantyCaseStatus.Committed)
                    .OrderByDescending(v => v.DateTime)
                    .FirstOrDefaultAsync();
                if (warrantyCaseVerification is null)
                {
                    return new Result<WarrantyCaseVerification, IFailure>(
                        new GetWarrantyCaseVerificationFailure(
                            $"There is no successful commit of OrderId: `{orderId}`.",
                            isNotFound: true));
                }
                return new Result<WarrantyCaseVerification, IFailure>(warrantyCaseVerification);
            }
            catch (Exception e)
            {
                return new Result<WarrantyCaseVerification, IFailure>(
                    new GetWarrantyCaseVerificationFailure(
                        e.Message,
                        isNotFound: null));
            }
        }

        public async Task<Result<WarrantyProof, GetWarrantyProofFailure>> GetWarrantyProof(
            Guid requestId)
        {
            try
            {
                var warrantyProof = await ComprehensiveDataDbContext.WarrantyProof
                    .Where(p => p.RequestId == requestId)
                    .SingleOrDefaultAsync();
                if (warrantyProof is null)
                {
                    return new Result<WarrantyProof, GetWarrantyProofFailure>(
                        new GetWarrantyProofFailure(
                            $"There is no warrantyProof of RequestId: `{requestId}`.",
                            isNotFound: true,
                            requestId: requestId));
                }
                return new Result<WarrantyProof, GetWarrantyProofFailure>(warrantyProof);
            }
            catch (Exception e)
            {
                return new Result<WarrantyProof, GetWarrantyProofFailure>(
                    new GetWarrantyProofFailure(
                        e.Message,
                        isNotFound: null,
                        requestId: requestId));
            }
        }
    }
}
