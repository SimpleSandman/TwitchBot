using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TwitchBotDb.Models;

namespace TwitchBotApi.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]/[action]")]
    public class ModeratorsController : Controller
    {
        private readonly TwitchBotDbContext _context;

        public ModeratorsController(TwitchBotDbContext context)
        {
            _context = context;
        }

        // GET: api/moderators/get/2
        [HttpGet("{broadcasterId:int}")]
        public async Task<IActionResult> Get([FromRoute] int broadcasterId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            List<Moderators> moderators = await _context.Moderators.Where(m => m.Broadcaster == broadcasterId).ToListAsync();

            if (moderators == null || moderators.Count() == 0)
            {
                return NotFound();
            }

            return Ok(moderators);
        }

        // POST: api/moderators/create
        // Body (JSON): { "username": "simple_sandman", "broadcaster": 2 }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Moderators moderator)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (ModeratorExists(moderator.Username, moderator.Broadcaster))
            {
                return BadRequest();
            }

            _context.Moderators.Add(moderator);
            await _context.SaveChangesAsync();

            return CreatedAtAction("Get", new { broadcasterId = moderator.Broadcaster }, moderator);
        }

        // DELETE: api/moderators/delete/2?username=simple_sandman
        [HttpDelete("{broadcasterId:int}")]
        public async Task<IActionResult> Delete([FromRoute] int broadcasterId, [FromQuery] string username)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Moderators moderator = await _context.Moderators.SingleOrDefaultAsync(m => m.Broadcaster == broadcasterId && m.Username == username);
            if (moderator == null)
            {
                return NotFound();
            }

            _context.Moderators.Remove(moderator);
            await _context.SaveChangesAsync();

            return Ok(moderator);
        }

        private bool ModeratorExists(string username, int broadcasterId)
        {
            return _context.Moderators.Any(e => e.Username == username && e.Broadcaster == broadcasterId);
        }
    }
}