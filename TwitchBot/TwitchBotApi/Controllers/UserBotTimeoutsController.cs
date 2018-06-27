using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TwitchBotDb.Models;

namespace TwitchBotApi.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]/[action]")]
    public class UserBotTimeoutsController : Controller
    {
        private readonly TwitchBotDbContext _context;

        public UserBotTimeoutsController(TwitchBotDbContext context)
        {
            _context = context;
        }

        // GET: api/userbottimeouts/get/2
        // GET: api/userbottimeouts/get/2?username=simple_sandman
        [HttpGet("{broadcasterId:int}")]
        public async Task<IActionResult> Get([FromRoute] int broadcasterId, [FromQuery] string username = "")
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var botTimeouts = new object();

            if (string.IsNullOrEmpty(username))
                botTimeouts = await _context.UserBotTimeout.Where(m => m.Broadcaster == broadcasterId).ToListAsync();
            else
                botTimeouts = await _context.UserBotTimeout.SingleOrDefaultAsync(m => m.Broadcaster == broadcasterId && m.Username == username);

            if (botTimeouts == null)
            {
                return NotFound();
            }

            return Ok(botTimeouts);
        }

        // PATCH: api/userbottimeouts/patch/2?username=simple_sandman
        // Body (JSON): [{ "op": "replace", "path": "/timeout", "value": "1/1/1970 12:00:00 AM" }]
        // Special note: The "timeout" value is based on T-SQL DateTime so any format it can take will do
        [HttpPatch("{broadcasterId:int}")]
        public async Task<IActionResult> Patch([FromRoute] int broadcasterId, [FromQuery] string username, [FromBody]JsonPatchDocument<UserBotTimeout> botTimeoutPatch)
        {
            UserBotTimeout userBotTimeout = _context.UserBotTimeout.SingleOrDefault(m => m.Username == username && m.Broadcaster == broadcasterId);

            if (userBotTimeout == null)
            {
                return BadRequest();
            }

            botTimeoutPatch.ApplyTo(userBotTimeout, ModelState);

            if (!ModelState.IsValid)
            {
                return new BadRequestObjectResult(ModelState);
            }

            _context.UserBotTimeout.Update(userBotTimeout);

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

        // POST: api/userbottimeouts/create
        // Body (JSON): { "username": "hello_world", "timeout": "2020-01-01T00:00:00", "broadcaster": 2 }
        // Special note: The "timeout" value is based on T-SQL DateTime so any format it can take will do
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UserBotTimeout userBotTimeout)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.UserBotTimeout.Add(userBotTimeout);
            await _context.SaveChangesAsync();

            return CreatedAtAction("Get", new { broadcasterId = userBotTimeout.Broadcaster, username = userBotTimeout.Username }, userBotTimeout);
        }

        // DELETE: api/userbottimeouts/delete/2?username=simple_sandman
        [HttpDelete("{broadcasterId:int}")]
        public async Task<IActionResult> Delete([FromRoute] int broadcasterId, [FromQuery] string username)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userBotTimeout = await _context.UserBotTimeout.SingleOrDefaultAsync(m => m.Username == username && m.Broadcaster == broadcasterId);
            if (userBotTimeout == null)
            {
                return NotFound();
            }

            _context.UserBotTimeout.Remove(userBotTimeout);
            await _context.SaveChangesAsync();

            return Ok(userBotTimeout);
        }

        private bool UserBotTimeoutExists(int broadcasterId, string username)
        {
            return _context.UserBotTimeout.Any(e => e.Id == broadcasterId && e.Username == username);
        }
    }
}