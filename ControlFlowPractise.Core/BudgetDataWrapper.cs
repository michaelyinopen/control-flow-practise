using ControlFlowPractise.BudgetData;
using ControlFlowPractise.BudgetData.Models;
using ControlFlowPractise.Common;
using ControlFlowPractise.Common.ControlFlow;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ControlFlowPractise.Core
{
    public class BudgetDataWrapper
    {
        public BudgetDataWrapper(
            BudgetDataDbContext budgetDataDbContext)
        {
            BudgetDataDbContext = budgetDataDbContext;
        }

        private BudgetDataDbContext BudgetDataDbContext { get; }

        public async Task<Result<Unit, SaveExternalPartyRequestFailure>> SaveExternalPartyRequest(
            ExternalPartyRequest externalPartyRequest)
        {
            try
            {
                BudgetDataDbContext.ExternalPartyRequest.Add(externalPartyRequest);
                await BudgetDataDbContext.SaveChangesAsync();
                return new Result<Unit, SaveExternalPartyRequestFailure>(Unit.Value);
            }
            catch (Exception e)
            {
                return new Result<Unit, SaveExternalPartyRequestFailure>(
                    new SaveExternalPartyRequestFailure(e.Message));
            }
        }

        public async Task<Result<Unit, SaveExternalPartyResponseFailure>> SaveExternalPartyResponse(
            ExternalPartyResponse externalPartyResponse)
        {
            try
            {
                BudgetDataDbContext.ExternalPartyResponse.Add(externalPartyResponse);
                await BudgetDataDbContext.SaveChangesAsync();
                return new Result<Unit, SaveExternalPartyResponseFailure>(Unit.Value);
            }
            catch (Exception e)
            {
                return new Result<Unit, SaveExternalPartyResponseFailure>(
                    new SaveExternalPartyResponseFailure(e.Message));
            }
        }

        public async Task<Result<ExternalPartyRequest, GetExternalPartyRequestFailure>> GetExternalPartyRequest(
            string orderId,
            Guid requestId)
        {
            try
            {
                var externalPartyRequest = await BudgetDataDbContext.ExternalPartyRequest
                    .Where(req => req.OrderId == orderId)
                    .Where(req => req.RequestId == requestId)
                    .FirstOrDefaultAsync();
                if (externalPartyRequest is null)
                {
                    return new Result<ExternalPartyRequest, GetExternalPartyRequestFailure>(
                        new GetExternalPartyRequestFailure(
                            $"ExternalPartyRequest of OrderId: `{orderId}`, RequestId: `{requestId}` is not found",
                            isNotFound: true));
                }
                return new Result<ExternalPartyRequest, GetExternalPartyRequestFailure>(externalPartyRequest);
            }
            catch (Exception e)
            {
                return new Result<ExternalPartyRequest, GetExternalPartyRequestFailure>(
                    new GetExternalPartyRequestFailure(
                        e.Message,
                        isNotFound: false));
            }
        }

        public async Task<Result<ExternalPartyResponse, GetExternalPartyResponseFailure>> GetExternalPartyResponse(
            string orderId,
            Guid requestId)
        {
            try
            {
                var externalPartyRequest = await BudgetDataDbContext.ExternalPartyResponse
                    .Where(req => req.OrderId == orderId)
                    .Where(req => req.RequestId == requestId)
                    .FirstOrDefaultAsync();
                if (externalPartyRequest is null)
                {
                    return new Result<ExternalPartyResponse, GetExternalPartyResponseFailure>(
                        new GetExternalPartyResponseFailure(
                            $"ExternalPartyResponse of OrderId: `{orderId}`, RequestId: `{requestId}` is not found",
                            isNotFound: true));
                }
                return new Result<ExternalPartyResponse, GetExternalPartyResponseFailure>(externalPartyRequest);
            }
            catch (Exception e)
            {
                return new Result<ExternalPartyResponse, GetExternalPartyResponseFailure>(
                    new GetExternalPartyResponseFailure(
                        e.Message,
                        isNotFound: false));
            }
        }
    }
}
