using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TwitchBot.Api.Helpers;

using TwitchBotDb.Context;
using TwitchBotDb.Models;

namespace TwitchBot.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SongRequestsController : ControllerBase
    {
        private readonly SimpleBotContext _context;

        public SongRequestsController(SimpleBotContext context)
        {
            _context = context;
        }

        // GET: api/songrequests/5
        [HttpGet("{broadcasterId:int}")]
        public async Task<IActionResult> Get([FromRoute] int broadcasterId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            List<SongRequest> songRequests = await _context.SongRequests.Where(m => m.BroadcasterId == broadcasterId).ToListAsync();

            if (songRequests == null)
            {
                throw new NotFoundException("Song requests cannot be found");
            }

            return Ok(songRequests);
        }

        // POST: api/songrequests/create
        // Body (JSON): 
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SongRequest songRequests)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.SongRequests.Add(songRequests);
            await _context.SaveChangesAsync();

            return CreatedAtAction("Get", new { broadcasterId = songRequests.BroadcasterId }, songRequests);
        }

        // DELETE: api/songrequests/2
        // DELETE: api/songrequests/2?popOne=true
        [HttpDelete("{broadcasterId:int}")]
        public async Task<IActionResult> Delete([FromRoute] int broadcasterId, [FromQuery] bool popOne = false)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            object songRequests = new object();

            if (popOne)
            {
                SongRequest? songRequest = await _context.SongRequests
                    .Where(m => m.BroadcasterId == broadcasterId)
                    .OrderBy(m => m.Id)
                    .Take(1)
                    .SingleOrDefaultAsync();

                if (songRequest == null)
                {
                    throw new NotFoundException("Song request cannot be found");
                }

                _context.SongRequests.Remove(songRequest);

                songRequests = songRequest;
            }
            else
            {
                List<SongRequest> removedSong = await _context.SongRequests
                    .Where(m => m.BroadcasterId == broadcasterId)
                    .ToListAsync();

                if (removedSong == null || removedSong.Count == 0)
                {
                    throw new NotFoundException("Song requests cannot be found");
                }

                _context.SongRequests.RemoveRange(removedSong);

                songRequests = removedSong;
            }

            await _context.SaveChangesAsync();

            return Ok(songRequests);
        }
    }
}
