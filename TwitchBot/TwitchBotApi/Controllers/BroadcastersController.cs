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

        [HttpGet("{twitchId:int}")]
        public async Task<IActionResult> GetByTwitchId([FromRoute] int twitchId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Broadcasters broadcaster = await _context.Broadcasters.SingleOrDefaultAsync(m => m.TwitchId == twitchId);

            if (broadcaster == null)
            {
                return NotFound();
            }

            return Ok(broadcaster);
        }

        // GET: api/broadcasters/getbyuserinfo/simple_sandman
        [HttpGet("{twitchId:int}")]
        public async Task<IActionResult> GetByUserInfo([FromRoute] int twitchId, [FromQuery] string username)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Broadcasters broadcaster = await _context.Broadcasters.SingleOrDefaultAsync(m => m.Username == username && m.TwitchId == twitchId);

            if (broadcaster == null)
            {
                return NotFound();
            }

            return Ok(broadcaster);
        }

        // PUT: api/Broadcasters/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBroadcasters([FromRoute] int id, [FromBody] Broadcasters broadcasters)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != broadcasters.Id)
            {
                return BadRequest();
            }

            _context.Entry(broadcasters).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BroadcastersExists(id))
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

        // POST: api/Broadcasters
        [HttpPost]
        public async Task<IActionResult> PostBroadcasters([FromBody] Broadcasters broadcasters)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Broadcasters.Add(broadcasters);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetBroadcasters", new { id = broadcasters.Id }, broadcasters);
        }

        private bool BroadcastersExists(int id)
        {
            return _context.Broadcasters.Any(e => e.Id == id);
        }
    }
}