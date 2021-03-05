using Newtonsoft.Json.Converters;
using System.Text.Json.Serialization;

namespace ControlFlowPractise.Common
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum FailureType
    {
        RequestValidationFailure,
        RequestConversionFailure,
        GetWarrantyCaseVerificationFailure,
        SaveExternalPartyRequestFailure,
        GetExternalPartyRequestFailure,
        SaveExternalPartyResponseFailure,
        GetExternalPartyResponseFailure,
        SaveWarrantyProofFailure,
        GetWarrantyProofFailure,
        ResponseValidationFailure,
        ResponseConversionFailure,
        SaveWarrantyCaseVerificationFailure,
        SuccessfulConditionFailure,
        NetworkFailure,
        ServiceNotAvailableFailure,
        InvalidRequestFailure,
        WarrantyServiceInternalErrorFailure,
        WarrantyCaseCancelledFailure,
        // VerifyBeforeCommitFailure
    }

    public interface IFailure
    {
        public FailureType FailureType { get; }
        public string Message { get; }
    }

    #region internal failures
    public class RequestValidationFailure : IFailure
    {
        public FailureType FailureType { get; } = FailureType.RequestValidationFailure;
        public string Message { get; }
        public RequestValidationFailure(string message)
        {
            Message = message;
        }
    }

    public class RequestConversionFailure : IFailure
    {
        public FailureType FailureType { get; } = FailureType.RequestConversionFailure;
        public string Message { get; }
        public RequestConversionFailure(string message)
        {
            Message = message;
        }
    }

    public class GetWarrantyCaseVerificationFailure : IFailure
    {
        public FailureType FailureType { get; } = FailureType.GetWarrantyCaseVerificationFailure;
        public string Message { get; }
        public bool IsNotFound { get; }
        public GetWarrantyCaseVerificationFailure(string message, bool isNotFound)
        {
            Message = message;
            IsNotFound = isNotFound;
        }
    }

    public class SaveExternalPartyRequestFailure : IFailure
    {
        public FailureType FailureType { get; } = FailureType.SaveExternalPartyRequestFailure;
        public string Message { get; }
        public SaveExternalPartyRequestFailure(string message)
        {
            Message = message;
        }
    }

    public class GetExternalPartyRequestFailure : IFailure
    {
        public FailureType FailureType { get; } = FailureType.GetExternalPartyRequestFailure;
        public string Message { get; }
        public bool IsNotFound { get; }
        public GetExternalPartyRequestFailure(string message, bool isNotFound)
        {
            Message = message;
            IsNotFound = isNotFound;
        }
    }

    public class SaveExternalPartyResponseFailure : IFailure
    {
        public FailureType FailureType { get; } = FailureType.SaveExternalPartyResponseFailure;
        public string Message { get; }
        public SaveExternalPartyResponseFailure(string message)
        {
            Message = message;
        }
    }

    public class GetExternalPartyResponseFailure : IFailure
    {
        public FailureType FailureType { get; } = FailureType.GetExternalPartyResponseFailure;
        public string Message { get; }
        public bool IsNotFound { get; }
        public GetExternalPartyResponseFailure(string message, bool isNotFound)
        {
            Message = message;
            IsNotFound = isNotFound;
        }
    }

    public class SaveWarrantyProofFailure : IFailure
    {
        public FailureType FailureType { get; } = FailureType.SaveWarrantyProofFailure;
        public string Message { get; }
        public SaveWarrantyProofFailure(string message)
        {
            Message = message;
        }
    }

    public class GetWarrantyProofFailure : IFailure
    {
        public FailureType FailureType { get; } = FailureType.GetWarrantyProofFailure;
        public string Message { get; }
        public bool IsNotFound { get; }
        public GetWarrantyProofFailure(string message, bool isNotFound)
        {
            Message = message;
            IsNotFound = isNotFound;
        }
    }

    public class ResponseValidationFailure : IFailure
    {
        public FailureType FailureType { get; } = FailureType.ResponseValidationFailure;
        public string Message { get; }
        public ResponseValidationFailure(string message)
        {
            Message = message;
        }
    }

    public class ResponseConversionFailure : IFailure
    {
        public FailureType FailureType { get; } = FailureType.ResponseConversionFailure;
        public string Message { get; }
        public ResponseConversionFailure(string message)
        {
            Message = message;
        }
    }

    public class SaveWarrantyCaseVerificationFailure : IFailure
    {
        public FailureType FailureType { get; } = FailureType.SaveWarrantyCaseVerificationFailure;
        public string Message { get; }
        public bool? CalledExternalParty { get; } // ture, false or unknown
        public SaveWarrantyCaseVerificationFailure(
            string message,
            bool? calledExternalParty)
        {
            Message = message;
            CalledExternalParty = calledExternalParty;
        }
    }

    public class SuccessfulConditionFailure : IFailure
    {
        public FailureType FailureType { get; } = FailureType.SuccessfulConditionFailure;
        public string Message { get; }
        public SuccessfulConditionFailure(string message)
        {
            Message = message;
        }
    }
    #endregion internal failures

    public class NetworkFailure : IFailure
    {
        public FailureType FailureType { get; } = FailureType.NetworkFailure;
        public string Message { get; }
        public NetworkFailure(string message)
        {
            Message = message;
        }
    }

    #region external party returned failures
    public class ServiceNotAvailableFailure : IFailure
    {
        public FailureType FailureType { get; } = FailureType.ServiceNotAvailableFailure;
        public string Message { get; }
        public ServiceNotAvailableFailure(string message)
        {
            Message = message;
        }
    }

    public class InvalidRequestFailure : IFailure
    {
        public FailureType FailureType { get; } = FailureType.InvalidRequestFailure;
        public string Message { get; }
        public InvalidRequestFailure(string message)
        {
            Message = message;
        }
    }

    public class WarrantyServiceInternalErrorFailure : IFailure
    {
        public FailureType FailureType { get; } = FailureType.WarrantyServiceInternalErrorFailure;
        public string Message { get; }
        public WarrantyServiceInternalErrorFailure(string message)
        {
            Message = message;
        }
    }
    #endregion external party returned error

    public class WarrantyCaseCancelledFailure : IFailure
    {
        public FailureType FailureType { get; } = FailureType.WarrantyCaseCancelledFailure;
        public string Message { get; }
        public WarrantyCaseCancelledFailure(string message)
        {
            Message = message;
        }
    }
}
