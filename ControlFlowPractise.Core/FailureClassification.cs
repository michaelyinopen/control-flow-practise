using ControlFlowPractise.Common;
using System;

namespace ControlFlowPractise.Core
{
    internal class FailureClassification
    {
        internal bool CalledExternalParty(IFailure failure)
        {
            return failure switch
            {
                NetworkFailure _ => true,
                ServiceNotAvailableFailure _ => true,
                InvalidRequestFailure _ => true,
                WarrantyServiceInternalErrorFailure _ => true,
                SaveExternalPartyResponseFailure _ => true,
                ResponseValidationFailure _ => true,
                ResponseConversionFailure _ => true,
                // save warranty proof
                SaveWarrantyCaseVerificationFailure _ => throw new InvalidOperationException(),
                _ => false
            };
        }

        internal bool CalledWithResponse(IFailure failure)
        {
            return failure switch
            {
                ServiceNotAvailableFailure _ => true,
                InvalidRequestFailure _ => true,
                WarrantyServiceInternalErrorFailure _ => true,
                SaveExternalPartyResponseFailure _ => true,
                ResponseValidationFailure _ => true,
                ResponseConversionFailure _ => true,
                // save warranty proof
                SaveWarrantyCaseVerificationFailure _ => throw new InvalidOperationException(),
                _ => false
            };
        }

        internal bool ResponseHasNoError(IFailure failure)
        {
            return failure switch
            {
                SaveExternalPartyResponseFailure _ => true,
                ResponseValidationFailure _ => true,
                ResponseConversionFailure _ => true,
                // save warranty proof
                SaveWarrantyCaseVerificationFailure _ => throw new InvalidOperationException(),
                _ => false
            };
        }
    }
}
