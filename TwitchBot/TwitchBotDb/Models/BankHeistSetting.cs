namespace TwitchBotDb.Models
{
    public class BankHeistSetting
    {
        public int Id { get; set; }
        public int BroadcasterId { get; set; }
        public int CooldownPeriodMin { get; set; }
        public int EntryPeriodSec { get; set; }
        public string EntryMessage { get; set; }
        public int MaxGamble { get; set; }
        public string MaxGambleText { get; set; }
        public string EntryInstructions { get; set; }
        public string CooldownEntry { get; set; }
        public string CooldownOver { get; set; }
        public string NextLevelMessage2 { get; set; }
        public string NextLevelMessage3 { get; set; }
        public string NextLevelMessage4 { get; set; }
        public string NextLevelMessage5 { get; set; }
        public string GameStart { get; set; }
        public string SingleUserSuccess { get; set; }
        public string SingleUserFail { get; set; }
        public string ResultsMessage { get; set; }
        public string Success100 { get; set; }
        public string Success34 { get; set; }
        public string Success1 { get; set; }
        public string Success0 { get; set; }
        public string LevelName1 { get; set; }
        public int LevelMaxUsers1 { get; set; }
        public string LevelName2 { get; set; }
        public int LevelMaxUsers2 { get; set; }
        public string LevelName3 { get; set; }
        public int LevelMaxUsers3 { get; set; }
        public string LevelName4 { get; set; }
        public int LevelMaxUsers4 { get; set; }
        public string LevelName5 { get; set; }
        public int LevelMaxUsers5 { get; set; }
        public decimal PayoutSuccessRate1 { get; set; }
        public decimal PayoutMultiplier1 { get; set; }
        public decimal PayoutSuccessRate2 { get; set; }
        public decimal PayoutMultiplier2 { get; set; }
        public decimal PayoutSuccessRate3 { get; set; }
        public decimal PayoutMultiplier3 { get; set; }
        public decimal PayoutSuccessRate4 { get; set; }
        public decimal PayoutMultiplier4 { get; set; }
        public decimal PayoutSuccessRate5 { get; set; }
        public decimal PayoutMultiplier5 { get; set; }

        public virtual Broadcaster Broadcaster { get; set; }
    }
}
