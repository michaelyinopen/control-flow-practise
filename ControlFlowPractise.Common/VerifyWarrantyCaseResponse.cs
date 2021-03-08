using Newtonsoft.Json;

namespace ControlFlowPractise.Common
{
    public class VerifyWarrantyCaseResponse
    {
        public bool IsSuccess { get; set; }

        public WarrantyCaseResponse? WarrantyCaseResponse { get; set; }

        public FailureType? FailureType { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsNotFound { get; set; }

        public string? FailureMessage { get; set; }
    }
}
