using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TwitchBotDb.Context;
using TwitchBotDb.Models;

namespace TwitchBotApi.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]/[action]")]
    public class SongRequestSettingsController : ControllerBase
    {
        private readonly SimpleBotContext _context;

        public SongRequestSettingsController(SimpleBotContext context)
        {
            _context = context;
        }

        // GET: api/songrequestsettings/get/5
        [HttpGet("{broadcasterId:int}")]
        public async Task<IActionResult> Get([FromRoute] int broadcasterId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            SongRequestSetting songRequestSetting = await _context.SongRequestSettings.SingleOrDefaultAsync(m => m.BroadcasterId == broadcasterId);

            if (songRequestSetting == null)
            {
                return NotFound();
            }

            return Ok(songRequestSetting);
        }

        // PUT: api/songrequestsettings/update/5
        [HttpPut("{broadcasterId:int}")]
        public async Task<IActionResult> Update([FromRoute] int broadcasterId, [FromBody] SongRequestSetting songRequestSetting)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (broadcasterId != songRequestSetting.BroadcasterId)
            {
                return BadRequest();
            }

            SongRequestSetting updatedSongRequestSetting = await _context.SongRequestSettings.FirstOrDefaultAsync(m => m.BroadcasterId == broadcasterId);
            if (updatedSongRequestSetting == null)
            {
                return NotFound();
            }

            updatedSongRequestSetting.PersonalPlaylistId = songRequestSetting.PersonalPlaylistId;
            updatedSongRequestSetting.RequestPlaylistId = songRequestSetting.RequestPlaylistId;
            updatedSongRequestSetting.DjMode = songRequestSetting.DjMode;
            _context.SongRequestSettings.Update(updatedSongRequestSetting);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SongRequestSettingExists(broadcasterId))
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

        // POST: api/songrequestsettings/create
        // Body (JSON): 
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SongRequestSetting songRequestSetting)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.SongRequestSettings.Add(songRequestSetting);
            await _context.SaveChangesAsync();

            return CreatedAtAction("Get", new { broadcasterId = songRequestSetting.BroadcasterId }, songRequestSetting);
        }

        private bool SongRequestSettingExists(int broadcasterId)
        {
            return _context.SongRequestSettings.Any(e => e.BroadcasterId == broadcasterId);
        }
    }
}