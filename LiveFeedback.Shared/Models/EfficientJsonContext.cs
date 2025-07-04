using System.Text.Json.Serialization;

namespace LiveFeedback.Shared.Models;

[JsonSerializable(typeof(Client))]
[JsonSerializable(typeof(ComprehensibilityInformation))]
[JsonSerializable(typeof(Lecture))]
[JsonSerializable(typeof(MessageCarrier<>))]
[JsonSerializable(typeof(RatingMessage<>))]
public partial class EfficientJsonContext : JsonSerializerContext;