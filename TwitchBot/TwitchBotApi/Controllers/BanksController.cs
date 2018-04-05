using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Snickler.EFCore;

using TwitchBotApi.DTO;
using TwitchBotApi.Extensions;
using TwitchBotApi.Models;

namespace TwitchBotApi.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class BanksController : Controller
    {
        private readonly TwitchBotContext _context;

        public BanksController(TwitchBotContext context)
        {
            _context = context;
        }

        // GET: api/banks
        //[HttpGet]
        //public IEnumerable<Bank> GetBank()
        //{
        //    return _context.Bank;
        //}

        // GET: api/banks/5
        // GET: api/banks/5?username=simple_sandman
        [HttpGet("{broadcasterId:int}")]
        public async Task<IActionResult> GetBank([FromRoute] int broadcasterId, [FromQuery] string username = "")
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var bank = await _context.Bank.Where(m => m.Broadcaster == broadcasterId).ToListAsync();

            if (!string.IsNullOrEmpty(username))
                bank = bank.Where(m => m.Username == username).ToList();

            if (bank == null)
            {
                return NotFound();
            }

            return Ok(bank);
        }

        // PUT: api/banks/5
        [HttpPut("{broadcasterId:int}")]
        public async Task<IActionResult> PutBank([FromRoute] int broadcasterId, [FromQuery] int deposit, [FromQuery] bool showOutput, [FromBody] List<string> usernames)
        {
            List<BalanceResult> results = new List<BalanceResult>();

            await _context.LoadStoredProc("dbo.UpdateCreateBalance")
                .WithSqlParam("tvpUsernames", usernames.ToDataTable())
                .WithSqlParam("intDeposit", deposit)
                .WithSqlParam("intBroadcasterID", broadcasterId)
                .WithSqlParam("bitShowOutput", showOutput)
                .ExecuteStoredProcAsync((handler) => 
                {
                    results = handler.ReadToList<BalanceResult>().ToList();
                });

            return Ok(results);
        }

        // POST: api/banks
        [HttpPost]
        public async Task<IActionResult> PostBank([FromBody] Bank bank)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Bank.Add(bank);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetBank", new { id = bank.Id }, bank);
        }

        //// DELETE: api/banks/5
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeleteBank([FromRoute] int id)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    var bank = await _context.Bank.SingleOrDefaultAsync(m => m.Id == id);
        //    if (bank == null)
        //    {
        //        return NotFound();
        //    }

        //    _context.Bank.Remove(bank);
        //    await _context.SaveChangesAsync();

        //    return Ok(bank);
        //}

        private bool BankExists(int id)
        {
            return _context.Bank.Any(e => e.Id == id);
        }
    }
}