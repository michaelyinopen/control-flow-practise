using System;
using System.Collections.Generic;
using System.Text;

namespace ControlFlowPractise.Common
{
    public interface IFailures
    {
        public string Message { get; }
        public bool FailureAfterCallingExternalParty { get; }
    }

    #region internal failures
    public class RequestValidationFailure : IFailures
    {
        public string Message { get; }
        public bool FailureAfterCallingExternalParty { get; }
        public RequestValidationFailure(string message)
        {
            Message = message;
        }
    }

    public class RequestConversionFailure : IFailures
    {
        public string Message { get; }
        public bool FailureAfterCallingExternalParty { get; }
        public RequestConversionFailure(string message)
        {
            Message = message;
        }
    }

    // read old request failure

    // other create request failure

    public class SaveRequestFailure : IFailures
    {
        public string Message { get; }
        public bool FailureAfterCallingExternalParty { get; }
        public SaveRequestFailure(string message)
        {
            Message = message;
        }
    }

    #region FailureAfterCallingExternalParty
    public class SaveRawResponseFailure : IFailures
    {
        public string Message { get; }
        public bool FailureAfterCallingExternalParty { get; } = true;
        public SaveRawResponseFailure(string message)
        {
            Message = message;
        }
    }
    public class ResponseValidationFailure : IFailures
    {
        public string Message { get; }
        public bool FailureAfterCallingExternalParty { get; } = true;
        public ResponseValidationFailure(string message)
        {
            Message = message;
        }
    }

    public class ResponseConversionFailure : IFailures
    {
        public string Message { get; }
        public bool FailureAfterCallingExternalParty { get; } = true;
        public ResponseConversionFailure(string message)
        {
            Message = message;
        }
    }

    public class SaveConvertedResponseFailure : IFailures
    {
        public string Message { get; }
        public bool IsAfterCallingExternalParty { get; } = true;
        public bool FailureAfterCallingExternalParty { get; } = true;
        public SaveConvertedResponseFailure(string message)
        {
            Message = message;
        }
    }
    #endregion FailureAfterCallingExternalParty
    #endregion internal failures

    public class NetworkFailure : IFailures
    {
        public string Message { get; }
        public bool FailureAfterCallingExternalParty { get; }
        public NetworkFailure(string message)
        {
            Message = message;
        }
    }

    #region external party returned failures
    public class ServiceNotAvailableFailure : IFailures
    {
        public string Message { get; }
        public bool FailureAfterCallingExternalParty { get; }
        public ServiceNotAvailableFailure(string message)
        {
            Message = message;
        }
    }

    public class InvalidRequestFailure : IFailures
    {
        public string Message { get; }
        public bool FailureAfterCallingExternalParty { get; }
        public InvalidRequestFailure(string message)
        {
            Message = message;
        }
    }

    public class WarrantyServiceInternalErrorFailure : IFailures
    {
        public string Message { get; }
        public bool FailureAfterCallingExternalParty { get; }
        public WarrantyServiceInternalErrorFailure(string message)
        {
            Message = message;
        }
    }
    #endregion external party returned error
}
