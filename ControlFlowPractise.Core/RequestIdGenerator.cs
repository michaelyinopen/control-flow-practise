using System;
using System.Collections.Generic;
using System.Text;

namespace ControlFlowPractise.Core
{
    // Replaces Guid.NewGuid()
    // Just to facilate testing
    public interface IRequestIdGenerator
    {
        Guid GenerateRequestId();
    }

    public class RequestIdGenerator : IRequestIdGenerator
    {
        public Guid GenerateRequestId() => Guid.NewGuid();
    }
}
