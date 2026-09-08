using GemBidScraper.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace GemBidScraper.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExtractController : ControllerBase
    {
        private readonly PythonParserService _pythonParserService;

        public ExtractController(PythonParserService pythonParserService)
        {
            _pythonParserService = pythonParserService;
        }

        /// <summary>
        /// Runs a scrape for the given ministry.
        ///
        /// `pages` defaults to -1, which tells the scraper to walk GeM's
        /// ENTIRE listing for that ministry rather than stopping after a
        /// fixed number of pages. Do this in normal use — it's what
        /// guarantees bids don't get missed just because they weren't on
        /// page 1, or are sitting near the end of the list. It's safe to
        /// leave as the default because listing pages are cheap to walk;
        /// the expensive PDF work only happens for bids that are new
        /// (see PythonParserService.ParseOnlineAsync).
        ///
        /// Pass a positive number only for a quick manual/test run where
        /// you deliberately want to cap how much gets scraped.
        /// </summary>
        [HttpPost("online")]
        public async Task<IActionResult> ParseOnline(
            [FromQuery] int pages = -1,
            [FromQuery] string ministry = "Ministry of Defence")
        {
            try
            {
                var bids = await _pythonParserService.ParseOnlineAsync(pages, ministry);

                return Ok(new
                {
                    Success = true,
                    Ministry = ministry,
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
    }
}