using System.Text.Json.Serialization;

namespace Shared.Common.Messaging;

public interface IDeduplicated
{
    [JsonIgnore]
    string DeduplicationKey { get; }
}