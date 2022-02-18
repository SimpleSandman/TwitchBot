using System;
using System.Linq;
using System.Threading.Tasks;

using TwitchBotDb.Services;

using TwitchBotShared.ClientLibraries;
using TwitchBotShared.ClientLibraries.Singletons;
using TwitchBotShared.Config;
using TwitchBotShared.Enums;
using TwitchBotShared.Models;
using TwitchBotShared.Models.JSON;
using TwitchBotShared.Threads;

namespace TwitchBotShared.Commands.Features
{
    /// <summary>
    /// The "Command Subsystem" for the "Mini Games" feature
    /// </summary>
    public sealed class MinigameFeature : BaseFeature
    {
        private readonly BankService _bank;
        private readonly FollowerService _follower;
        private readonly TwitchInfoService _twitchInfo;
        
        private readonly BankHeistSingleton _heistSettingsInstance = BankHeistSingleton.Instance;
        private readonly BossFightSingleton _bossSettingsInstance = BossFightSingleton.Instance;
        private readonly BroadcasterSingleton _broadcasterInstance = BroadcasterSingleton.Instance;
        private readonly TwitchChatterList _twitchChatterListInstance = TwitchChatterList.Instance;
        private readonly RouletteSingleton _rouletteSingleton = RouletteSingleton.Instance;
        private readonly ErrorHandler _errHndlrInstance = ErrorHandler.Instance;

        public MinigameFeature(IrcClient irc, TwitchBotConfigurationSection botConfig, BankService bank, FollowerService follower, 
            TwitchInfoService twitchInfo) : base(irc, botConfig)
        {
            _bank = bank;
            _follower = follower;
            _twitchInfo = twitchInfo;
            _rolePermissions.Add("!roulette", new CommandPermission { General = ChatterType.Viewer });
            _rolePermissions.Add("!bankheist", new CommandPermission { General = ChatterType.Viewer });
            _rolePermissions.Add("!heist", new CommandPermission { General = ChatterType.Viewer });
            _rolePermissions.Add("!raid", new CommandPermission { General = ChatterType.Viewer });
            _rolePermissions.Add("!bossfight", new CommandPermission { General = ChatterType.Viewer });
        }

        public override async Task<(bool, DateTime)> ExecCommandAsync(TwitchChatter chatter, string requestedCommand)
        {
            try
            {
                switch (requestedCommand)
                {
                    case "!roulette":
                        return (true, await RussianRoulette(chatter));
                    case "!bankheist":
                    case "!heist":
                        return (true, await BankHeist(chatter));
                    case "!raid":
                    case "!bossfight":
                        return (true, await BossFight(chatter));
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "MinigameFeature", "ExecCommand(TwitchChatter, string)", false, requestedCommand, chatter.Message);
            }

            return (false, DateTime.Now);
        }

