namespace ControlFlowPractise.Common
{
    public class GetCurrentWarrantyCaseVerificationResponse
    {
        public bool IsFound { get; set; }
        public WarrantyCaseResponse? WarrantyCaseResponse { get; set; }
        public FailureType? FailureType { get; set; }
        public string? FailureMessage { get; set; }
    }
}
