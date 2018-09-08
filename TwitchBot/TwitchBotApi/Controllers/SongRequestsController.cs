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

            List<SongRequest> songRequests = await _context.SongRequest.Where(m => m.BroadcasterId == broadcasterId).ToListAsync();

            if (songRequests == null)
            {
                return NotFound();
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

            _context.SongRequest.Add(songRequests);
            await _context.SaveChangesAsync();

            return CreatedAtAction("Get", new { broadcasterId = songRequests.Broadcaster }, songRequests);
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

            var songRequests = new object();

            if (popOne)
            {
                SongRequest songRequest = await _context.SongRequest
                    .Where(m => m.BroadcasterId == broadcasterId)
                    .OrderBy(m => m.Id)
                    .Take(1)
                    .SingleOrDefaultAsync();

                if (songRequest == null)
                    return NotFound();

                _context.SongRequest.Remove(songRequest);

                songRequests = songRequest;
            }
            else
            {
                List<SongRequest> removedSong = await _context.SongRequest.Where(m => m.BroadcasterId == broadcasterId).ToListAsync();

                if (removedSong == null || removedSong.Count == 0)
                    return NotFound();

                _context.SongRequest.RemoveRange(removedSong);

                songRequests = removedSong;
            }

            await _context.SaveChangesAsync();

            return Ok(songRequests);
        }
    }
}