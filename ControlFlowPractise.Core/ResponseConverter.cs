using ControlFlowPractise.Common;
using ControlFlowPractise.Common.ControlFlow;
using ControlFlowPractise.ExternalParty;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ControlFlowPractise.Core
{
    // Builds WarrantyCaseResponse (as part of http response)
    // from VerifyWarrantyCaseRequest (from http request)
    // and requestId (generated in WarrantyService)
    // and WarrantyResponse (response from External Party)
    public class ResponseConverter
    {
        public Result<WarrantyCaseResponse, IFailure> Convert(
            VerifyWarrantyCaseRequest request,
            WarrantyCaseOperation operation,
            Guid requestId,
            WarrantyResponse warrantyResponse)
        {
            try
            {
                var warrantyCaseResponse = new WarrantyCaseResponse(
                    orderId: request.OrderId,
                    warrantyCaseId: warrantyResponse.Header.WarrantyCaseId!)
                {
                    Operation = operation,
                    WarrantyCaseStatus = WarrantyCaseStatusMap[warrantyResponse.Body!.CaseStatus],
                    Conformance = ConformanceIndicatorMap[warrantyResponse.Body!.ConformanceIndicator],
                };
                if (warrantyResponse.Body!.CaseStatus != CaseStatus.Cancelled)
                {
                    var orderReport = warrantyResponse.Body!.OrderReports.Single();
                    warrantyCaseResponse.WarrantyEstimatedAmount = orderReport.WarrantyEstimatedAmount;
                    warrantyCaseResponse.WarrantyAmount = orderReport.WarrantyAmount;
                    warrantyCaseResponse.ConformanceMessages = orderReport.ConformanceMessages
                        .Select(m => new WarrantyConformanceMessage(m.Message)
                        {
                            Level = WarrantyConformanceLevelMap[m.Level]
                        })
                        .ToList();
                }
                return new Result<WarrantyCaseResponse, IFailure>(warrantyCaseResponse);
            }
            catch (Exception e)
            {
                return new Result<WarrantyCaseResponse, IFailure>(
                    new ResponseConversionFailure($"Cannot convert response of RequestId: `{requestId}` because:" + Environment.NewLine + e.Message));
            }
        }

        private static IReadOnlyDictionary<CaseStatus, WarrantyCaseStatus> WarrantyCaseStatusMap { get; } =
            new ReadOnlyDictionary<CaseStatus, WarrantyCaseStatus>(
                new Dictionary<CaseStatus, WarrantyCaseStatus>
                {
                    { CaseStatus.WaitingForClaim, WarrantyCaseStatus.WaitingForClaim },
                    { CaseStatus.Claimed, WarrantyCaseStatus.Claimed },
                    { CaseStatus.Certified, WarrantyCaseStatus.Certified },
                    { CaseStatus.Committed, WarrantyCaseStatus.Committed },
                    { CaseStatus.Completed, WarrantyCaseStatus.Completed },
                    { CaseStatus.Cancelled, WarrantyCaseStatus.Cancelled },
                });

        private static IReadOnlyDictionary<ConformanceLevel, WarrantyConformanceLevel> WarrantyConformanceLevelMap { get; } =
            new ReadOnlyDictionary<ConformanceLevel, WarrantyConformanceLevel>(
                new Dictionary<ConformanceLevel, WarrantyConformanceLevel>
                {
                    { ConformanceLevel.Information, WarrantyConformanceLevel.Information },
                    { ConformanceLevel.Warning, WarrantyConformanceLevel.Warning },
                    { ConformanceLevel.Error, WarrantyConformanceLevel.Error },
                });

        private static IReadOnlyDictionary<string, bool> ConformanceIndicatorMap { get; } =
            new ReadOnlyDictionary<string, bool>(
                new Dictionary<string, bool>
                {
                    { "YES", true },
                    { "NO", false },
                });
    }
}
