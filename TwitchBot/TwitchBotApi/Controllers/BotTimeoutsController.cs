using System;
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
    public class BotTimeoutsController : ControllerBase
    {
        private readonly SimpleBotContext _context;

        public BotTimeoutsController(SimpleBotContext context)
        {
            _context = context;
        }

        // GET: api/bottimeouts/get/2
        // GET: api/bottimeouts/get/2?username=simple_sandman
        [HttpGet("{broadcasterId:int}")]
        public async Task<IActionResult> Get([FromRoute] int broadcasterId, [FromQuery] string username = "")
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var botTimeouts = new object();

            if (string.IsNullOrEmpty(username))
                botTimeouts = await _context.BotTimeout.Where(m => m.BroadcasterId == broadcasterId).ToListAsync();
            else
                botTimeouts = await _context.BotTimeout.SingleOrDefaultAsync(m => m.BroadcasterId == broadcasterId && m.Username == username);

            if (botTimeouts == null)
            {
                return NotFound();
            }

            return Ok(botTimeouts);
        }

        // PATCH: api/bottimeouts/patch/2?username=simple_sandman
        // Body (JSON): [{ "op": "replace", "path": "/timeout", "value": "1/1/1970 12:00:00 AM" }]
        // Special note: The "timeout" value is based on T-SQL DateTime so any format it can take will do
        [HttpPatch("{broadcasterId:int}")]
        public async Task<IActionResult> Patch([FromRoute] int broadcasterId, [FromQuery] string username, [FromBody]JsonPatchDocument<BotTimeout> botTimeoutPatch)
        {
            BotTimeout userBotTimeout = _context.BotTimeout.SingleOrDefault(m => m.Username == username && m.BroadcasterId == broadcasterId);

            if (userBotTimeout == null)
            {
                return BadRequest();
            }

            botTimeoutPatch.ApplyTo(userBotTimeout, ModelState);

            if (!ModelState.IsValid)
            {
                return new BadRequestObjectResult(ModelState);
            }

            _context.BotTimeout.Update(userBotTimeout);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserBotTimeoutExists(broadcasterId, username))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok(userBotTimeout);
        }

        // POST: api/bottimeouts/create
        // Body (JSON): { "username": "hello_world", "timeout": "2020-01-01T00:00:00", "broadcaster": 2 }
        // Special note: The "timeout" value is based on T-SQL DateTime so any format it can take will do
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BotTimeout userBotTimeout)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.BotTimeout.Add(userBotTimeout);
            await _context.SaveChangesAsync();

            return CreatedAtAction("Get", new { broadcasterId = userBotTimeout.Broadcaster, username = userBotTimeout.Username }, userBotTimeout);
        }

        // DELETE: api/bottimeouts/delete/2
        // DELETE: api/bottimeouts/delete/2?username=simple_sandman
        [HttpDelete("{broadcasterId:int}")]
        public async Task<IActionResult> Delete([FromRoute] int broadcasterId, [FromQuery] string username = "")
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var botTimeout = new object();

            if (!string.IsNullOrEmpty(username))
            {
                BotTimeout userBotTimeout = await _context.BotTimeout
                    .SingleOrDefaultAsync(m => m.Username == username && m.BroadcasterId == broadcasterId);

                if (userBotTimeout == null)
                    return NotFound();

                _context.BotTimeout.Remove(userBotTimeout);

                botTimeout = userBotTimeout;
            }
            else
            {
                List<BotTimeout> userBotTimeouts = await _context.BotTimeout
                    .Where(m => m.BroadcasterId == broadcasterId && m.Timeout < DateTime.UtcNow)
                    .ToListAsync();

                if (userBotTimeouts == null || userBotTimeouts.Count == 0)
                    return NotFound();

                _context.BotTimeout.RemoveRange(userBotTimeouts);

                botTimeout = userBotTimeouts;
            }

            await _context.SaveChangesAsync();

            return Ok(botTimeout);
        }

        private bool UserBotTimeoutExists(int broadcasterId, string username)
        {
            return _context.BotTimeout.Any(e => e.Id == broadcasterId && e.Username == username);
        }
    }
}