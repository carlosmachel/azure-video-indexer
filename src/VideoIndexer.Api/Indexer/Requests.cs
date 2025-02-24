using System.Text.Json.Serialization;

namespace VideoIndexer.Api;

public record Account( [property: JsonPropertyName("properties")] AccountProperties Properties, [property: JsonPropertyName("location")] string Location);

public record AccountProperties([property: JsonPropertyName("accountId")] string Id);

