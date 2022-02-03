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
    public class BossFightClassStatsController : ControllerBase
    {
        private readonly SimpleBotContext _context;

        public BossFightClassStatsController(SimpleBotContext context)
        {
            _context = context;
        }

        // GET: api/bossfightclassstats/get/1
        [HttpGet("{settingsId:int}")]
        public async Task<IActionResult> Get([FromRoute] int settingsId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            BossFightClassStats bossFightClassStats = await _context.BossFightClassStats.SingleOrDefaultAsync(m => m.SettingsId == settingsId);

            if (bossFightClassStats == null)
            {
                return NotFound();
            }

            return Ok(bossFightClassStats);
        }

        // PUT: api/bossfightclassstats/update/1?id=1
        [HttpPut("{settingsId:int}")]
        public async Task<IActionResult> Update([FromRoute] int settingsId, [FromQuery] int id, [FromBody] BossFightClassStats bossFightClassStats)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != bossFightClassStats.Id || settingsId != bossFightClassStats.SettingsId)
            {
                return BadRequest();
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
                    return NotFound();
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