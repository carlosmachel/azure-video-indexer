using Microsoft.AspNetCore.Mvc;
using Scalar.AspNetCore;
using VideoIndexer.Api;
using VideoIndexer.Api.Auth;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var options = builder.Configuration.GetSection("AzureVideoIndexer").Get<AzureVideoIndexerOptions>();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<AzureVideoIndexerOptions>(options);
builder.Services.AddScoped<AuthenticationService>();
builder.Services.AddScoped<AzureVideoIndexerService>();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference(); // scalar/v1
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/videos/upload", async ([FromServices] AzureVideoIndexerService service, [FromServices] AuthenticationService authenticationService,
        [FromQuery] string videoUrl,
        [FromQuery] string videoName) =>
    {
        var armToken = await authenticationService.GetArmAccessTokenAsync();
        var accessToken = await authenticationService.GetAccountAccessTokenAsync(armToken);

        var account = await service.GetAccountAsync(armToken, options.AccountName);
        return Results.Ok(await service.UploadUrlAsync(accessToken, account.Properties.Id, account.Location, videoUrl, videoName));
    })
    .WithName("UploadVideo");


app.MapGet("/videos/index", async ([FromServices] AzureVideoIndexerService service, [FromServices] AuthenticationService authenticationService,
        [FromQuery] string videoId) =>
    {
        var armToken = await authenticationService.GetArmAccessTokenAsync();
        var accessToken = await authenticationService.GetAccountAccessTokenAsync(armToken);

        var account = await service.GetAccountAsync(armToken, options.AccountName);
        var result = await service.IndexAsync(accessToken, account.Properties.Id, account.Location, videoId);
        
        return Results.Text(result, contentType: "application/json");
    })
    .WithName("Index");

app.Run();
