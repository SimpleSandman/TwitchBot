using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Adapters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TwitchBot.Api.Helpers;
using TwitchBot.Api.Helpers.ErrorExceptions;
using TwitchBotDb.Context;
using TwitchBotDb.Models;

namespace TwitchBot.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class QuotesController : ExtendedControllerBase
    {
        private readonly SimpleBotContext _context;

        public QuotesController(SimpleBotContext context)
        {
            _context = context;
        }

        // GET: api/quotes/get/5
        [HttpGet("{broadcasterId}")]
        public async Task<IActionResult> Get(int broadcasterId)
        {
            IsModelStateValid();

            List<Quote> quote = await _context.Quotes.Where(m => m.BroadcasterId == broadcasterId).ToListAsync();

            if (quote == null || quote.Count == 0)
            {
                throw new NotFoundException("Quote not found");
            }

            return Ok(quote);
        }

        // PATCH: api/quotes/patch/5?broadcasterId=2
        // Body (JSON): [{ "op": "replace", "path": "/userquote", "value": "repalce old quote with this" }]
        [HttpPatch("{id}")]
        public async Task<IActionResult> Patch(int id, [FromQuery] int broadcasterId, [FromBody] JsonPatchDocument<Quote> quotePatch)
        {
            IsModelStateValid();

            Quote? quote = await _context.Quotes.SingleOrDefaultAsync(m => m.Id == id && m.BroadcasterId == broadcasterId);

            if (quote == null)
            {
                throw new NotFoundException("Quote not found");
            }

            quotePatch.ApplyTo(quote, (IObjectAdapter)ModelState);

            if (!ModelState.IsValid)
            {
                return new BadRequestObjectResult(ModelState);
            }

            _context.Quotes.Update(quote);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!QuoteExists(id))
                {
                    throw new NotFoundException("Quote not found");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/quotes/create
        // Body (JSON): { "userquote": "insert new broadcaster quote here", "username": "quoter", "broadcaster": "2" }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Quote quote)
        {
            IsModelStateValid();

            _context.Quotes.Add(quote);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/quotes/delete/5?broadcasterId=2
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, [FromQuery] int broadcasterId)
        {
            IsModelStateValid();

            Quote? quote = await _context.Quotes.SingleOrDefaultAsync(m => m.Id == id && m.BroadcasterId == broadcasterId);
            if (quote == null)
            {
                throw new NotFoundException("Quote not found");
            }

            _context.Quotes.Remove(quote);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool QuoteExists(int id)
        {
            return _context.Quotes.Any(e => e.Id == id);
        }
    }
}
