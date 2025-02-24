namespace VideoIndexer.Api;

public record AzureVideoIndexerOptions(
    string SubscriptionId, 
    string ResourceGroup,
    string AccountName,
    string ApiEndpoint, 
    string ApiVersion);