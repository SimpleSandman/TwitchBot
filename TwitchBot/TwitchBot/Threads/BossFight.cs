using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using TwitchBot.Configuration;
using TwitchBot.Libraries;
using TwitchBot.Models;
using TwitchBot.Services;

namespace TwitchBot.Threads
{
    public class BossFight
    {
        private IrcClient _irc;
        private string _connStr;
        private int _broadcasterId;
        private Thread _thread;
        private BankService _bank;
        private TwitchBotConfigurationSection _botConfig;
        private string _resultMessage;
        private BossFightSettings _bossSettings = BossFightSettings.Instance;

        public BossFight() { }

        public BossFight(string connStr, BankService bank, TwitchBotConfigurationSection botConfig)
        {
            _connStr = connStr;
            _thread = new Thread(new ThreadStart(this.Run));
            _bank = bank;
            _botConfig = botConfig;
        }

        public void Start(IrcClient irc, int broadcasterId)
        {
            _irc = irc;
            _broadcasterId = broadcasterId;
            _bossSettings.CooldownTimePeriod = DateTime.Now;
            _bossSettings.Fighters = new BlockingCollection<BossFighter>();
            _resultMessage = _bossSettings.ResultsMessage;

            _thread.IsBackground = true;
            _thread.Start();
        }

        private void Run()
        {
            while (true)
            {
                if (_bossSettings.IsBossFightOnCooldown())
                {
                    double cooldownTime = (_bossSettings.CooldownTimePeriod.Subtract(DateTime.Now)).TotalMilliseconds;
                    Thread.Sleep((int)cooldownTime);
                    _irc.SendPublicChatMessage(_bossSettings.CooldownOver);
                }
                else if (_bossSettings.Fighters.Count > 0 && _bossSettings.IsEntryPeriodOver())
                {
                    _bossSettings.Fighters.CompleteAdding();
                    Consume();

                    // refresh the list and reset the cooldown time period
                    _bossSettings.Fighters = new BlockingCollection<BossFighter>();
                    _bossSettings.CooldownTimePeriod = DateTime.Now.AddMinutes(_bossSettings.CooldownTimePeriodMinutes);
                    _resultMessage = _bossSettings.ResultsMessage;
                }

                Thread.Sleep(200);
            }
        }

        public void Produce(BossFighter fighter)
        {
            _bossSettings.Fighters.Add(fighter);
        }

        public void Consume()
        {
            BossFightLevel bossLevel = _bossSettings.Levels[BossLevel() - 1];
            BossFightPayout payout = _bossSettings.Payouts[BossLevel() - 1];

            _irc.SendPublicChatMessage(_bossSettings.GameStart
                .Replace("@bankname@", bossLevel.LevelBankName));

            Thread.Sleep(5000); // wait in anticipation

            Random rnd = new Random();
            int chance = rnd.Next(1, 101); // 1 - 100

            if (chance >= payout.SuccessRate) // failed
            {
                if (_bossSettings.Fighters.Count == 1)
                {
                    _irc.SendPublicChatMessage(_bossSettings.SingleUserFail
                        .Replace("user@", _bossSettings.Fighters.First().Username)
                        .Replace("@bankname@", bossLevel.LevelBankName));
                }
                else
                {
                    _irc.SendPublicChatMessage(_bossSettings.Success0);
                }

                return;
            }

            int numWinners = (int)Math.Ceiling(_bossSettings.Fighters.Count * (payout.SuccessRate / 100));
            IEnumerable<BossFighter> winners = _bossSettings.Fighters.OrderBy(x => rnd.Next()).Take(numWinners);

            foreach (BossFighter winner in winners)
            {
                int funds = _bank.CheckBalance(winner.Username.ToLower(), _broadcasterId);
                decimal earnings = Math.Ceiling(winner.Gamble * payout.WinMultiplier);

                _bank.UpdateFunds(winner.Username.ToLower(), _broadcasterId, (int)earnings + funds);

                _resultMessage += $" {winner.Username} ({(int)earnings} {_botConfig.CurrencyType}),";
            }

            // remove extra ","
            _resultMessage = _resultMessage.Remove(_resultMessage.LastIndexOf(','), 1);

            decimal numWinnersPercentage = numWinners / (decimal)_bossSettings.Fighters.Count;

            // display success outcome
            if (winners.Count() == 1)
            {
                BossFighter onlyWinner = winners.First();
                int earnings = (int)Math.Ceiling(onlyWinner.Gamble * payout.WinMultiplier);

                _irc.SendPublicChatMessage(_bossSettings.SingleUserSuccess
                    .Replace("user@", onlyWinner.Username)
                    .Replace("@bankname@", bossLevel.LevelBankName)
                    .Replace("@winamount@", earnings.ToString())
                    .Replace("@pointsname@", _botConfig.CurrencyType));
            }
            else if (numWinners == _bossSettings.Fighters.Count)
            {
                _irc.SendPublicChatMessage(_bossSettings.Success100 + " " + _resultMessage);
            }
            else if (numWinnersPercentage >= 0.34m)
            {
                _irc.SendPublicChatMessage(_bossSettings.Success34 + " " + _resultMessage);
            }
            else if (numWinnersPercentage > 0)
            {
                _irc.SendPublicChatMessage(_bossSettings.Success1 + " " + _resultMessage);
            }

            // show in case Twitch deletes the message because of exceeding character length
            Console.WriteLine("\n" + _resultMessage + "\n");
        }

        public bool HasFighterAlreadyEntered(string username)
        {
            return _bossSettings.Fighters.Any(u => u.Username == username) ? true : false;
        }

        public bool IsEntryPeriodOver()
        {
            return _bossSettings.Fighters.IsAddingCompleted ? true : false;
        }

        public int BossLevel()
        {
            if (_bossSettings.Fighters.Count <= _bossSettings.Levels[0].MaxUsers)
                return 1;
            else if (_bossSettings.Fighters.Count <= _bossSettings.Levels[1].MaxUsers)
                return 2;
            else if (_bossSettings.Fighters.Count <= _bossSettings.Levels[2].MaxUsers)
                return 3;
            else if (_bossSettings.Fighters.Count <= _bossSettings.Levels[3].MaxUsers)
                return 4;
            else
                return 5;
        }

        public string NextLevelMessage()
        {
            if (_bossSettings.Fighters.Count == _bossSettings.Levels[0].MaxUsers + 1)
                return _bossSettings.NextLevelMessages[0];
            else if (_bossSettings.Fighters.Count == _bossSettings.Levels[1].MaxUsers + 1)
                return _bossSettings.NextLevelMessages[1];
            else if (_bossSettings.Fighters.Count == _bossSettings.Levels[2].MaxUsers + 1)
                return _bossSettings.NextLevelMessages[2];
            else if (_bossSettings.Fighters.Count == _bossSettings.Levels[3].MaxUsers + 1)
                return _bossSettings.NextLevelMessages[3];

            return "";
        }
    }
}
