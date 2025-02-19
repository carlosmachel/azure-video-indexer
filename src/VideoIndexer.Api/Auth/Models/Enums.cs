using System.Text.Json.Serialization;

namespace VideoIndexer.Api.Auth.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ArmAccessTokenScope
{
    Account,
    Project,
    Video
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ArmAccessTokenPermission
{
    Reader,
    Contributor,
    MyAccessAdministrator,
    Owner,
}