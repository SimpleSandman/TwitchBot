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
    public class ModeratorsController : Controller
    {
        private readonly TwitchBotContext _context;

        public ModeratorsController(TwitchBotContext context)
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

        // POST: api/moderators
        [HttpPost]
        public async Task<IActionResult> PostModerators([FromBody] Moderators moderators)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Moderators.Add(moderators);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/moderators/5?username=transformersblackops
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteModerators([FromRoute] int broadcasterId, [FromQuery] string username)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Moderators moderators = await _context.Moderators.SingleOrDefaultAsync(m => m.Broadcaster == broadcasterId && m.Username == username);
            if (moderators == null)
            {
                return NotFound();
            }

            _context.Moderators.Remove(moderators);
            await _context.SaveChangesAsync();

            return Ok(moderators);
        }
    }
}