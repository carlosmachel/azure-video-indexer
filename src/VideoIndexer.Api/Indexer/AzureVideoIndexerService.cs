using System.Net.Http.Headers;
using System.Text.Json;
using System.Web;
using VideoIndexer.Api.Auth;

namespace VideoIndexer.Api;

public class AzureVideoIndexerService(IHttpClientFactory httpClientFactory, AuthenticationService authService, AzureVideoIndexerOptions options)
{
    
    /// <summary>
    /// Get Information about the Account
    /// </summary>
    /// <param name="accountName"></param>
    /// <returns></returns>
    public async Task<Account> GetAccountAsync(string armToken, string accountName)
    {
        var requestUri = $"https://management.azure.com/subscriptions/{options.SubscriptionId}/resourcegroups/{options.ResourceGroup}/providers/Microsoft.VideoIndexer/accounts/{accountName}?api-version={options.ApiVersion}";
        var client = httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", armToken);

        var result = await client.GetAsync(requestUri);

        if (result.StatusCode != System.Net.HttpStatusCode.OK)
        {
            throw new Exception(result.ToString());
        }
        
        var jsonResponseBody = await result.Content.ReadAsStringAsync();
        var account = JsonSerializer.Deserialize<Account>(jsonResponseBody);
        
        if (string.IsNullOrWhiteSpace(account.Location) || account.Properties == null || string.IsNullOrWhiteSpace(account.Properties.Id))
        {
            throw new Exception($"Account {accountName} not found.");
        }
        return account;
    }
    
    /// <summary>
    /// Uploads a video and starts the video index. Calls the uploadVideo API (https://api-portal.videoindexer.ai/api-details#api=Operations&operation=Upload-Video)
    /// </summary>
    /// <param name="videoUrl"> Link To Publicy Accessed Video URL</param>
    /// <param name="videoName"> The Asset name to be used </param>
    /// <param name="exludedAIs"> The ExcludeAI list to run </param>
    /// <param name="waitForIndex"> should this method wait for index operation to complete </param>
    /// <exception cref="Exception"></exception>
    /// <returns> Video Id of the video being indexed, otherwise throws excpetion</returns>
    public async Task<string> UploadUrlAsync(
        string accessToken, 
        string accountId, 
        string location, 
        string videoUrl , 
        string videoName, 
        string exludedAIs = null)
    {
      
        //Build Query Parameter Dictionary
        var queryDictionary = new Dictionary<string, string>
        {
            { "name", videoName },
            { "description", "video_description" },
            { "privacy", "private" },
            { "accessToken" , accessToken },
            { "videoUrl" , videoUrl }
        };

        if (!Uri.IsWellFormedUriString(videoUrl, UriKind.Absolute))
        {
            throw new ArgumentException("VideoUrl or LocalVidePath are invalid");
        }

        var queryParams = CreateQueryString(queryDictionary);
        if (!string.IsNullOrEmpty(exludedAIs))
            queryParams += AddExcludedAIs(exludedAIs);

        // Send POST request
        var url = $"{options.ApiEndpoint}/{location}/Accounts/{accountId}/Videos?{queryParams}";
        var client = httpClientFactory.CreateClient();
        var result = await client.PostAsync(url, null);

        if (result.StatusCode != System.Net.HttpStatusCode.OK)
        {
            var content = await result.Content.ReadAsStringAsync();
            throw new Exception(content);
        }

        var uploadResult = await result.Content.ReadAsStringAsync();

        // Get the video ID from the upload result
        var videoId = JsonSerializer.Deserialize<VideoResponse>(uploadResult).Id;
        
        return videoId;
    }
    
    private static string AddExcludedAIs(string ExcludedAI)
    {
        if (string.IsNullOrEmpty(ExcludedAI))
        {
            return "";
        }
        var list = ExcludedAI.Split(',');
        return list.Aggregate("", (current, item) => current + ("&ExcludedAI=" + item));
    }
    
    private static string CreateQueryString(IDictionary<string, string> parameters)
    {
        var queryParameters = HttpUtility.ParseQueryString(string.Empty);
        foreach (var parameter in parameters)
        {
            queryParameters[parameter.Key] = parameter.Value;
        }
        return queryParameters.ToString();
    }
}