        /// <summary>
        /// Play a friendly game of Russian Roulette and risk chat privileges for stream currency
        /// </summary>
        /// <param name="chatter">User that sent the message</param>
        public async Task<DateTime> RussianRoulette(TwitchChatter chatter)
        {
            try
            {
                RouletteUser rouletteUser = _rouletteSingleton.RouletteUsers.FirstOrDefault(u => u.Username == chatter.Username);

                Random rnd = new Random(DateTime.Now.Millisecond);
                int bullet = rnd.Next(6); // between 0 and 5

                if (bullet == 0) // user was shot
                {
                    if (rouletteUser != null)
                    {
                        _rouletteSingleton.RouletteUsers.Remove(rouletteUser);
                    }

                    if (_botConfig.Broadcaster.ToLower() == chatter.Username || chatter.Badges.Contains("moderator"))
                    {
                        _irc.SendPublicChatMessage($"Enjoy your 15 minutes without russian roulette @{chatter.DisplayName}");
                        return DateTime.Now.AddMinutes(15);
                    }

                    _irc.SendChatTimeout(chatter.Username, 300); // 5 minute timeout
                    _irc.SendPublicChatMessage($"You are dead @{chatter.DisplayName}. Enjoy your 5 minutes in limbo (cannot talk)");
                    return DateTime.Now.AddMinutes(5);
                }

                if (rouletteUser == null) // new roulette user
                {
                    rouletteUser = new RouletteUser() { Username = chatter.Username, ShotsTaken = 1 };
                    _rouletteSingleton.RouletteUsers.Add(rouletteUser);

                    _irc.SendPublicChatMessage($"@{chatter.DisplayName} -> 1/6 attempts...good luck Kappa");
                }
                else // existing roulette user
                {
                    if (rouletteUser.ShotsTaken < 6)
                    {
                        foreach (RouletteUser user in _rouletteSingleton.RouletteUsers)
                        {
                            if (user.Username == chatter.Username)
                            {
                                user.ShotsTaken++;
                                break;
                            }
                        }
                    }

                    if (rouletteUser.ShotsTaken == 6)
                    {
                        int funds = await _bank.CheckBalanceAsync(chatter.Username, _broadcasterInstance.DatabaseId);
                        int reward = 3000; // ToDo: Make roulette reward deposit config setting

                        if (funds > -1)
                        {
                            funds += reward; // deposit 500 stream currency
                            await _bank.UpdateFundsAsync(chatter.Username, _broadcasterInstance.DatabaseId, funds);
                        }
                        else
                        {
                            await _bank.CreateAccountAsync(chatter.Username, _broadcasterInstance.DatabaseId, reward);
                        }

                        _rouletteSingleton.RouletteUsers.RemoveAll(u => u.Username == chatter.Username);

                        _irc.SendPublicChatMessage($"Congrats on surviving russian roulette. Here's {reward} {_botConfig.CurrencyType}!");
                        return DateTime.Now.AddMinutes(5);
                    }
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "MinigameFeature", "RussianRoulette(TwitchChatter)", false, "!roulette");
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Engage in the bank heist minigame
        /// </summary>
        /// <param name="chatter">User that sent the message</param>
        public async Task<DateTime> BankHeist(TwitchChatter chatter)
        {
            try
            {
                BankHeist bankHeist = new BankHeist();
                int funds = await _bank.CheckBalanceAsync(chatter.Username, _broadcasterInstance.DatabaseId);
                int gambleIndex = chatter.Message.IndexOf(" ");
                bool isValid = true;
                int gamble = 0;

                if (gambleIndex != -1)
                {
                    string parseGamble = chatter.Message.Substring(gambleIndex);
                    isValid = int.TryParse(parseGamble, out gamble);
                }

                if (_heistSettingsInstance.IsHeistOnCooldown())
                {
                    TimeSpan cooldown = _heistSettingsInstance.CooldownTimePeriod.Subtract(DateTime.Now);

                    if (cooldown.Minutes >= 1)
                    {
                        _irc.SendPublicChatMessage(_heistSettingsInstance.CooldownEntry
                            .Replace("@timeleft@", cooldown.Minutes.ToString()));
                    }
                    else
                    {
                        _irc.SendPublicChatMessage(_heistSettingsInstance.CooldownEntry
                            .Replace("@timeleft@", cooldown.Seconds.ToString())
                            .Replace("minutes", "seconds"));
                    }

                    return DateTime.Now;
                }

                if (bankHeist.HasRobberAlreadyEntered(chatter.Username))
                {
                    _irc.SendPublicChatMessage($"You are already in this heist @{chatter.DisplayName}");
                    return DateTime.Now;
                }

                // check if funds and gambling amount are valid
                if (!isValid)
                {
                    _irc.SendPublicChatMessage($"Please gamble with a positive amount of {_botConfig.CurrencyType} @{chatter.DisplayName}");
                    return DateTime.Now;
                }
                else if (funds > 0 && gamble == 0 && (chatter.Message.ToLower() == "!bankheist" || chatter.Message.ToLower() == "!heist"))
                {
                    // make sure the user can gamble something if an amount wasn't specified
                    if (funds > _heistSettingsInstance.MaxGamble)
                    {
                        gamble = _heistSettingsInstance.MaxGamble;
                    }
                    else
                    {
                        gamble = funds;
                    }
                }
                else if (gamble < 1)
                {
                    _irc.SendPublicChatMessage($"You cannot gamble less than one {_botConfig.CurrencyType} @{chatter.DisplayName}");
                    return DateTime.Now;
                }
                else if (funds < 1)
                {
                    _irc.SendPublicChatMessage($"You need at least one {_botConfig.CurrencyType} to join the heist @{chatter.DisplayName}");
                    return DateTime.Now;
                }
                else if (funds < gamble)
                {
                    _irc.SendPublicChatMessage($"You do not have enough to gamble {gamble} {_botConfig.CurrencyType} @{chatter.DisplayName}");
                    return DateTime.Now;
                }
                else if (gamble > _heistSettingsInstance.MaxGamble)
                {
                    _irc.SendPublicChatMessage($"{_heistSettingsInstance.MaxGamble} {_botConfig.CurrencyType} is the most you can put in. " +
                        $"Please try again with less {_botConfig.CurrencyType} @{chatter.DisplayName}");
                    return DateTime.Now;
                }

                if (!bankHeist.IsEntryPeriodOver())
                {
                    // make heist announcement if first robber and start recruiting members
                    if (_heistSettingsInstance.Robbers.Count == 0)
                    {
                        _heistSettingsInstance.EntryPeriod = DateTime.Now.AddSeconds(_heistSettingsInstance.EntryPeriodSeconds);
                        _irc.SendPublicChatMessage(_heistSettingsInstance.EntryMessage.Replace("user@", chatter.Username));
                    }

                    // join bank heist
                    BankRobber robber = new BankRobber { Username = chatter.Username, Gamble = gamble };
                    bankHeist.Produce(robber);
                    await _bank.UpdateFundsAsync(chatter.Username, _broadcasterInstance.DatabaseId, funds - gamble);

                    // display new heist level
                    if (!string.IsNullOrEmpty(bankHeist.NextLevelMessage()))
                    {
                        _irc.SendPublicChatMessage(bankHeist.NextLevelMessage());
                    }

                    // display if more than one robber joins
                    if (_heistSettingsInstance.Robbers.Count > 1)
                    {
                        _irc.SendPublicChatMessage($"@{chatter.DisplayName} has joined the heist");
                    }
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "MinigameFeature", "BankHeist(TwitchChatter)", false, "!bankheist", chatter.Message);
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Engage in the boss fight minigame
        /// </summary>
        /// <param name="chatter">User that sent the message</param>
        public async Task<DateTime> BossFight(TwitchChatter chatter)
        {
            try
            {
                BossFight bossFight = new BossFight();
                int funds = await _bank.CheckBalanceAsync(chatter.Username, _broadcasterInstance.DatabaseId);

                if (_bossSettingsInstance.IsBossFightOnCooldown())
                {
                    TimeSpan cooldown = _bossSettingsInstance.CooldownTimePeriod.Subtract(DateTime.Now);

                    if (cooldown.Minutes >= 1)
                    {
                        _irc.SendPublicChatMessage(_bossSettingsInstance.CooldownEntry
                            .Replace("@timeleft@", cooldown.Minutes.ToString()));
                    }
                    else
                    {
                        _irc.SendPublicChatMessage(_bossSettingsInstance.CooldownEntry
                            .Replace("@timeleft@", cooldown.Seconds.ToString())
                            .Replace("minutes", "seconds"));
                    }

                    return DateTime.Now;
                }

                if (_bossSettingsInstance.RefreshBossFight)
                {
                    _irc.SendPublicChatMessage($"The boss fight is currently being refreshed with new settings @{chatter.DisplayName}");
                    return DateTime.Now;
                }

                if (bossFight.HasFighterAlreadyEntered(chatter.Username))
                {
                    _irc.SendPublicChatMessage($"You are already in this fight @{chatter.DisplayName}");
                    return DateTime.Now;
                }

                if (funds < _bossSettingsInstance.Cost)
                {
                    _irc.SendPublicChatMessage($"You do need {_bossSettingsInstance.Cost} {_botConfig.CurrencyType} to enter this fight @{chatter.DisplayName}");
                    return DateTime.Now;
                }

                if (!bossFight.IsEntryPeriodOver())
                {
                    ChatterType chatterType = ChatterType.DoesNotExist;

                    // join boss fight
                    if (chatter.Badges.Contains("moderator")
                        || chatter.Badges.Contains("admin")
                        || chatter.Badges.Contains("global_mod")
                        || chatter.Badges.Contains("staff")
                        || chatter.Username == _botConfig.Broadcaster.ToLower())
                    {
                        chatterType = ChatterType.Moderator;
                    }
                    else if (chatter.Badges.Contains("subscriber") || chatter.Badges.Contains("vip"))
                    {
                        chatterType = ChatterType.Subscriber;
                    }
                    // ToDo: Create new columns in the BossFightClassStats table for VIP stats
                    //else if (chatter.Badges.Contains("vip"))
                    //{
                    //    chatterType = ChatterType.VIP;
                    //}
                    else
                    {
                        chatterType = _twitchChatterListInstance.GetUserChatterType(chatter.Username);
                        if (chatterType == ChatterType.DoesNotExist)
                        {
                            FollowerJSON response = await _twitchInfo.CheckFollowerStatusAsync(chatter.TwitchId);

                            // check if chatter is a follower
                            if (response != null)
                            {
                                int currentExp = await _follower.CurrentExpAsync(chatter.Username, _broadcasterInstance.DatabaseId);
                                if (_follower.IsRegularFollower(currentExp, _botConfig.RegularFollowerHours))
                                {
                                    chatterType = ChatterType.RegularFollower;
                                }
                                else
                                {
                                    chatterType = ChatterType.Follower;
                                }
                            }
                            else
                            {
                                chatterType = ChatterType.Viewer;
                            }
                        }
                    }

                    // make boss fight announcement if first fighter and start recruiting members
                    if (_bossSettingsInstance.Fighters.Count == 0)
                    {
                        _bossSettingsInstance.EntryPeriod = DateTime.Now.AddSeconds(_bossSettingsInstance.EntryPeriodSeconds);
                        _irc.SendPublicChatMessage(_bossSettingsInstance.EntryMessage.Replace("user@", chatter.Username));
                    }

                    FighterClass fighterClass = _bossSettingsInstance.ClassStats.Single(c => c.ChatterType == chatterType);
                    BossFighter fighter = new BossFighter { Username = chatter.Username, FighterClass = fighterClass };
                    bossFight.Produce(fighter);
                    await _bank.UpdateFundsAsync(chatter.Username, _broadcasterInstance.DatabaseId, funds - _bossSettingsInstance.Cost);

                    // display new boss level
                    if (!string.IsNullOrEmpty(bossFight.NextLevelMessage()))
                    {
                        _irc.SendPublicChatMessage(bossFight.NextLevelMessage());
                    }
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "MinigameFeature", "BossFight(TwitchChatter)", false, "!raid");
            }

            return DateTime.Now;
        }
    }
}
