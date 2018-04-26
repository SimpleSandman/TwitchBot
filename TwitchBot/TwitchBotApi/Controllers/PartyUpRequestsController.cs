using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TwitchBotApi.Models;

namespace TwitchBotApi.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]/[action]")]
    public class PartyUpRequestsController : Controller
    {
        private readonly TwitchBotContext _context;

        public PartyUpRequestsController(TwitchBotContext context)
        {
            _context = context;
        }

        // GET: api/PartyUpRequests/5
        [HttpGet("{broadcasterId}")]
        public async Task<IActionResult> GetPartyUpRequests([FromRoute] int broadcasterId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var partyUpRequests = await _context.PartyUpRequests.Where(m => m.Broadcaster == broadcasterId).ToListAsync();

            if (partyUpRequests == null)
            {
                return NotFound();
            }

            return Ok(partyUpRequests);
        }

        // POST: api/PartyUpRequests
        [HttpPost]
        public async Task<IActionResult> PostPartyUpRequests([FromBody] PartyUpRequests partyUpRequests)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.PartyUpRequests.Add(partyUpRequests);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPartyUpRequests", new { id = partyUpRequests.Id }, partyUpRequests);
        }

        // DELETE: api/PartyUpRequests/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePartyUpRequests([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var partyUpRequests = await _context.PartyUpRequests.SingleOrDefaultAsync(m => m.Id == id);
            if (partyUpRequests == null)
            {
                return NotFound();
            }

            _context.PartyUpRequests.Remove(partyUpRequests);
            await _context.SaveChangesAsync();

            return Ok(partyUpRequests);
        }
    }
}