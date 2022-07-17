using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TwitchBot.Api.Helpers.ErrorExceptions;

using TwitchBotDb.Context;
using TwitchBotDb.Models;

namespace TwitchBot.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class BossFightClassStatsController : ControllerBase
    {
        private readonly SimpleBotContext _context;

        public BossFightClassStatsController(SimpleBotContext context)
        {
            _context = context;
        }

        // GET: api/bossfightclassstats/get/1
        [HttpGet("{settingsId}")]
        public async Task<IActionResult> Get(int settingsId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            BossFightClassStats? bossFightClassStats = await _context.BossFightClassStats
                .SingleOrDefaultAsync(m => m.SettingsId == settingsId);

            if (bossFightClassStats == null)
            {
                throw new NotFoundException("Boss fight class stats not found");
            }

            return Ok(bossFightClassStats);
        }

        // PUT: api/bossfightclassstats/update/1?id=1
        [HttpPut("{settingsId}")]
        public async Task<IActionResult> Update(int settingsId, [FromQuery] int id, [FromBody] BossFightClassStats bossFightClassStats)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != bossFightClassStats.Id || settingsId != bossFightClassStats.SettingsId)
            {
                throw new ApiException("Settings ID does not match with boss fight class stats's settings ID");
            }

            _context.Entry(bossFightClassStats).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BossFightClassStatsExists(id))
                {
                    throw new NotFoundException("Boss fight class stats not found");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/bossfightclassstats/create
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BossFightClassStats bossFightClassStats)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.BossFightClassStats.Add(bossFightClassStats);
            await _context.SaveChangesAsync();

            return CreatedAtAction("Get", new { settingsId = bossFightClassStats.Id }, bossFightClassStats);
        }

        private bool BossFightClassStatsExists(int id)
        {
            return _context.BossFightClassStats.Any(e => e.Id == id);
        }
    }
}
