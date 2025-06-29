using System.Text.Json.Serialization;

namespace LiveFeedback.Models;

[JsonSourceGenerationOptions(
    PropertyNameCaseInsensitive = true,
    WriteIndented = true,
    IncludeFields = true
)]
[JsonSerializable(typeof(LocalConfig))]
[JsonSerializable(typeof(ServerConfig))]
public partial class JsonContext : JsonSerializerContext;