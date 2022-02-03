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
    public class BossFightSettingsController : ControllerBase
    {
        private readonly SimpleBotContext _context;

        public BossFightSettingsController(SimpleBotContext context)
        {
            _context = context;
        }

        // GET: api/bossfightsettings/get/2
        [HttpGet("{broadcasterId:int}")]
        public async Task<IActionResult> Get([FromRoute] int broadcasterId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            BossFightSetting bossFightSetting = await _context.BossFightSettings.SingleOrDefaultAsync(m => m.BroadcasterId == broadcasterId);

            if (bossFightSetting == null)
            {
                return NotFound();
            }

            return Ok(bossFightSetting);
        }

        // PUT: api/bossfightsettings/update/2
        [HttpPut("{broadcasterId:int}")]
        public async Task<IActionResult> Update([FromRoute] int broadcasterId, [FromBody] BossFightSetting bossFightSetting)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (broadcasterId != bossFightSetting.BroadcasterId)
            {
                return BadRequest();
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
                    return NotFound();
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