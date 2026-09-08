
using System.Diagnostics;
using GemBidScraper.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GemBidScraper.Background
{
    public class ScraperBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly IConfiguration _config;
        private readonly ILogger<ScraperBackgroundService> _logger;

        private Process? _pythonProcess;

        public ScraperBackgroundService(
            IServiceProvider services,
            IConfiguration config,
            ILogger<ScraperBackgroundService> logger)
        {
            _services = services;
            _config = config;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(
    CancellationToken stoppingToken)
        {
            var runAtTimes =
                _config
                    .GetSection("PythonService:RunAt")
                    .Get<string[]>()?
                    .Select(TimeSpan.Parse)
                    .OrderBy(x => x)
                    .ToList()
                ?? new List<TimeSpan>
                {
            new TimeSpan(9, 0, 0)
                };

            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;

                var nextRun = runAtTimes
                    .Select(time =>
                    {
                        var candidate = DateTime.Today.Add(time);

                        if (candidate <= now)
                        {
                            candidate = candidate.AddDays(1);
                        }

                        return candidate;
                    })
                    .OrderBy(x => x)
                    .First();

                var delay = nextRun - now;

                _logger.LogInformation(
                    "Next scrape run scheduled for {NextRun}",
                    nextRun);

                try
                {
                    await Task.Delay(delay, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }

                await RunScrapeAsync(stoppingToken);
            }
        }

        private async Task RunScrapeAsync(
            CancellationToken ct)
        {
            try
            {
                StartPythonService();

                await WaitForPythonReadyAsync(ct);

                using var scope =
                    _services.CreateScope();

                var parser =
                    scope.ServiceProvider
                        .GetRequiredService<PythonParserService>();

                var pages = int.Parse(
                    _config["PythonService:Pages"] ?? "5");

                var ministry =
                    _config["PythonService:Ministry"]
                    ?? "Ministry of Defence";

                var newBids =
                    await parser.ParseOnlineAsync(
                        pages,
                        ministry);

                _logger.LogInformation(
                    "Scheduled scrape run completed: {Count} new bids",
                    newBids.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Scheduled scrape run failed");
            }
            finally
            {
                StopPythonService();
            }
        }

        private void StartPythonService()
        {
            if (_pythonProcess is { HasExited: false })
            {
                _logger.LogInformation(
                    "Python service is already running. PID: {Pid}",
                    _pythonProcess.Id);

                return;
            }

            // ---------------------------------------------------------
            // FIND PdfParserApi RELATIVE TO THE DEPLOYED APPLICATION
            // ---------------------------------------------------------

            var applicationPath =
                AppContext.BaseDirectory;

            var pythonProjectPath =
                Path.Combine(
                    applicationPath,
                    "PdfParserApi");

            var pythonExePath =
                Path.Combine(
                    pythonProjectPath,
                    "venv",
                    "Scripts",
                    "python.exe");

            // ---------------------------------------------------------
            // VALIDATE PATHS
            // ---------------------------------------------------------

            if (!Directory.Exists(pythonProjectPath))
            {
                throw new DirectoryNotFoundException(
                    $"Python project directory not found: {pythonProjectPath}");
            }

            if (!File.Exists(pythonExePath))
            {
                throw new FileNotFoundException(
                    $"Python executable not found: {pythonExePath}");
            }

            _logger.LogInformation(
                "Python Project Path: {PythonProjectPath}",
                pythonProjectPath);

            _logger.LogInformation(
                "Python Executable Path: {PythonExePath}",
                pythonExePath);

            // ---------------------------------------------------------
            // START PYTHON / UVICORN
            // ---------------------------------------------------------

            _pythonProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = pythonExePath,

                    Arguments =
                        "-m uvicorn app:app --host 127.0.0.1 --port 8000",

                    WorkingDirectory =
                        pythonProjectPath,

                    UseShellExecute = false,

                    CreateNoWindow = true,

                    RedirectStandardOutput = true,

                    RedirectStandardError = true
                }
            };

            _pythonProcess.OutputDataReceived +=
                (sender, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(e.Data))
                    {
                        _logger.LogInformation(
                            "[Python] {Message}",
                            e.Data);
                    }
                };

            _pythonProcess.ErrorDataReceived +=
                (sender, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(e.Data))
                    {
                        _logger.LogError(
                            "[Python] {Message}",
                            e.Data);
                    }
                };

            _pythonProcess.Start();

            _pythonProcess.BeginOutputReadLine();

            _pythonProcess.BeginErrorReadLine();

            _logger.LogInformation(
                "Started Python process. PID: {Pid}",
                _pythonProcess.Id);
        }

        private async Task WaitForPythonReadyAsync(
            CancellationToken ct)
        {
            using var http = new HttpClient();

            for (int i = 0; i < 30; i++)
            {
                try
                {
                    var response =
                        await http.GetAsync(
                            "http://127.0.0.1:8000/docs",
                            ct);

                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation(
                            "Python service is ready.");

                        return;
                    }
                }
                catch
                {
                    // Python service is not ready yet.
                }

                await Task.Delay(1000, ct);
            }

            throw new TimeoutException(
                "Python service did not become ready in time.");
        }

        private void StopPythonService()
        {
            try
            {
                if (_pythonProcess is { HasExited: false })
                {
                    _logger.LogInformation(
                        "Stopping Python process. PID: {Pid}",
                        _pythonProcess.Id);

                    _pythonProcess.Kill(
                        entireProcessTree: true);

                    _pythonProcess.WaitForExit();

                    _logger.LogInformation(
                        "Python process stopped.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while stopping Python process.");
            }
            finally
            {
                _pythonProcess?.Dispose();
                _pythonProcess = null;
            }
        }

        public override async Task StopAsync(
            CancellationToken cancellationToken)
        {
            StopPythonService();

            await base.StopAsync(
                cancellationToken);
        }
    }
}