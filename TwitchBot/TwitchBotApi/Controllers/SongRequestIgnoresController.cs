using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TwitchBotDb.Models;

namespace TwitchBotApi.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]/[action]")]
    public class SongRequestIgnoresController : ControllerBase
    {
        private readonly SimpleBotContext _context;

        public SongRequestIgnoresController(SimpleBotContext context)
        {
            _context = context;
        }

        // GET: api/songrequestblacklists/get/2
        // GET: api/songrequestblacklists/get/2?id=1
        [HttpGet("{broadcasterId:int}")]
        public async Task<IActionResult> Get([FromRoute] int broadcasterId, [FromQuery] int id = 0)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var songRequestIgnore = new object();

            if (id > 0)
                songRequestIgnore = await _context.SongRequestIgnore.SingleOrDefaultAsync(m => m.BroadcasterId == broadcasterId && m.Id == id);
            else
                songRequestIgnore = await _context.SongRequestIgnore.Where(m => m.BroadcasterId == broadcasterId).ToListAsync();

            if (songRequestIgnore == null)
            {
                return NotFound();
            }

            return Ok(songRequestIgnore);
        }

        // PUT: api/songrequestblacklists/update/2?id=1
        // Body (JSON): { "id": 1, "artist": "some stupid artist", "broadcaster": 2 }
        // Body (JSON): { "id": 1, "artist": "some stupid artist", "title": "some stupid title", "broadcaster": 2 }
        [HttpPut("{broadcasterId:int}")]
        public async Task<IActionResult> Update([FromRoute] int broadcasterId, [FromQuery] int id, [FromBody] SongRequestIgnore songRequestIgnore)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != songRequestIgnore.Id && broadcasterId != songRequestIgnore.BroadcasterId)
            {
                return BadRequest();
            }

            _context.Entry(songRequestIgnore).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SongRequestBlacklistExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok(songRequestIgnore);
        }

        // POST: api/songrequestblacklists/create
        // Body (JSON): { "artist": "some stupid artist", "broadcaster": 2 }
        // Body (JSON): { "artist": "some stupid artist", "title": "some stupid title", "broadcaster": 2 }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SongRequestIgnore songRequestIgnore)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.SongRequestIgnore.Add(songRequestIgnore);
            await _context.SaveChangesAsync();

            return CreatedAtAction("Get", new { broadcasterId = songRequestIgnore.Broadcaster, id = songRequestIgnore.Id }, songRequestIgnore);
        }

        // DELETE: api/songrequestblacklists/delete/2
        // DELETE: api/songrequestblacklists/delete/2?artist=someone
        // DELETE: api/songrequestblacklists/delete/2?artist=someone&title=something
        [HttpDelete("{broadcasterId:int}")]
        public async Task<IActionResult> Delete([FromRoute] int broadcasterId, [FromQuery] string artist = "", [FromQuery] string title = "")
        {
            // don't accept an invalid model state or a request with just the title
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            else if (string.IsNullOrEmpty(artist) && !string.IsNullOrEmpty(title))
            {
                return BadRequest();
            }

            var songRequestIgnore = new object();

            if (!string.IsNullOrEmpty(artist) && !string.IsNullOrEmpty(title))
            {
                SongRequestIgnore songRequestIgnoredItem = await _context.SongRequestIgnore
                    .SingleOrDefaultAsync(m => m.BroadcasterId == broadcasterId 
                        && m.Artist.Equals(artist, StringComparison.CurrentCultureIgnoreCase) 
                        && m.Title.Equals(title, StringComparison.CurrentCultureIgnoreCase));

                if (songRequestIgnoredItem == null)
                    return NotFound();

                _context.SongRequestIgnore.Remove(songRequestIgnoredItem);

                songRequestIgnore = songRequestIgnoredItem;
            }
            else
            {
                List<SongRequestIgnore> songRequestBlacklistItems = new List<SongRequestIgnore>();

                if (!string.IsNullOrEmpty(artist))
                {
                    songRequestBlacklistItems = await _context.SongRequestIgnore
                        .Where(m => m.BroadcasterId == broadcasterId 
                            && m.Artist.Equals(artist, StringComparison.CurrentCultureIgnoreCase))
                        .ToListAsync();
                }
                else
                {
                    songRequestBlacklistItems = await _context.SongRequestIgnore
                        .Where(m => m.BroadcasterId == broadcasterId)
                        .ToListAsync();
                }

                if (songRequestBlacklistItems == null || songRequestBlacklistItems.Count == 0)
                    return NotFound();

                _context.SongRequestIgnore.RemoveRange(songRequestBlacklistItems);

                songRequestIgnore = songRequestBlacklistItems;
            }

            await _context.SaveChangesAsync();

            return Ok(songRequestIgnore);
        }

        private bool SongRequestBlacklistExists(int id)
        {
            return _context.SongRequestIgnore.Any(e => e.Id == id);
        }
    }
}