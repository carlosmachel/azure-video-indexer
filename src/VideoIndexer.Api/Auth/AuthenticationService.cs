using System.Net.Http.Headers;
using System.Text.Json;
using Azure.Core;
using Azure.Identity;
using VideoIndexer.Api.Auth.Models;

namespace VideoIndexer.Api.Auth;

public class AuthenticationService(IHttpClientFactory httpClientFactory, AzureVideoIndexerOptions options)
{
    public static async Task<string> GetArmAccessTokenAsync(CancellationToken ct = default)
    {
        var credentials = new DefaultAzureCredential();
        var tokenRequestContext = new TokenRequestContext([$"https://management.azure.com/.default"]);
        var tokenRequestResult = await credentials.GetTokenAsync(tokenRequestContext, ct);
        return tokenRequestResult.Token;
    }
    
    public async Task<string> GetAccountAccessTokenAsync(
        string armAccessToken, 
        ArmAccessTokenPermission permission = ArmAccessTokenPermission.Contributor, 
        ArmAccessTokenScope scope = ArmAccessTokenScope.Account, 
        string videoId = null, 
        CancellationToken ct = default)
    {
        var accessTokenRequest = new AccessTokenRequest(permission, scope, VideoId: videoId);
        
        var jsonRequestBody = JsonSerializer.Serialize(accessTokenRequest);
        Console.WriteLine($"Getting Account access token: {jsonRequestBody}");
        var httpContent = new StringContent(jsonRequestBody, System.Text.Encoding.UTF8, "application/json");

        // Set request uri
        var requestUri = $"https://management.azure.com/subscriptions/{options.SubscriptionId}/resourcegroups/{options.ResourceGroup}/providers/Microsoft.VideoIndexer/accounts/{options.AccountName}/generateAccessToken?api-version={options.ApiVersion}";
        var client = httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", armAccessToken);

        var result = await client.PostAsync(requestUri, httpContent, ct);
        result.EnsureSuccessStatusCode();
        var jsonResponseBody = await result.Content.ReadAsStringAsync(ct);
        Console.WriteLine($"Got Account access token: {scope} , {permission}");
        return JsonSerializer.Deserialize<GenerateAccessTokenResponse>(jsonResponseBody)?.AccessToken!;
        
    }

}