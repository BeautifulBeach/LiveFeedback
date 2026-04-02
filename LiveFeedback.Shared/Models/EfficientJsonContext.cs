using System.Text.Json.Serialization;

namespace LiveFeedback.Shared.Models;

[JsonSerializable(typeof(Client))]
[JsonSerializable(typeof(ComprehensibilityInformation))]
[JsonSerializable(typeof(Lecture))]
[JsonSerializable(typeof(List<Lecture>))]
[JsonSerializable(typeof(MessageCarrier<string>))]
[JsonSerializable(typeof(MessageCarrier<ushort>))]
[JsonSerializable(typeof(RatingMessage<ushort>))]
[JsonSerializable(typeof(Task))]
public partial class EfficientJsonContext : JsonSerializerContext;