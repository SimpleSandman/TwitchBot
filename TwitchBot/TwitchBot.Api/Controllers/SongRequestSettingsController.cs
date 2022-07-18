using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TwitchBot.Api.Helpers;
using TwitchBot.Api.Helpers.ErrorExceptions;
using TwitchBotDb.Context;
using TwitchBotDb.Models;

namespace TwitchBot.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SongRequestSettingsController : ExtendedControllerBase
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
            IsModelStateValid();

            SongRequestSetting? songRequestSetting = await _context.SongRequestSettings
                .SingleOrDefaultAsync(m => m.BroadcasterId == broadcasterId);

            if (songRequestSetting == null)
            {
                throw new ApiException("Song request setting cannot be found");
            }

            return Ok(songRequestSetting);
        }

        // PUT: api/songrequestsettings/update/5
        [HttpPut("{broadcasterId:int}")]
        public async Task<IActionResult> Update([FromRoute] int broadcasterId, [FromBody] SongRequestSetting songRequestSetting)
        {
            IsModelStateValid();

            if (broadcasterId != songRequestSetting.BroadcasterId)
            {
                throw new ApiException("Broadcaster ID does not match song request setting's broadcaster ID");
            }

            SongRequestSetting? updatedSongRequestSetting = await _context.SongRequestSettings
                .FirstOrDefaultAsync(m => m.BroadcasterId == broadcasterId);

            if (updatedSongRequestSetting == null)
            {
                throw new ApiException("Song request setting cannot be found");
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
                    throw new ApiException("Song request setting cannot be found");
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
            IsModelStateValid();

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
