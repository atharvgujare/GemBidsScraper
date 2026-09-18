using System.Diagnostics;
using GemBidScraper.Data;
using GemBidScraper.Services;
using Microsoft.EntityFrameworkCore;
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

        // =========================================================
        // SCHEDULER
        // =========================================================

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
                    await Task.Delay(
                        delay,
                        stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }

                await RunScrapeAsync(stoppingToken);
            }
        }

        // =========================================================
        // RUN SCRAPE
        // =========================================================

        private async Task RunScrapeAsync(
            CancellationToken ct)
        {
            try
            {
                if (!await IsDatabaseReadyAsync(ct))
                {
                    _logger.LogWarning(
                        "Database is not reachable. Scrape run skipped before starting Python.");

                    return;
                }

                await StartPythonServiceAsync(ct);

                await WaitForPythonReadyAsync(ct);

                using var scope =
                    _services.CreateScope();

                var parser =
                    scope.ServiceProvider
                        .GetRequiredService<PythonParserService>();

                var pages = int.Parse(
     _config["PythonService:Pages"] ?? "5");

                var allBidsSetting = _config["PythonService:All-bids"]
                                     ?? _config["PythonService:AllBids"]
                                     ?? "off";

                bool isAllBids = allBidsSetting.Equals("on", StringComparison.OrdinalIgnoreCase)
                                 || allBidsSetting.Equals("true", StringComparison.OrdinalIgnoreCase);

                if (isAllBids)
                {
                    _logger.LogInformation(
                        "All-bids setting is ON. Starting scrape directly from All Bids listing page...");

                    var processedBids =
                        await parser.ParseOnlineAsync(
                            pages,
                            ministry: null,
                            allBids: true);

                    _logger.LogInformation(
                        "All-bids scrape completed. Processed {Count} bids.",
                        processedBids.Count);
                }
                else
                {
                    var ministries =
                        _config
                            .GetSection("PythonService:Ministry")
                            .Get<string[]>();

                    if (ministries == null || ministries.Length == 0)
                    {
                        _logger.LogWarning(
                            "All-bids is OFF and no ministries configured. Scrape run skipped.");

                        return;
                    }

                    foreach (var ministry in ministries)
                    {
                        _logger.LogInformation(
                            "Starting scrape for ministry: {Ministry}",
                            ministry);

                        var processedBids =
                            await parser.ParseOnlineAsync(
                                pages,
                                ministry,
                                allBids: false);

                        _logger.LogInformation(
                            "Scrape completed for {Ministry}. Processed {Count} bids.",
                            ministry,
                            processedBids.Count);
                    }

                    _logger.LogInformation(
                        "Scheduled scrape run completed for all configured ministries.");
                }
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

        // =========================================================
        // DATABASE PREFLIGHT
        // =========================================================

        private async Task<bool> IsDatabaseReadyAsync(
            CancellationToken ct)
        {
            using var scope =
                _services.CreateScope();

            var db =
                scope.ServiceProvider
                    .GetRequiredService<ApplicationDbContext>();

            var strategy =
                db.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(
                async () =>
                {
                    return await db.Database.CanConnectAsync(ct);
                });
        }

        // =========================================================
        // START PYTHON SERVICE
        // =========================================================

        private async Task StartPythonServiceAsync(
            CancellationToken ct)
        {
            if (_pythonProcess is { HasExited: false })
            {
                _logger.LogInformation(
                    "Python service is already running. PID: {Pid}",
                    _pythonProcess.Id);

                return;
            }

            // -----------------------------------------------------
            // GET PYTHON PORT
            // -----------------------------------------------------

            var pythonPort =
                _config.GetValue<int>(
                    "PythonService:Port",
                    8000);

            _logger.LogInformation(
                "Python API Port: {PythonPort}",
                pythonPort);

            // -----------------------------------------------------
            // FIND PYTHON PROJECT
            // -----------------------------------------------------

            var applicationPath =
                AppContext.BaseDirectory;

            var pythonProjectPath =
                Path.Combine(
                    applicationPath,
                    "PdfParserApi");

            if (!Directory.Exists(pythonProjectPath))
            {
                throw new DirectoryNotFoundException(
                    $"Python project directory not found: {pythonProjectPath}");
            }

            var requirementsPath =
                Path.Combine(
                    pythonProjectPath,
                    "requirements.txt");

            if (!File.Exists(requirementsPath))
            {
                throw new FileNotFoundException(
                    $"Python requirements.txt not found: {requirementsPath}");
            }

            _logger.LogInformation(
                "Python Project Path: {PythonProjectPath}",
                pythonProjectPath);

            // -----------------------------------------------------
            // ENSURE PYTHON ENVIRONMENT
            // -----------------------------------------------------

            var pythonExePath =
                await EnsurePythonEnvironmentAsync(
                    pythonProjectPath,
                    requirementsPath,
                    ct);

            // -----------------------------------------------------
            // START UVICORN
            // -----------------------------------------------------

            _logger.LogInformation(
                "Python Executable Path: {PythonExePath}",
                pythonExePath);

            _pythonProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = pythonExePath,

                    Arguments =
                        $"-m uvicorn app:app --host 127.0.0.1 --port {pythonPort}",

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

        // =========================================================
        // ENSURE PYTHON ENVIRONMENT
        // =========================================================

        private async Task<string> EnsurePythonEnvironmentAsync(
            string pythonProjectPath,
            string requirementsPath,
            CancellationToken ct)
        {
            // -----------------------------------------------------
            // GET CONFIGURED PYTHON ENVIRONMENT LOCATION
            // -----------------------------------------------------

            var configuredEnvironmentPath =
                _config["PythonService:PythonEnvironmentPath"];

            var venvPath =
                string.IsNullOrWhiteSpace(
                    configuredEnvironmentPath)
                    ? Path.Combine(
                        AppContext.BaseDirectory,
                        "PythonEnv")
                    : configuredEnvironmentPath;

            // Convert relative path to absolute path
            if (!Path.IsPathFullyQualified(venvPath))
            {
                venvPath =
                    Path.GetFullPath(
                        Path.Combine(
                            AppContext.BaseDirectory,
                            venvPath));
            }

            var venvPythonPath =
                Path.Combine(
                    venvPath,
                    "Scripts",
                    "python.exe");

            var setupMarkerPath =
                Path.Combine(
                    venvPath,
                    ".gem_setup_complete");

            _logger.LogInformation(
                "Python Environment Path: {VenvPath}",
                venvPath);

            // -----------------------------------------------------
            // CHECK COMPLETE SETUP
            // -----------------------------------------------------

            if (File.Exists(venvPythonPath) &&
                File.Exists(setupMarkerPath))
            {
                _logger.LogInformation(
                    "Python environment is already configured.");

                _logger.LogInformation(
                    "Using existing Python environment: {VenvPythonPath}",
                    venvPythonPath);

                return venvPythonPath;
            }

            // -----------------------------------------------------
            // FIND SYSTEM PYTHON
            // -----------------------------------------------------

            _logger.LogInformation(
                "Python environment is not fully configured.");

            _logger.LogInformation(
                "Finding system Python...");

            var systemPythonPath =
                FindPython();

            _logger.LogInformation(
                "System Python found at: {PythonPath}",
                systemPythonPath);

            // -----------------------------------------------------
            // CREATE VENV
            // -----------------------------------------------------

            if (!File.Exists(venvPythonPath))
            {
                _logger.LogInformation(
                    "Creating Python virtual environment at: {VenvPath}",
                    venvPath);

                Directory.CreateDirectory(
                    Path.GetDirectoryName(venvPath)!);

                await RunProcessAsync(
                    systemPythonPath,
                    $"-m venv \"{venvPath}\"",
                    pythonProjectPath,
                    ct);

                if (!File.Exists(venvPythonPath))
                {
                    throw new FileNotFoundException(
                        $"Virtual environment Python executable was not created: {venvPythonPath}");
                }

                _logger.LogInformation(
                    "Python virtual environment created successfully.");
            }
            else
            {
                _logger.LogInformation(
                    "Python environment exists but setup is incomplete.");
            }

            // -----------------------------------------------------
            // UPGRADE PIP
            // -----------------------------------------------------

            _logger.LogInformation(
                "Upgrading pip...");

            await RunProcessAsync(
                venvPythonPath,
                "-m pip install --upgrade pip",
                pythonProjectPath,
                ct);

            // -----------------------------------------------------
            // INSTALL REQUIREMENTS
            // -----------------------------------------------------

            _logger.LogInformation(
                "Installing Python dependencies from requirements.txt...");

            await RunProcessAsync(
                venvPythonPath,
                $"-m pip install -r \"{requirementsPath}\"",
                pythonProjectPath,
                ct);

            _logger.LogInformation(
                "Python dependencies installed successfully.");

            // -----------------------------------------------------
            // INSTALL PLAYWRIGHT
            // -----------------------------------------------------

            _logger.LogInformation(
                "Installing Playwright browsers...");

            await RunProcessAsync(
                venvPythonPath,
                "-m playwright install",
                pythonProjectPath,
                ct);

            _logger.LogInformation(
                "Playwright browsers installed successfully.");

            // -----------------------------------------------------
            // MARK SETUP COMPLETE
            // -----------------------------------------------------

            await File.WriteAllTextAsync(
                setupMarkerPath,
                DateTime.Now.ToString("O"),
                ct);

            _logger.LogInformation(
                "Python environment setup completed successfully.");

            return venvPythonPath;
        }

        // =========================================================
        // FIND SYSTEM PYTHON
        // =========================================================

        private string FindPython()
        {
            // -----------------------------------------------------
            // FIRST: USE WINDOWS WHERE.EXE
            // -----------------------------------------------------

            var pythonFromWhere =
                FindPythonUsingWhere();

            if (pythonFromWhere != null)
            {
                return pythonFromWhere;
            }

            // -----------------------------------------------------
            // FALLBACK: COMMON PYTHON LOCATIONS
            // -----------------------------------------------------

            var possiblePaths = new[]
            {
                @"C:\Program Files\Python312\python.exe",
                @"C:\Program Files\Python311\python.exe",
                @"C:\Program Files\Python313\python.exe",

                @"C:\Python312\python.exe",
                @"C:\Python311\python.exe",
                @"C:\Python313\python.exe"
            };

            foreach (var path in possiblePaths)
            {
                if (File.Exists(path) &&
                    IsPython312OrNewer(path))
                {
                    _logger.LogInformation(
                        "Found Python installation at: {PythonPath}",
                        path);

                    return path;
                }
            }

            throw new FileNotFoundException(
                "Python could not be found on this server. " +
                "Please install Python 3.12 (64-bit).");
        }

        // =========================================================
        // FIND PYTHON USING WINDOWS WHERE.EXE
        // =========================================================

        private string? FindPythonUsingWhere()
        {
            try
            {
                var startInfo =
                    new ProcessStartInfo
                    {
                        FileName = "where.exe",

                        Arguments = "python",

                        UseShellExecute = false,

                        CreateNoWindow = true,

                        RedirectStandardOutput = true,

                        RedirectStandardError = true
                    };

                using var process =
                    Process.Start(startInfo);

                if (process == null)
                {
                    return null;
                }

                var output =
                    process.StandardOutput.ReadToEnd();

                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    return null;
                }

                var paths =
                    output
                        .Split(
                            new[] { '\r', '\n' },
                            StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim())
                        .Where(File.Exists)
                        .ToList();

                // Prefer Python 3.12 or newer
                foreach (var path in paths)
                {
                    if (IsPython312OrNewer(path))
                    {
                        _logger.LogInformation(
                            "Found suitable Python using where.exe: {PythonPath}",
                            path);

                        return path;
                    }
                }

                // If Python exists but version check failed,
                // use the first valid Python executable.
                if (paths.Count > 0)
                {
                    _logger.LogInformation(
                        "Found Python using where.exe: {PythonPath}",
                        paths[0]);

                    return paths[0];
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(
                    ex,
                    "Unable to locate Python using where.exe.");
            }

            return null;
        }

        // =========================================================
        // CHECK PYTHON VERSION
        // =========================================================

        private bool IsPython312OrNewer(
            string pythonPath)
        {
            try
            {
                var startInfo =
                    new ProcessStartInfo
                    {
                        FileName = pythonPath,

                        Arguments = "--version",

                        UseShellExecute = false,

                        CreateNoWindow = true,

                        RedirectStandardOutput = true,

                        RedirectStandardError = true
                    };

                using var process =
                    Process.Start(startInfo);

                if (process == null)
                {
                    return false;
                }

                var output =
                    process.StandardOutput.ReadToEnd();

                var error =
                    process.StandardError.ReadToEnd();

                process.WaitForExit();

                var versionText =
                    string.IsNullOrWhiteSpace(output)
                        ? error
                        : output;

                var version =
                    versionText
                        .Replace("Python", "")
                        .Trim();

                if (Version.TryParse(
                    version,
                    out var parsedVersion))
                {
                    return parsedVersion >=
                           new Version(3, 12);
                }
            }
            catch
            {
                // Continue searching.
            }

            return false;
        }

        // =========================================================
        // RUN PYTHON SETUP COMMAND
        // =========================================================

        private async Task RunProcessAsync(
            string fileName,
            string arguments,
            string workingDirectory,
            CancellationToken ct)
        {
            _logger.LogInformation(
                "Running: {FileName} {Arguments}",
                fileName,
                arguments);

            var startInfo =
                new ProcessStartInfo
                {
                    FileName = fileName,

                    Arguments = arguments,

                    WorkingDirectory =
                        workingDirectory,

                    UseShellExecute = false,

                    CreateNoWindow = true,

                    RedirectStandardOutput = true,

                    RedirectStandardError = true
                };

            using var process =
                new Process
                {
                    StartInfo = startInfo
                };

            var errorBuilder =
                new System.Text.StringBuilder();

            process.OutputDataReceived +=
                (sender, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(e.Data))
                    {
                        _logger.LogInformation(
                            "[Python Setup] {Message}",
                            e.Data);
                    }
                };

            process.ErrorDataReceived +=
                (sender, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(e.Data))
                    {
                        errorBuilder.AppendLine(e.Data);

                        _logger.LogWarning(
                            "[Python Setup] {Message}",
                            e.Data);
                    }
                };

            if (!process.Start())
            {
                throw new InvalidOperationException(
                    $"Failed to start process: {fileName}");
            }

            process.BeginOutputReadLine();

            process.BeginErrorReadLine();

            await process.WaitForExitAsync(ct);

            if (process.ExitCode != 0)
            {
                var error =
                    errorBuilder.ToString().Trim();

                throw new InvalidOperationException(
                    $"Process failed with exit code {process.ExitCode}: " +
                    $"{fileName} {arguments}" +
                    (string.IsNullOrWhiteSpace(error)
                        ? string.Empty
                        : Environment.NewLine + error));
            }

            _logger.LogInformation(
                "Process completed successfully: {FileName}",
                fileName);
        }

        // =========================================================
        // WAIT FOR PYTHON API
        // =========================================================

        private async Task WaitForPythonReadyAsync(
            CancellationToken ct)
        {
            var pythonPort =
                _config.GetValue<int>(
                    "PythonService:Port",
                    8000);

            using var http =
                new HttpClient
                {
                    Timeout =
                        TimeSpan.FromSeconds(5)
                };

            for (int i = 0; i < 30; i++)
            {
                try
                {
                    var response =
                        await http.GetAsync(
                            $"http://127.0.0.1:{pythonPort}/docs",
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

                await Task.Delay(
                    TimeSpan.FromSeconds(1),
                    ct);
            }

            throw new TimeoutException(
                $"Python service on port {pythonPort} did not become ready in time.");
        }

        // =========================================================
        // STOP PYTHON SERVICE
        // =========================================================

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

                    // Wait maximum 10 seconds.
                    // Do not allow Python shutdown to block
                    // the scraper scheduler indefinitely.
                    if (_pythonProcess.WaitForExit(10000))
                    {
                        _logger.LogInformation(
                            "Python process stopped.");
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Python process did not exit within 10 seconds after Kill(). Continuing scheduler.");
                    }
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

        // =========================================================
        // WINDOWS SERVICE STOP
        // =========================================================

        public override async Task StopAsync(
            CancellationToken cancellationToken)
        {
            StopPythonService();

            await base.StopAsync(
                cancellationToken);
        }
    }
}
