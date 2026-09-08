using GemBidScraper.Data;
using GemBidScraper.Services;
using GemBidScraper.Services.CategoryClassification;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using Microsoft.Extensions.Hosting.WindowsServices;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------
// WINDOWS SERVICE
// ---------------------------------------------------------

builder.Host.UseWindowsService(options =>
{
    options.ServiceName = "GemBidScraperService";
});

// ---------------------------------------------------------
// SERILOG FILE LOGGING
// ---------------------------------------------------------

Directory.CreateDirectory(@"C:\GemBidScraperPublish\Logs");

builder.Host.UseSerilog((context, config) =>
{
    config
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
        .WriteTo.Console()
        .WriteTo.File(
            @"C:\GemBidScraperPublish\Logs\log-.txt",
            rollingInterval: RollingInterval.Day
        );
});

// ---------------------------------------------------------
// CONTROLLERS
// ---------------------------------------------------------

builder.Services.AddControllers();

// ---------------------------------------------------------
// DATABASE
// ---------------------------------------------------------

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.CommandTimeout(180);

            // Required for a 500k+ bid scrape: transient SQL failures
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null);
        });


});

// ---------------------------------------------------------
// BACKGROUND SCRAPER SERVICE
// ---------------------------------------------------------

builder.Services.AddHostedService<
    GemBidScraper.Background.ScraperBackgroundService>();

// ---------------------------------------------------------
// CORS
// ---------------------------------------------------------

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// ---------------------------------------------------------
// SWAGGER / OPEN API
// ---------------------------------------------------------

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ---------------------------------------------------------
// HTTP CLIENTS
// ---------------------------------------------------------

builder.Services.AddHttpClient();

builder.Services.AddScoped<BrowserService>();

builder.Services.AddHttpClient<PythonApiService>();

// ---------------------------------------------------------
// API SERVICES
// ---------------------------------------------------------


// ---------------------------------------------------------
// CATEGORY CLASSIFICATION
// ---------------------------------------------------------

builder.Services.AddSingleton<CategoryClassifier>();
builder.Services.AddMemoryCache();

// ---------------------------------------------------------
// PYTHON PARSER API
// ---------------------------------------------------------

builder.Services.AddHttpClient<PythonParserService>(client =>
{
    client.BaseAddress = new Uri("http://127.0.0.1:8000/");
    client.Timeout = TimeSpan.FromHours(6);
});

// ---------------------------------------------------------
// BUILD APPLICATION
// ---------------------------------------------------------

var app = builder.Build();

// ---------------------------------------------------------
// HTTP REQUEST PIPELINE
// ---------------------------------------------------------

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowReact");

app.UseAuthorization();

app.MapControllers();

app.Run();