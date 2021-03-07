using ControlFlowPractise.Common;
using System;

namespace ControlFlowPractise.Core
{
    public class FailureClassification
    {
        public bool CalledExternalParty(IFailure failure)
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

        public bool CalledWithResponse(IFailure failure)
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

        public bool ResponseHasNoError(IFailure failure)
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
