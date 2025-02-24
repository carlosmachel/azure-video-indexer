using System.Text.Json.Serialization;

namespace VideoIndexer.Api.Auth.Models;

public record AccessTokenRequest(
    [property: JsonPropertyName("permissionType")] ArmAccessTokenPermission PermissionType,
    [property: JsonPropertyName("scope")] ArmAccessTokenScope Scope,
    [property: JsonPropertyName("projectId")] string ProjectId = null,
    [property: JsonPropertyName("videoId")] string VideoId = null);
    
public record GenerateAccessTokenResponse([property: JsonPropertyName("accessToken")] string AccessToken);
    