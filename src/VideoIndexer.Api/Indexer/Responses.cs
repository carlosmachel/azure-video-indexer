using System.Text.Json.Serialization;

namespace VideoIndexer.Api;

public record VideoResponse( [property: JsonPropertyName("id")] string Id, [property: JsonPropertyName("state")] string State);

