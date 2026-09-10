using GemBidScraper.Data;
using GemBidScraper.Services;
using GemBidScraper.Services.CategoryClassification;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using Microsoft.Extensions.Hosting.WindowsServices;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// WINDOWS SERVICE
// ============================================================
builder.Host.UseWindowsService(options =>
{
    options.ServiceName = "GemBidScraperService";
});


// ============================================================
// SERILOG
// ============================================================
// Store logs next to the published application.
// Example:
// D:\JemsPublish\GemBidScraper.exe
// D:\JemsPublish\Logs\log-20260909.txt
//
// This works automatically on both development and server
// machines without hard-coded computer-specific paths.
// ============================================================
var logDirectory = Path.Combine(
    AppContext.BaseDirectory,
    "Logs"
);

Directory.CreateDirectory(logDirectory);

builder.Host.UseSerilog((context, config) =>
{
    config
        .MinimumLevel.Information()

        // Reduce noisy EF Core logs
        .MinimumLevel.Override(
            "Microsoft.EntityFrameworkCore",
            Serilog.Events.LogEventLevel.Warning
        )

        // Console logging
        .WriteTo.Console()

        // File logging
        .WriteTo.File(
            Path.Combine(logDirectory, "log-.txt"),
            rollingInterval: RollingInterval.Day
        );
});


// ============================================================
// CONTROLLERS
// ============================================================
builder.Services.AddControllers();


// ============================================================
// DATABASE
// ============================================================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.CommandTimeout(180);

            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null
            );
        });
});


// ============================================================
// BACKGROUND SCRAPER
// ============================================================
builder.Services.AddHostedService<
    GemBidScraper.Background.ScraperBackgroundService
>();


// ============================================================
// CORS
// ============================================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


// ============================================================
// SWAGGER
// ============================================================
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// ============================================================
// HTTP CLIENTS
// ============================================================
builder.Services.AddHttpClient();

builder.Services.AddScoped<BrowserService>();

builder.Services.AddHttpClient<PythonApiService>();


// ============================================================
// CATEGORY CLASSIFICATION
// ============================================================
builder.Services.AddSingleton<CategoryClassifier>();

builder.Services.AddMemoryCache();


// ============================================================
// PYTHON PARSER
// ============================================================
// Python FastAPI service runs locally on the configured port.
// ScraperBackgroundService starts/stops the Python service
// automatically when a scheduled scrape is required.
// ============================================================
var pythonPort = builder.Configuration.GetValue<int>(
    "PythonService:Port",
    8000
);

builder.Services.AddHttpClient<PythonParserService>(client =>
{
    client.BaseAddress = new Uri(
        $"http://127.0.0.1:{pythonPort}/"
    );

    // PDF scraping/parsing can take a long time.
    client.Timeout = TimeSpan.FromHours(6);
});


// ============================================================
// BUILD APPLICATION
// ============================================================
var app = builder.Build();


// ============================================================
// SWAGGER
// ============================================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


// ============================================================
// MIDDLEWARE
// ============================================================
app.UseHttpsRedirection();

app.UseCors("AllowReact");

app.UseAuthorization();

app.MapControllers();


// ============================================================
// RUN
// ============================================================
app.Run();