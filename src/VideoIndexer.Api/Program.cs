using Microsoft.AspNetCore.Mvc;
using VideoIndexer.Api;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var options = builder.Configuration.GetSection("AzureVideoIndexer").Get<AzureVideoIndexerOptions>();
builder.Services.AddSingleton<AzureVideoIndexerOptions>(options);
builder.Services.AddScoped<AzureVideoIndexerService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/videos/upload",async ([FromServices] AzureVideoIndexerService service,
        [FromQuery] string videoUrl, 
        [FromQuery] string videoName) 
        => Results.Ok(service.UploadVideoByUrlAsync(videoUrl, videoName)))
    .WithName("UploadVideo");

app.Run();
