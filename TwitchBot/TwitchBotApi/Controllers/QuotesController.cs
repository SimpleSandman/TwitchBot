using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TwitchBotDb.Models;

namespace TwitchBotApi.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]/[action]")]
    public class QuotesController : Controller
    {
        private readonly SimpleBotContext _context;

        public QuotesController(SimpleBotContext context)
        {
            _context = context;
        }

        // GET: api/quotes/get/5
        [HttpGet("{broadcasterId:int}")]
        public async Task<IActionResult> Get([FromRoute] int broadcasterId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            List<Quote> quote = await _context.Quote.Where(m => m.BroadcasterId == broadcasterId).ToListAsync();

            if (quote == null || quote.Count == 0)
            {
                return NotFound();
            }

            return Ok(quote);
        }

        // PATCH: api/quotes/patch/5?broadcasterId=2
        // Body (JSON): [{ "op": "replace", "path": "/userquote", "value": "repalce old quote with this" }]
        [HttpPatch("{id:int}")]
        public async Task<IActionResult> Patch([FromRoute] int id, [FromQuery] int broadcasterId, [FromBody]JsonPatchDocument<Quote> quotePatch)
        {
            Quote quote = _context.Quote.SingleOrDefault(m => m.Id == id && m.BroadcasterId == broadcasterId);

            if (quote == null)
            {
                return BadRequest();
            }

            quotePatch.ApplyTo(quote, (Microsoft.AspNetCore.JsonPatch.Adapters.IObjectAdapter)ModelState);

            if (!ModelState.IsValid)
            {
                return new BadRequestObjectResult(ModelState);
            }

            _context.Quote.Update(quote);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!QuoteExists(id))
                {
                    return NotFound();
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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Quote.Add(quote);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/quotes/delete/5?broadcasterId=2
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id, [FromQuery] int broadcasterId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Quote quote = await _context.Quote.SingleOrDefaultAsync(m => m.Id == id && m.BroadcasterId == broadcasterId);
            if (quote == null)
            {
                return NotFound();
            }

            _context.Quote.Remove(quote);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool QuoteExists(int id)
        {
            return _context.Quote.Any(e => e.Id == id);
        }
    }
}