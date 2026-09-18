using GemBidScraper.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace GemBidScraper.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExtractController : ControllerBase
    {
        private readonly PythonParserService _pythonParserService;
        private readonly IConfiguration _config;

        public ExtractController(
            PythonParserService pythonParserService,
            IConfiguration config)
        {
            _pythonParserService = pythonParserService;
            _config = config;
        }

        /// <summary>
        /// Runs a scrape from either All Bids or a given ministry.
        ///
        /// `pages` defaults to -1, which tells the scraper to walk GeM's
        /// ENTIRE listing rather than stopping after a fixed number of pages.
        ///
        /// `allBids`: When true, scrapes from https://bidplus.gem.gov.in/all-bids.
        /// When null, defaults to the "PythonService:All-bids" setting in appsettings.json.
        /// </summary>
        [HttpPost("online")]
        public async Task<IActionResult> ParseOnline(
            [FromQuery] int pages = -1,
            [FromQuery] string? ministry = null,
            [FromQuery] bool? allBids = null)
        {
            try
            {
                bool isAllBids;
                if (allBids.HasValue)
                {
                    isAllBids = allBids.Value;
                }
                else
                {
                    var allBidsSetting = _config["PythonService:All-bids"]
                                         ?? _config["PythonService:AllBids"]
                                         ?? "off";
                    isAllBids = allBidsSetting.Equals("on", StringComparison.OrdinalIgnoreCase)
                                || allBidsSetting.Equals("true", StringComparison.OrdinalIgnoreCase);
                }

                string effectiveMinistry = ministry ?? (isAllBids ? "All Bids" : "Ministry of Defence");

                var bids = await _pythonParserService.ParseOnlineAsync(
                    pages,
                    isAllBids ? null : effectiveMinistry,
                    allBids: isAllBids);

                return Ok(new
                {
                    Success = true,
                    Mode = isAllBids ? "All Bids" : "Ministry",
                    Ministry = effectiveMinistry,
                    Pages = pages,
                    TotalRecords = bids.Count,
                    Data = bids
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Re-runs category classification for bids already stored in the database.
        /// Useful when keywords or classification rules are updated.
        /// Supports optional `take` parameter (e.g. ?take=500) or `bidNumber` (e.g. ?bidNumber=GEM/2026/B/8038742).
        /// </summary>
        [HttpPost("reclassify")]
        public async Task<IActionResult> Reclassify(
            [FromQuery] int? take = null,
            [FromQuery] string? bidNumber = null)
        {
            try
            {
                var result = await _pythonParserService.ReclassifyBidsAsync(take, bidNumber);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Error = ex.Message
                });
            }
        }
    }
}