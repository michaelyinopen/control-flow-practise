using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ControlFlowPractise.Common
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum WarrantyCaseOperation
    {
        Create,
        Verify,
        Commit,
        Cancel
    }
}
