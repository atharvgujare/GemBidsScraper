
using GemBidScraper.Data;
using GemBidScraper.GeMBIdMapper;
using GemBidScraper.Models;
using GemBidScraper.Services.CategoryClassification;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GemBidScraper.Services
{
    public class PythonParserService
    {
        private readonly HttpClient _httpClient;
        private readonly ApplicationDbContext _context;
        private readonly CategoryClassifier _categoryClassifier;

        // Batch size for inserts.
        private const int BatchSize = 500;

        // SQL Server parameter limit protection.
        private const int ExistenceCheckChunkSize = 1000;

        // Only send currently active/recent bids to Python as known bids.
        private const int KnownBidLookbackBufferDays = 2;

        public PythonParserService(
            HttpClient httpClient,
            ApplicationDbContext context,
            CategoryClassifier categoryClassifier)
        {
            _httpClient = httpClient;
            _context = context;
            _categoryClassifier = categoryClassifier;
        }

        // ---------------------------------------------------------
        // FIX TIME FORMAT
        // ---------------------------------------------------------

        private string FixTime(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return value;

            return Regex.Replace(
                value,
                @"(\d{1,2})\s+(\d{2})\s+(AM|PM)",
                "$1:$2 $3",
                RegexOptions.IgnoreCase);
        }

        // ---------------------------------------------------------
        // PARSE ONLINE
        // ---------------------------------------------------------

        public async Task<List<GeMBidExtract>> ParseOnlineAsync(
            int pages,
            string ministry = "Ministry of Defence")
        {

            var runStopwatch = System.Diagnostics.Stopwatch.StartNew();
            //--------------------------------------------------
            // Get known active bids
            //--------------------------------------------------

            var knownBidNumbers =
                await GetKnownActiveBidNumbersAsync();

            var requestPayload = new
            {
                Pages = pages,
                Ministry = ministry,
                KnownBidNumbers = knownBidNumbers
            };

            Console.WriteLine("==========================================");
            Console.WriteLine("[C#] Calling Python API");
            Console.WriteLine($"[C#] BaseAddress: {_httpClient.BaseAddress}");
            Console.WriteLine("[C#] Endpoint: extract-online-pages");
            Console.WriteLine($"[C#] Pages: {pages}");
            Console.WriteLine($"[C#] Ministry: {ministry}");
            Console.WriteLine($"[C#] Known bids: {knownBidNumbers.Count}");
            Console.WriteLine("==========================================");

            var response = await _httpClient.PostAsJsonAsync(
                "extract-online-pages",
                requestPayload);

            Console.WriteLine("==========================================");
            Console.WriteLine(
                $"[C#] Python response: {(int)response.StatusCode} {response.StatusCode}");
            Console.WriteLine("==========================================");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();

                throw new Exception(
                    $"Python API Error ({response.StatusCode}) : {error}");
            }

            //--------------------------------------------------
            // Deserialize Response
            //--------------------------------------------------

            var json =
                await response.Content.ReadAsStringAsync();

            Console.WriteLine(
                $"[Parser] Received {json.Length:N0} bytes from Python API.");

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var apiResponse =
                JsonSerializer.Deserialize<PythonApiResponse>(
                    json,
                    options);

            Console.WriteLine(
                $"[Parser] Result count: {apiResponse?.Result?.Count ?? 0}");

            Console.WriteLine("[Parser] JSON preview:");

            Console.WriteLine(
                json.Substring(
                    0,
                    Math.Min(json.Length, 2000)));

            List<GeMBidExtract> bids = new();

            if (apiResponse == null ||
                apiResponse.Result == null ||
                !apiResponse.Result.Any())
            {
                Console.WriteLine(
                    "No records received from Python scraper.");

                return bids;
            }

            //--------------------------------------------------
            // JSON -> MODEL MAPPING
            //--------------------------------------------------

            var mappedBids =
                new ConcurrentBag<GeMBidExtract>();

            var mappingErrors =
                new ConcurrentBag<(string PdfUrl, string Error)>();

            Parallel.ForEach(
                apiResponse.Result,
                new ParallelOptions
                {
                    MaxDegreeOfParallelism =
                        Environment.ProcessorCount
                },
                item =>
                {
                    if (item.Data.ValueKind != JsonValueKind.Object)
                        return;

                    if (!item.Data.EnumerateObject().Any())
                        return;

                    try
                    {
                        //--------------------------------------------------
                        // MAP PDF DATA
                        //--------------------------------------------------

                        var bid =
                            GeMBidMapper_2.Map<GeMBidExtract>(
                                item.PdfUrl,
                                item.Data);

                        //--------------------------------------------------
                        // BASIC CARD DATA
                        //--------------------------------------------------

                        bid.BidNumber =
                            item.BidNumber;

                        bid.CardItemName =
                            item.ItemName;

                        //--------------------------------------------------
                        // MINISTRY
                        //--------------------------------------------------

                        if (!string.IsNullOrWhiteSpace(item.Ministry))
                        {
                            bid.CardMinistry =
                                item.Ministry;
                        }
                        else if (
                            item.Data.TryGetProperty(
                                "Ministry/State Name",
                                out var minProp))
                        {
                            bid.CardMinistry =
                                minProp.GetString();
                        }

                        //--------------------------------------------------
                        // DEPARTMENT
                        //--------------------------------------------------

                        if (!string.IsNullOrWhiteSpace(item.Department))
                        {
                            bid.CardDepartment =
                                item.Department;
                        }
                        else if (
                            item.Data.TryGetProperty(
                                "Department Name",
                                out var deptProp))
                        {
                            bid.CardDepartment =
                                deptProp.GetString();
                        }

                        //--------------------------------------------------
                        // QUANTITY
                        //--------------------------------------------------

                        if (int.TryParse(
                            item.Quantity,
                            out var qty))
                        {
                            bid.CardQuantity =
                                qty;
                        }
                        else if (
                            item.Data.TryGetProperty(
                                "Total Quantity",
                                out var totalQty))
                        {
                            if (int.TryParse(
                                totalQty.GetString(),
                                out qty))
                            {
                                bid.CardQuantity =
                                    qty;
                            }
                        }

                        // ==================================================
                        // CARD START / END DATE
                        // ==================================================
                        //
                        // These values come from the GeM card/search
                        // response, NOT from the PDF.
                        //
                        // We intentionally use DateTime.TryParse instead
                        // of one strict TryParseExact format because the
                        // GeM response can return slightly different
                        // date/time representations.
                        // ==================================================

                        item.StartDate =
                            FixTime(item.StartDate);

                        item.EndDate =
                            FixTime(item.EndDate);

                        //--------------------------------------------------
                        // LOG RAW CARD DATES
                        //--------------------------------------------------

                        Console.WriteLine(
                            $"[CARD DATE] Bid: {item.BidNumber} | " +
                            $"Start: '{item.StartDate}' | " +
                            $"End: '{item.EndDate}'");

                        //--------------------------------------------------
                        // CARD START DATE
                        //--------------------------------------------------

                        if (DateTime.TryParse(
                            item.StartDate,
                            System.Globalization.CultureInfo.InvariantCulture,
                            System.Globalization.DateTimeStyles.AllowWhiteSpaces,
                            out var start))
                        {
                            bid.CardStartDate =
                                start;

                            Console.WriteLine(
                                $"[CARD START OK] {item.BidNumber} -> " +
                                $"{bid.CardStartDate:yyyy-MM-dd HH:mm:ss}");
                        }
                        else
                        {
                            Console.WriteLine(
                                $"[CARD START FAILED] Bid: {item.BidNumber} | " +
                                $"Value: '{item.StartDate}'");
                        }

                        //--------------------------------------------------
                        // CARD END DATE
                        //--------------------------------------------------

                        if (DateTime.TryParse(
                            item.EndDate,
                            System.Globalization.CultureInfo.InvariantCulture,
                            System.Globalization.DateTimeStyles.AllowWhiteSpaces,
                            out var end))
                        {
                            bid.CardEndDate =
                                end;

                            Console.WriteLine(
                                $"[CARD END OK] {item.BidNumber} -> " +
                                $"{bid.CardEndDate:yyyy-MM-dd HH:mm:ss}");
                        }
                        else
                        {
                            Console.WriteLine(
                                $"[CARD END FAILED] Bid: {item.BidNumber} | " +
                                $"Value: '{item.EndDate}'");
                        }

                        //--------------------------------------------------
                        // ADD MAPPED BID
                        //--------------------------------------------------

                        mappedBids.Add(bid);
                    }
                    catch (Exception ex)
                    {
                        mappingErrors.Add(
                            (
                                item.PdfUrl,
                                ex.Message
                            ));
                    }
                });

            //--------------------------------------------------
            // DISPLAY MAPPING ERRORS
            //--------------------------------------------------

            foreach (var (pdfUrl, error) in mappingErrors)
            {
                Console.WriteLine("------------------------------------");
                Console.WriteLine(
                    $"Mapping Failed : {pdfUrl}");

                Console.WriteLine(error);
            }

            bids =
                mappedBids.ToList();

            if (bids.Count == 0)
            {
                Console.WriteLine(
                    "No bids parsed successfully.");

                return bids;
            }

            //--------------------------------------------------
            // DEDUPLICATE AGAINST DATABASE
            //--------------------------------------------------

            var incomingBidNumbers =
                bids
                    .Where(b =>
                        !string.IsNullOrWhiteSpace(
                            b.BidNumber))
                    .Select(b =>
                        b.BidNumber!)
                    .Distinct()
                    .ToList();

            var existingBidNumbers =
                await GetExistingBidNumbersAsync(
                    incomingBidNumbers);

            List<GeMBidExtract> newRecords = new();

            foreach (var bid in bids)
            {
                if (string.IsNullOrWhiteSpace(
                    bid.BidNumber))
                    continue;

                if (existingBidNumbers.Contains(
                    bid.BidNumber))
                    continue;

                //--------------------------------------------------
                // CATEGORY CLASSIFICATION
                //--------------------------------------------------

                var category =
                    _categoryClassifier.Classify(
                        $"{bid.BOQTitle} " +
                        $"{bid.PrimaryProductCategory} " +
                        $"{bid.SimilarCategory}",

                        $"{bid.ItemCategory} " +
                        $"{bid.Specification}");

                bid.CategoryKey =
                    category.CategoryKey;

                bid.CategorySubKey =
                    category.CategorySubKey;

                //--------------------------------------------------
                // ADD NEW RECORD
                //--------------------------------------------------

                newRecords.Add(bid);

                existingBidNumbers.Add(
                    bid.BidNumber);
            }

            //--------------------------------------------------
            // NOTHING NEW
            //--------------------------------------------------

            if (newRecords.Count == 0)
            {
                Console.WriteLine(
                    "No new bids found.");

                return newRecords;
            }

            //--------------------------------------------------
            // SAVE
            //--------------------------------------------------

            await SaveInBatchesAsync(
    newRecords);

            runStopwatch.Stop();

            Console.WriteLine("------------------------------------");
            Console.WriteLine("[SCRAPE TIMING]");
            Console.WriteLine($"Total Run Time : {runStopwatch.Elapsed:hh\\:mm\\:ss\\.fff}");
            Console.WriteLine($"New Bids       : {newRecords.Count:N0}");
            Console.WriteLine("------------------------------------");

            return newRecords;
        }

        // ---------------------------------------------------------
        // GET KNOWN ACTIVE BID NUMBERS
        // ---------------------------------------------------------

        private async Task<List<string>>
            GetKnownActiveBidNumbersAsync()
        {
            var cutoff =
                DateTime.Now.AddDays(
                    -KnownBidLookbackBufferDays);

            return await _context.GeMBidExtracts
                .AsNoTracking()
                .Where(x =>
                    x.BidNumber != null &&
                    (
                        x.CardEndDate == null ||
                        x.CardEndDate >= cutoff
                    ))
                .Select(x =>
                    x.BidNumber!)
                .ToListAsync();
        }

        // ---------------------------------------------------------
        // GET EXISTING BID NUMBERS
        // ---------------------------------------------------------

        private async Task<HashSet<string>>
            GetExistingBidNumbersAsync(
                List<string> bidNumbers)
        {
            var existing =
                new HashSet<string>();

            if (bidNumbers.Count == 0)
                return existing;

            for (
                int i = 0;
                i < bidNumbers.Count;
                i += ExistenceCheckChunkSize)
            {
                var chunk =
                    bidNumbers
                        .Skip(i)
                        .Take(ExistenceCheckChunkSize)
                        .ToList();

                var found =
                    await _context.GeMBidExtracts
                        .AsNoTracking()
                        .Where(x =>
                            chunk.Contains(
                                x.BidNumber!))
                        .Select(x =>
                            x.BidNumber!)
                        .ToListAsync();

                foreach (var f in found)
                    existing.Add(f);
            }

            return existing;
        }

        // ---------------------------------------------------------
        // SAVE IN BATCHES
        // ---------------------------------------------------------

        private async Task SaveInBatchesAsync(
            List<GeMBidExtract> newRecords)
        {
            var strategy =
                _context.Database
                    .CreateExecutionStrategy();

            var failedBids =
                new List<(string BidNumber, string Error)>();

            int totalSaved = 0;

            for (
                int i = 0;
                i < newRecords.Count;
                i += BatchSize)
            {
                var batch =
                    newRecords
                        .Skip(i)
                        .Take(BatchSize)
                        .ToList();

                bool batchSucceeded =
                    await strategy.ExecuteAsync(
                        async () =>
                        {
                            using var transaction =
                                await _context.Database
                                    .BeginTransactionAsync();

                            try
                            {
                                _context.GeMBidExtracts
                                    .AddRange(batch);

                                await _context
                                    .SaveChangesAsync();

                                await transaction
                                    .CommitAsync();

                                return true;
                            }
                            catch (Exception ex)
                            {
                                await transaction
                                    .RollbackAsync();

                                _context
                                    .ChangeTracker
                                    .Clear();

                                Console.WriteLine(
                                    "------------------------------------");

                                Console.WriteLine(
                                    $"[Batch {i}-{i + batch.Count}] failed: " +
                                    $"{ex.InnerException?.Message ?? ex.Message}");

                                Console.WriteLine(
                                    "Retrying this batch row-by-row " +
                                    "to isolate the bad record(s)...");

                                return false;
                            }
                        });

                //--------------------------------------------------
                // BATCH SUCCESS
                //--------------------------------------------------

                if (batchSucceeded)
                {
                    totalSaved +=
                        batch.Count;

                    Console.WriteLine(
                        $"Imported {totalSaved:N0} / " +
                        $"{newRecords.Count:N0} bids...");

                    continue;
                }

                //--------------------------------------------------
                // ROW-BY-ROW FALLBACK
                //--------------------------------------------------

                foreach (var bid in batch)
                {
                    try
                    {
                        await strategy.ExecuteAsync(
                            async () =>
                            {
                                _context.GeMBidExtracts
                                    .Add(bid);

                                await _context
                                    .SaveChangesAsync();
                            });

                        totalSaved++;
                    }
                    catch (Exception rowEx)
                    {
                        var msg =
                            rowEx.InnerException?.Message ??
                            rowEx.Message;

                        failedBids.Add(
                            (
                                bid.BidNumber ??
                                "(no number)",

                                msg
                            ));

                        Console.WriteLine(
                            $"  Skipped bad record " +
                            $"{bid.BidNumber}: {msg}");
                    }
                    finally
                    {
                        _context
                            .ChangeTracker
                            .Clear();
                    }
                }

                Console.WriteLine(
                    $"Imported {totalSaved:N0} / " +
                    $"{newRecords.Count:N0} bids " +
                    "(after row-level retry)...");
            }

            //--------------------------------------------------
            // IMPORT SUMMARY
            //--------------------------------------------------

            Console.WriteLine(
                "------------------------------------");

            Console.WriteLine(
                "Import Completed.");

            Console.WriteLine(
                $"Total Saved  : {totalSaved:N0} / " +
                $"{newRecords.Count:N0}");

            Console.WriteLine(
                $"Total Failed : {failedBids.Count:N0}");

            Console.WriteLine(
                "------------------------------------");

            if (failedBids.Count > 0)
            {
                Console.WriteLine(
                    $"Failed bid(s) — " +
                    $"{failedBids.Count:N0} total " +
                    "(showing all):");

                foreach (
                    var (num, err)
                    in failedBids)
                {
                    Console.WriteLine(
                        $"  {num}: {err}");
                }
            }
        }
    }
}
