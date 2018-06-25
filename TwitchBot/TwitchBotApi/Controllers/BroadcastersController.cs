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
    public class BroadcastersController : Controller
    {
        private readonly TwitchBotDbContext _context;

        public BroadcastersController(TwitchBotDbContext context)
        {
            _context = context;
        }

        // GET: api/broadcasters/get/12345678
        // GET: api/broadcasters/get/12345678?username=simple_sandman
        [HttpGet("{twitchId:int}")]
        public async Task<IActionResult> Get([FromRoute] int twitchId, [FromQuery] string username = "")
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Broadcasters broadcaster = new Broadcasters();

            if (!string.IsNullOrEmpty(username))
                broadcaster = await _context.Broadcasters.SingleOrDefaultAsync(m => m.Username == username && m.TwitchId == twitchId);
            else
                broadcaster = await _context.Broadcasters.SingleOrDefaultAsync(m => m.TwitchId == twitchId);

            if (broadcaster == null)
            {
                return NotFound();
            }

            return Ok(broadcaster);
        }

        // PUT: api/broadcasters/update/12345678
        // Body (JSON): { "username": "simple_sandman", "twitchId": 12345678 }
        [HttpPut("{twitchId:int}")]
        public async Task<IActionResult> Update([FromRoute] int twitchId, [FromBody] Broadcasters broadcaster)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (twitchId != broadcaster.TwitchId)
            {
                return BadRequest();
            }

            Broadcasters updatedbroadcaster = await _context.Broadcasters.FirstOrDefaultAsync(m => m.TwitchId == twitchId);
            if (updatedbroadcaster == null)
            {
                return NotFound();
            }

            updatedbroadcaster.Username = broadcaster.Username;
            _context.Broadcasters.Update(updatedbroadcaster);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BroadcasterExists(twitchId))
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

        // POST: api/broadcasters/create
        // Body (JSON): { "username": "simple_sandman", "twitchId": 12345678 }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Broadcasters broadcaster)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Broadcasters.Add(broadcaster);
            await _context.SaveChangesAsync();

            return CreatedAtAction("Get", new { twitchId = broadcaster.TwitchId }, broadcaster);
        }

        private bool BroadcasterExists(int twitchId)
        {
            return _context.Broadcasters.Any(e => e.TwitchId == twitchId);
        }
    }
}