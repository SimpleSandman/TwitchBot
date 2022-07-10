using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TwitchBot.Api.Helpers;
using TwitchBotDb.Context;
using TwitchBotDb.Models;

namespace TwitchBot.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BankHeistSettingsController : ControllerBase
    {
        private readonly SimpleBotContext _context;

        public BankHeistSettingsController(SimpleBotContext context)
        {
            _context = context;
        }

        // GET: api/bankheistsettings/2
        [HttpGet("{broadcasterId:int}")]
        public async Task<IActionResult> Get([FromRoute] int broadcasterId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            BankHeistSetting? bankHeistSetting = await _context.BankHeistSettings
                .SingleOrDefaultAsync(m => m.BroadcasterId == broadcasterId);

            if (bankHeistSetting == null)
            {
                throw new KeyNotFoundException("Bank heist setting not found");
            }

            return Ok(bankHeistSetting);
        }

        // PUT: api/bankheistsettings/2
        [HttpPut("{broadcasterId:int}")]
        public async Task<IActionResult> Update([FromRoute] int broadcasterId, [FromBody] BankHeistSetting bankHeistSetting)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (broadcasterId != bankHeistSetting.BroadcasterId)
            {
                throw new ApiException("Broadcaster ID does not match with bank heist setting's broadcaster ID");
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
                    throw new KeyNotFoundException("Bank heist setting not found");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/bankheistsettings
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
