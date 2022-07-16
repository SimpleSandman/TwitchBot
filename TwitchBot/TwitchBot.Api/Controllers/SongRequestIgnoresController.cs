using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TwitchBot.Api.Helpers;

using TwitchBotDb.Context;
using TwitchBotDb.Models;

namespace TwitchBot.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SongRequestIgnoresController : ControllerBase
    {
        private readonly SimpleBotContext _context;

        public SongRequestIgnoresController(SimpleBotContext context)
        {
            _context = context;
        }

        // GET: api/songrequestblacklists/get/2
        // GET: api/songrequestblacklists/get/2?id=1
        [HttpGet("{broadcasterId}")]
        public async Task<IActionResult> Get(int broadcasterId, [FromQuery] int id = 0)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            object? songRequestIgnore = new object();

            if (id > 0)
            {
                songRequestIgnore = await _context.SongRequestIgnores
                    .SingleOrDefaultAsync(m => m.BroadcasterId == broadcasterId && m.Id == id);
            }
            else
            {
                songRequestIgnore = await _context.SongRequestIgnores
                    .Where(m => m.BroadcasterId == broadcasterId)
                    .ToListAsync();
            }

            if (songRequestIgnore == null)
            {
                throw new NotFoundException("Ignored song request cannot be found");
            }

            return Ok(songRequestIgnore);
        }

        // PUT: api/songrequestblacklists/update/2?id=1
        // Body (JSON): { "id": 1, "artist": "some stupid artist", "broadcaster": 2 }
        // Body (JSON): { "id": 1, "artist": "some stupid artist", "title": "some stupid title", "broadcaster": 2 }
        [HttpPut("{broadcasterId}")]
        public async Task<IActionResult> Update(int broadcasterId, [FromQuery] int id, [FromBody] SongRequestIgnore songRequestIgnore)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != songRequestIgnore.Id && broadcasterId != songRequestIgnore.BroadcasterId)
            {
                throw new ApiException("ID or broadcaster ID does not match ignored song request's ID or broadcaster ID");
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
                    throw new NotFoundException("Ignored song request cannot be found");
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

            _context.SongRequestIgnores.Add(songRequestIgnore);
            await _context.SaveChangesAsync();

            return CreatedAtAction("Get", 
                new 
                { 
                    broadcasterId = songRequestIgnore.BroadcasterId, 
                    id = songRequestIgnore.Id 
                }, 
                songRequestIgnore);
        }

        // DELETE: api/songrequestblacklists/delete/2
        // DELETE: api/songrequestblacklists/delete/2?artist=someone
        // DELETE: api/songrequestblacklists/delete/2?artist=someone&title=something
        [HttpDelete("{broadcasterId}")]
        public async Task<IActionResult> Delete(int broadcasterId, [FromQuery] string artist = "", [FromQuery] string title = "")
        {
            // don't accept an invalid model state or a request with just the title
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            else if (string.IsNullOrEmpty(artist) && !string.IsNullOrEmpty(title))
            {
                throw new ApiException("Artist or title are null or empty");
            }

            object songRequestIgnore = new object();

            if (!string.IsNullOrEmpty(artist) && !string.IsNullOrEmpty(title))
            {
                SongRequestIgnore? songRequestIgnoredItem = await _context.SongRequestIgnores
                    .SingleOrDefaultAsync(m => m.BroadcasterId == broadcasterId
                        && m.Artist.Equals(artist, StringComparison.CurrentCultureIgnoreCase)
                        && m.Title.Equals(title, StringComparison.CurrentCultureIgnoreCase));

                if (songRequestIgnoredItem == null)
                {
                    throw new NotFoundException("Ignored song request cannot be found");
                }

                _context.SongRequestIgnores.Remove(songRequestIgnoredItem);

                songRequestIgnore = songRequestIgnoredItem;
            }
            else
            {
                List<SongRequestIgnore> songRequestBlacklistItems = new List<SongRequestIgnore>();

                if (!string.IsNullOrEmpty(artist))
                {
                    songRequestBlacklistItems = await _context.SongRequestIgnores
                        .Where(m => m.BroadcasterId == broadcasterId
                            && m.Artist.Equals(artist, StringComparison.CurrentCultureIgnoreCase))
                        .ToListAsync();
                }
                else
                {
                    songRequestBlacklistItems = await _context.SongRequestIgnores
                        .Where(m => m.BroadcasterId == broadcasterId)
                        .ToListAsync();
                }

                if (songRequestBlacklistItems == null || songRequestBlacklistItems.Count == 0)
                {
                    throw new NotFoundException("Ignored song request(s) cannot be found");
                }

                _context.SongRequestIgnores.RemoveRange(songRequestBlacklistItems);

                songRequestIgnore = songRequestBlacklistItems;
            }

            await _context.SaveChangesAsync();

            return Ok(songRequestIgnore);
        }

        private bool SongRequestBlacklistExists(int id)
        {
            return _context.SongRequestIgnores.Any(e => e.Id == id);
        }
    }
}
