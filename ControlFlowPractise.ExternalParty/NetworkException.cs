using System;
using System.Collections.Generic;
using System.Text;

namespace ControlFlowPractise.ExternalParty
{
    public class NetworkException : Exception
    {
        public NetworkException(string? message)
            : base(message)
        { }
    }
}
