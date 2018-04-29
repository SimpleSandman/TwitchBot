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
    public class BroadcastersController : Controller
    {
        private readonly TwitchBotContext _context;

        public BroadcastersController(TwitchBotContext context)
        {
            _context = context;
        }

        // GET: api/broadcasters/getuserinfo/12345678
        // GET: api/broadcasters/getuserinfo/12345678?username=simple_sandman
        [HttpGet("{twitchId:int}")]
        public async Task<IActionResult> GetUserInfo([FromRoute] int twitchId, [FromQuery] string username = "")
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

        // PUT: api/broadcasters/updateusername/12345678
        // Body (JSON): { "id": 2, "username": "simple_sandman", "timeAdded": "1970-01-01T00:00:00.000", "twitchId": 12345678 }
        [HttpPut("{twitchId:int}")]
        public async Task<IActionResult> UpdateUsername([FromRoute] int twitchId, [FromBody] Broadcasters broadcaster)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (twitchId != broadcaster.TwitchId)
            {
                return BadRequest();
            }

            _context.Entry(broadcaster).State = EntityState.Modified;

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

            return CreatedAtAction("GetUserInfo", new { twitchId = broadcaster.TwitchId }, broadcaster);
        }

        private bool BroadcasterExists(int twitchId)
        {
            return _context.Broadcasters.Any(e => e.TwitchId == twitchId);
        }
    }
}