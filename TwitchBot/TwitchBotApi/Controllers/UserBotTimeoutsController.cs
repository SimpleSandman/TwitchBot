using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TwitchBotApi.Models;

namespace TwitchBotApi.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]/[action]")]
    public class UserBotTimeoutsController : Controller
    {
        private readonly TwitchBotContext _context;

        public UserBotTimeoutsController(TwitchBotContext context)
        {
            _context = context;
        }

        // GET: api/userbottimeouts/get/2
        [HttpGet("{broadcasterId:int}")]
        public async Task<IActionResult> Get([FromRoute] int broadcasterId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            List<UserBotTimeout> botTimeouts = await _context.UserBotTimeout.Where(m => m.Broadcaster == broadcasterId).ToListAsync();

            if (botTimeouts == null)
            {
                return NotFound();
            }

            return Ok(botTimeouts);
        }

        // PATCH: api/userbottimeouts/patch/2?id=5
        // Body (JSON): [{ "op": "replace", "path": "/timeout", "value": "1/1/1970 12:00:00 AM" }]
        // Special note: The "timeout" value is based on T-SQL DateTime so any format it can take will do
        [HttpPatch("{broadcasterId:int}")]
        public async Task<IActionResult> Patch([FromRoute] int broadcasterId, [FromQuery] int id, [FromBody]JsonPatchDocument<UserBotTimeout> botTimeoutPatch)
        {
            UserBotTimeout botTimeout = _context.UserBotTimeout.SingleOrDefault(m => m.Id == id && m.Broadcaster == broadcasterId);

            if (botTimeout == null)
            {
                return BadRequest();
            }

            botTimeoutPatch.ApplyTo(botTimeout, ModelState);

            if (!ModelState.IsValid)
            {
                return new BadRequestObjectResult(ModelState);
            }

            _context.UserBotTimeout.Update(botTimeout);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserBotTimeoutExists(id))
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

            return NoContent();
        }

        // DELETE: api/userbottimeouts/delete/2?id=2
        [HttpDelete("{broadcasterId:int}")]
        public async Task<IActionResult> Delete([FromRoute] int broadcasterId, [FromQuery] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userBotTimeout = await _context.UserBotTimeout.SingleOrDefaultAsync(m => m.Id == id && m.Broadcaster == broadcasterId);
            if (userBotTimeout == null)
            {
                return NotFound();
            }

            _context.UserBotTimeout.Remove(userBotTimeout);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserBotTimeoutExists(int id)
        {
            return _context.UserBotTimeout.Any(e => e.Id == id);
        }
    }
}