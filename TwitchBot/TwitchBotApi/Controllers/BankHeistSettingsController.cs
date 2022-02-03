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
    public class BankHeistSettingsController : ControllerBase
    {
        private readonly SimpleBotContext _context;

        public BankHeistSettingsController(SimpleBotContext context)
        {
            _context = context;
        }

        // GET: api/bankheistsettings/get/2
        [HttpGet("{broadcasterId:int}")]
        public async Task<IActionResult> Get([FromRoute] int broadcasterId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            BankHeistSetting bankHeistSetting = await _context.BankHeistSettings.SingleOrDefaultAsync(m => m.BroadcasterId == broadcasterId);

            if (bankHeistSetting == null)
            {
                return NotFound();
            }

            return Ok(bankHeistSetting);
        }

        // PUT: api/bankheistsettings/update/2
        [HttpPut("{broadcasterId:int}")]
        public async Task<IActionResult> Update([FromRoute] int broadcasterId, [FromBody] BankHeistSetting bankHeistSetting)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (broadcasterId != bankHeistSetting.BroadcasterId)
            {
                return BadRequest();
            }

            _context.Entry(bankHeistSetting).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BankHeistSettingExists(broadcasterId))
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

        // POST: api/bankheistsettings/create
        // Body (JSON): { "broadcaster": 2 }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BankHeistSetting bankHeistSetting)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.BankHeistSettings.Add(bankHeistSetting);
            await _context.SaveChangesAsync();

            return CreatedAtAction("Get", new { broadcasterId = bankHeistSetting.BroadcasterId }, bankHeistSetting);
        }

        private bool BankHeistSettingExists(int broadcasterId)
        {
            return _context.BankHeistSettings.Any(e => e.BroadcasterId == broadcasterId);
        }
    }
}