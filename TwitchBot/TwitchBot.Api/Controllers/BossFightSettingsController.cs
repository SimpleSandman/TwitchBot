using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TwitchBot.Api.Helpers;

using TwitchBotDb.Context;
using TwitchBotDb.Models;

namespace TwitchBot.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class BossFightSettingsController : ControllerBase
    {
        private readonly SimpleBotContext _context;

        public BossFightSettingsController(SimpleBotContext context)
        {
            _context = context;
        }

        // GET: api/bossfightsettings/get/2
        [HttpGet("{broadcasterId}")]
        public async Task<IActionResult> Get(int broadcasterId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            BossFightSetting? bossFightSetting = await _context.BossFightSettings
                .SingleOrDefaultAsync(m => m.BroadcasterId == broadcasterId);

            if (bossFightSetting == null)
            {
                throw new NotFoundException("Boss fight settings not found");
            }

            return Ok(bossFightSetting);
        }

        // PUT: api/bossfightsettings/update/2
        [HttpPut("{broadcasterId}")]
        public async Task<IActionResult> Update(int broadcasterId, [FromBody] BossFightSetting bossFightSetting)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (broadcasterId != bossFightSetting.BroadcasterId)
            {
                throw new ApiException("Broadcaster ID does not match with boss fight class stats's broadcaster ID");
            }

            _context.Entry(bossFightSetting).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BossFightSettingExists(broadcasterId))
                {
                    throw new NotFoundException("Boss fight settings not found");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/bossfightsettings/create
        // Body (JSON): { "broadcaster": 2 }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BossFightSetting bossFightSetting)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.BossFightSettings.Add(bossFightSetting);
            await _context.SaveChangesAsync();

            return CreatedAtAction("Get", new { broadcasterId = bossFightSetting.BroadcasterId }, bossFightSetting);
        }

        private bool BossFightSettingExists(int broadcasterId)
        {
            return _context.BossFightSettings.Any(e => e.BroadcasterId == broadcasterId);
        }
    }
}
