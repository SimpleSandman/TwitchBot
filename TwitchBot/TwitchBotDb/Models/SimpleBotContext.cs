using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace TwitchBotDb.Models
{
    public partial class SimpleBotContext : DbContext
    {
        public SimpleBotContext()
        {
        }

        public SimpleBotContext(DbContextOptions<SimpleBotContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Bank> Bank { get; set; }
        public virtual DbSet<BankHeistSetting> BankHeistSetting { get; set; }
        public virtual DbSet<BossFightBossStats> BossFightBossStats { get; set; }
        public virtual DbSet<BossFightClassStats> BossFightClassStats { get; set; }
        public virtual DbSet<BossFightSetting> BossFightSetting { get; set; }
        public virtual DbSet<BotModerator> BotModerator { get; set; }
        public virtual DbSet<BotTimeout> BotTimeout { get; set; }
        public virtual DbSet<Broadcaster> Broadcaster { get; set; }
        public virtual DbSet<CustomCommand> CustomCommand { get; set; }
        public virtual DbSet<ErrorLog> ErrorLog { get; set; }
        public virtual DbSet<InGameUsername> InGameUsername { get; set; }
        public virtual DbSet<PartyUp> PartyUp { get; set; }
        public virtual DbSet<PartyUpRequest> PartyUpRequest { get; set; }
        public virtual DbSet<Quote> Quote { get; set; }
        public virtual DbSet<Rank> Rank { get; set; }
        public virtual DbSet<RankFollower> RankFollower { get; set; }
        public virtual DbSet<Reminder> Reminder { get; set; }
        public virtual DbSet<SongRequest> SongRequest { get; set; }
        public virtual DbSet<SongRequestIgnore> SongRequestIgnore { get; set; }
        public virtual DbSet<SongRequestSetting> SongRequestSetting { get; set; }
        public virtual DbSet<TwitchGameCategory> TwitchGameCategory { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.4-servicing-10062");

            modelBuilder.Entity<Bank>(entity =>
            {
                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.HasOne(d => d.BroadcasterNavigation)
                    .WithMany(p => p.Bank)
                    .HasForeignKey(d => d.Broadcaster)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Bank_Broadcaster");
            });

            modelBuilder.Entity<BankHeistSetting>(entity =>
            {
                entity.Property(e => e.CooldownEntry)
                    .IsRequired()
                    .HasMaxLength(150)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('The cops are on high alert after the last job, we have to lay low for a bit. Call me again after @timeleft@ minutes')");

                entity.Property(e => e.CooldownOver)
                    .IsRequired()
                    .HasMaxLength(150)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('Looks like the cops have given up the search … the banks are ripe for hitting!')");

                entity.Property(e => e.CooldownPeriodMin).HasDefaultValueSql("((10))");

                entity.Property(e => e.EntryInstructions)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('Type @command@ [x] to enter')");

                entity.Property(e => e.EntryMessage)
                    .IsRequired()
                    .HasMaxLength(150)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('@user@ has started planning a bank heist! Looking for a bigger crew for a bigger score. Join in!')");

                entity.Property(e => e.EntryPeriodSec).HasDefaultValueSql("((120))");

                entity.Property(e => e.GameStart)
                    .IsRequired()
                    .HasMaxLength(200)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('Alright guys, check your guns. We are storming into the @bankname@ through all entrances. Let''s get the cash and get out before the cops get here.')");

                entity.Property(e => e.LevelMaxUsers1).HasDefaultValueSql("((9))");

                entity.Property(e => e.LevelMaxUsers2).HasDefaultValueSql("((19))");

                entity.Property(e => e.LevelMaxUsers3).HasDefaultValueSql("((29))");

                entity.Property(e => e.LevelMaxUsers4).HasDefaultValueSql("((39))");

                entity.Property(e => e.LevelMaxUsers5).HasDefaultValueSql("((9001))");

                entity.Property(e => e.LevelName1)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('Simple Municipal Bank')");

                entity.Property(e => e.LevelName2)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('Simple City Bank')");

                entity.Property(e => e.LevelName3)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('Simple State Bank')");

                entity.Property(e => e.LevelName4)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('Simple National Reserve')");

                entity.Property(e => e.LevelName5)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('Simple Federal Reserve')");

                entity.Property(e => e.MaxGamble).HasDefaultValueSql("((5000))");

                entity.Property(e => e.MaxGambleText)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('Buy-in to''ps out at @maxbet@ @pointsname@')");

                entity.Property(e => e.NextLevelMessage2)
                    .IsRequired()
                    .HasMaxLength(200)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('With this crew, we can now hit the @bankname@. Lets see if we can get a bigger crew to hit the @nextbankname@!')");

                entity.Property(e => e.NextLevelMessage3)
                    .IsRequired()
                    .HasMaxLength(200)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('Oh yeah! With this crew, we can now hit the @bankname@. Lets see if we can get a bigger crew to hit the @nextbankname@!')");

                entity.Property(e => e.NextLevelMessage4)
                    .IsRequired()
                    .HasMaxLength(200)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('Hell yeah! We can now hit the @bankname@. A few more, and we could hit the @nextbankname@!')");

                entity.Property(e => e.NextLevelMessage5)
                    .IsRequired()
                    .HasMaxLength(200)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('Epic crew! We are going to hit the @bankname@ guys! Gear up and get ready to head out.')");

                entity.Property(e => e.PayoutMultiplier1)
                    .HasColumnType("decimal(3, 2)")
                    .HasDefaultValueSql("((1.50))");

                entity.Property(e => e.PayoutMultiplier2)
                    .HasColumnType("decimal(3, 2)")
                    .HasDefaultValueSql("((1.70))");

                entity.Property(e => e.PayoutMultiplier3)
                    .HasColumnType("decimal(3, 2)")
                    .HasDefaultValueSql("((2.00))");

                entity.Property(e => e.PayoutMultiplier4)
                    .HasColumnType("decimal(3, 2)")
                    .HasDefaultValueSql("((2.25))");

                entity.Property(e => e.PayoutMultiplier5)
                    .HasColumnType("decimal(3, 2)")
                    .HasDefaultValueSql("((2.75))");

                entity.Property(e => e.PayoutSuccessRate1)
                    .HasColumnType("decimal(5, 2)")
                    .HasDefaultValueSql("((54.00))");

                entity.Property(e => e.PayoutSuccessRate2)
                    .HasColumnType("decimal(5, 2)")
                    .HasDefaultValueSql("((48.80))");

                entity.Property(e => e.PayoutSuccessRate3)
                    .HasColumnType("decimal(5, 2)")
                    .HasDefaultValueSql("((42.50))");

                entity.Property(e => e.PayoutSuccessRate4)
                    .HasColumnType("decimal(5, 2)")
                    .HasDefaultValueSql("((38.70))");

                entity.Property(e => e.PayoutSuccessRate5)
                    .HasColumnType("decimal(5, 2)")
                    .HasDefaultValueSql("((32.40))");

                entity.Property(e => e.ResultsMessage)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('The heist payouts are:')");

                entity.Property(e => e.SingleUserFail)
                    .IsRequired()
                    .HasMaxLength(200)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('Local security caught @user@ trying to sneak into the @bankname@ through the back entrance and opened fire.')");

                entity.Property(e => e.SingleUserSuccess)
                    .IsRequired()
                    .HasMaxLength(200)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('@user@ executed the heist flawlessly, sneaking into the @bankname@ through the back entrance and looting @winamount@ @pointsname@ from the vault.')");

                entity.Property(e => e.Success0)
                    .IsRequired()
                    .HasMaxLength(200)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('SWAT teams nearby stormed the bank and killed the entire crew. Not a single soul survived…')");

                entity.Property(e => e.Success1)
                    .IsRequired()
                    .HasMaxLength(200)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('The crew suffered major losses as they engaged the SWAT backup.')");

                entity.Property(e => e.Success100)
                    .IsRequired()
                    .HasMaxLength(200)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('The execution was flawless, in and out before the first cop arrived on scene.')");

                entity.Property(e => e.Success34)
                    .IsRequired()
                    .HasMaxLength(200)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('The crew suffered a few losses engaging the local security team.')");

                entity.HasOne(d => d.Broadcaster)
                    .WithMany(p => p.BankHeistSetting)
                    .HasForeignKey(d => d.BroadcasterId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_BankHeistSetting_Broadcaster");
            });

            modelBuilder.Entity<BossFightBossStats>(entity =>
            {
                entity.Property(e => e.Attack1).HasDefaultValueSql("((15))");

                entity.Property(e => e.Attack2).HasDefaultValueSql("((25))");

                entity.Property(e => e.Attack3).HasDefaultValueSql("((35))");

                entity.Property(e => e.Attack4).HasDefaultValueSql("((40))");

                entity.Property(e => e.Attack5).HasDefaultValueSql("((50))");

                entity.Property(e => e.Defense2).HasDefaultValueSql("((10))");

                entity.Property(e => e.Defense3).HasDefaultValueSql("((20))");

                entity.Property(e => e.Defense4).HasDefaultValueSql("((25))");

                entity.Property(e => e.Defense5).HasDefaultValueSql("((30))");

                entity.Property(e => e.Evasion1).HasDefaultValueSql("((5))");

                entity.Property(e => e.Evasion2).HasDefaultValueSql("((15))");

                entity.Property(e => e.Evasion3).HasDefaultValueSql("((20))");

                entity.Property(e => e.Evasion4).HasDefaultValueSql("((25))");

                entity.Property(e => e.Evasion5).HasDefaultValueSql("((35))");

                entity.Property(e => e.Health1).HasDefaultValueSql("((200))");

                entity.Property(e => e.Health2).HasDefaultValueSql("((750))");

                entity.Property(e => e.Health3).HasDefaultValueSql("((1500))");

                entity.Property(e => e.Health4).HasDefaultValueSql("((3000))");

                entity.Property(e => e.Health5).HasDefaultValueSql("((5000))");

                entity.Property(e => e.LastAttackBonus1).HasDefaultValueSql("((150))");

                entity.Property(e => e.LastAttackBonus2).HasDefaultValueSql("((300))");

                entity.Property(e => e.LastAttackBonus3).HasDefaultValueSql("((600))");

                entity.Property(e => e.LastAttackBonus4).HasDefaultValueSql("((1000))");

                entity.Property(e => e.LastAttackBonus5).HasDefaultValueSql("((2500))");

                entity.Property(e => e.Loot1).HasDefaultValueSql("((300))");

                entity.Property(e => e.Loot2).HasDefaultValueSql("((750))");

                entity.Property(e => e.Loot3).HasDefaultValueSql("((2000))");

                entity.Property(e => e.Loot4).HasDefaultValueSql("((5000))");

                entity.Property(e => e.Loot5).HasDefaultValueSql("((10000))");

                entity.Property(e => e.MaxUsers1).HasDefaultValueSql("((9))");

                entity.Property(e => e.MaxUsers2).HasDefaultValueSql("((19))");

                entity.Property(e => e.MaxUsers3).HasDefaultValueSql("((29))");

                entity.Property(e => e.MaxUsers4).HasDefaultValueSql("((39))");

                entity.Property(e => e.MaxUsers5).HasDefaultValueSql("((49))");

                entity.Property(e => e.Name1)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('Boss 1')");

                entity.Property(e => e.Name2)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('Boss 2')");

                entity.Property(e => e.Name3)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('Boss 3')");

                entity.Property(e => e.Name4)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('Boss 4')");

                entity.Property(e => e.Name5)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('Boss 5')");

                entity.Property(e => e.TurnLimit1).HasDefaultValueSql("((20))");

                entity.Property(e => e.TurnLimit2).HasDefaultValueSql("((20))");

                entity.Property(e => e.TurnLimit3).HasDefaultValueSql("((20))");

                entity.Property(e => e.TurnLimit4).HasDefaultValueSql("((20))");

                entity.Property(e => e.TurnLimit5).HasDefaultValueSql("((20))");

                entity.HasOne(d => d.Game)
                    .WithMany(p => p.BossFightBossStats)
                    .HasForeignKey(d => d.GameId)
                    .HasConstraintName("FK_BossFightBossStats_TwitchGameCategory");

                entity.HasOne(d => d.Settings)
                    .WithMany(p => p.BossFightBossStats)
                    .HasForeignKey(d => d.SettingsId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_BossFightBossStats_Broadcaster");
            });

            modelBuilder.Entity<BossFightClassStats>(entity =>
            {
                entity.Property(e => e.FollowerAttack).HasDefaultValueSql("((15))");

                entity.Property(e => e.FollowerDefense).HasDefaultValueSql("((7))");

                entity.Property(e => e.FollowerEvasion).HasDefaultValueSql("((15))");

                entity.Property(e => e.FollowerHealth).HasDefaultValueSql("((125))");

                entity.Property(e => e.ModeratorAttack).HasDefaultValueSql("((35))");

                entity.Property(e => e.ModeratorDefense).HasDefaultValueSql("((20))");

                entity.Property(e => e.ModeratorEvasion).HasDefaultValueSql("((35))");

                entity.Property(e => e.ModeratorHealth).HasDefaultValueSql("((225))");

                entity.Property(e => e.RegularAttack).HasDefaultValueSql("((20))");

                entity.Property(e => e.RegularDefense).HasDefaultValueSql("((10))");

                entity.Property(e => e.RegularEvasion).HasDefaultValueSql("((20))");

                entity.Property(e => e.RegularHealth).HasDefaultValueSql("((175))");

                entity.Property(e => e.SubscriberAttack).HasDefaultValueSql("((25))");

                entity.Property(e => e.SubscriberDefense).HasDefaultValueSql("((15))");

                entity.Property(e => e.SubscriberEvasion).HasDefaultValueSql("((25))");

                entity.Property(e => e.SubscriberHealth).HasDefaultValueSql("((200))");

                entity.Property(e => e.ViewerAttack).HasDefaultValueSql("((10))");

                entity.Property(e => e.ViewerDefense).HasDefaultValueSql("((5))");

                entity.Property(e => e.ViewerEvasion).HasDefaultValueSql("((10))");

                entity.Property(e => e.ViewerHealth).HasDefaultValueSql("((100))");

                entity.HasOne(d => d.Settings)
                    .WithMany(p => p.BossFightClassStats)
                    .HasForeignKey(d => d.SettingsId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_BossFightClassStats_Broadcaster");
            });

            modelBuilder.Entity<BossFightSetting>(entity =>
            {
                entity.Property(e => e.CooldownEntry)
                    .IsRequired()
                    .HasMaxLength(150)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('The boss floor is currently being cleaned up and won''t be available for at least @timeleft@ minutes')");

                entity.Property(e => e.CooldownOver)
                    .IsRequired()
                    .HasMaxLength(150)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('The boss floor has been cleaned up... Want to go again?! Type !raid to start!')");

                entity.Property(e => e.CooldownPeriodMin).HasDefaultValueSql("((10))");

                entity.Property(e => e.Cost).HasDefaultValueSql("((100))");

                entity.Property(e => e.EntryMessage)
                    .IsRequired()
                    .HasMaxLength(150)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('@user@ is trying to get a group of adventurers ready to fight a boss... Will you join them? Type !raid to join!')");

                entity.Property(e => e.EntryPeriodSec).HasDefaultValueSql("((60))");

                entity.Property(e => e.GameStart)
                    .IsRequired()
                    .HasMaxLength(200)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('The combatants have stepped into the boss room... Will they be able to defeat @bossname@?!')");

                entity.Property(e => e.NextLevelMessage2)
                    .IsRequired()
                    .HasMaxLength(200)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('With this raid party, we can now attack @bossname@. Let''s see if we can get a bigger party to attack @nextbossname@!')");

                entity.Property(e => e.NextLevelMessage3)
                    .IsRequired()
                    .HasMaxLength(200)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('Oh yeah! With this raid party, we can now attack @bossname@. Let''s see if we can get a bigger party to attack @nextbossname@!')");

                entity.Property(e => e.NextLevelMessage4)
                    .IsRequired()
                    .HasMaxLength(200)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('Hell yeah! We can now attack @bossname@. A few more and we could attack @nextbossname@!')");

                entity.Property(e => e.NextLevelMessage5)
                    .IsRequired()
                    .HasMaxLength(200)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('Epic raid party! We are going to attack @bossname@ guys! Gear up and get ready to head out.')");

                entity.Property(e => e.ResultsMessage)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('The survivors are:')");

                entity.Property(e => e.SingleUserFail)
                    .IsRequired()
                    .HasMaxLength(200)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('@user@ thought they could try to solo @bossname@, but was deleted immediately...RIP')");

                entity.Property(e => e.SingleUserSuccess)
                    .IsRequired()
                    .HasMaxLength(200)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('@user@ executed the raid flawlessly, soloing @bossname@ with ease and looting @winamount@ @pointsname@ with a last attack bonus of @lastattackbonus@ @pointsname@.')");

                entity.Property(e => e.Success0)
                    .IsRequired()
                    .HasMaxLength(200)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('It was absolute hell. The field is covered with the blood of the fallen...no one survived')");

                entity.Property(e => e.Success1)
                    .IsRequired()
                    .HasMaxLength(200)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('The raid party suffered major casualties as they fought valiantly.')");

                entity.Property(e => e.Success100)
                    .IsRequired()
                    .HasMaxLength(200)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('The raid was a complete success. No one fell into the hands of @bossname@.')");

                entity.Property(e => e.Success34)
                    .IsRequired()
                    .HasMaxLength(200)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('The raid party suffered a few casualties as they fought valiantly.')");

                entity.HasOne(d => d.Broadcaster)
                    .WithMany(p => p.BossFightSetting)
                    .HasForeignKey(d => d.BroadcasterId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_BossFightSetting_Broadcaster");
            });

            modelBuilder.Entity<BotModerator>(entity =>
            {
                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.HasOne(d => d.Broadcaster)
                    .WithMany(p => p.BotModerator)
                    .HasForeignKey(d => d.BroadcasterId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_BotModerator_Broadcaster");
            });

            modelBuilder.Entity<BotTimeout>(entity =>
            {
                entity.Property(e => e.TimeAdded)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Timeout).HasColumnType("datetime");

                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.HasOne(d => d.Broadcaster)
                    .WithMany(p => p.BotTimeout)
                    .HasForeignKey(d => d.BroadcasterId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_BotTimeout_Broadcaster");
            });

            modelBuilder.Entity<Broadcaster>(entity =>
            {
                entity.Property(e => e.LastUpdated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CustomCommand>(entity =>
            {
                entity.ToTable("CustomCommand", "Cmd");

                entity.Property(e => e.Message)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(30);

                entity.Property(e => e.Whitelist)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.HasOne(d => d.Broadcaster)
                    .WithMany(p => p.CustomCommand)
                    .HasForeignKey(d => d.BroadcasterId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Cmd_CustomCommand_Broadcaster");
            });

            modelBuilder.Entity<ErrorLog>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Command)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ErrorClass)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.ErrorMethod)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.ErrorMsg)
                    .IsRequired()
                    .HasMaxLength(4000)
                    .IsUnicode(false);

                entity.Property(e => e.ErrorTime).HasColumnType("datetime");

                entity.Property(e => e.UserMsg)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.HasOne(d => d.Broadcaster)
                    .WithMany(p => p.ErrorLog)
                    .HasForeignKey(d => d.BroadcasterId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ErrorLog_Broadcaster");
            });

            modelBuilder.Entity<InGameUsername>(entity =>
            {
                entity.Property(e => e.Message)
                    .IsRequired()
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.HasOne(d => d.Broadcaster)
                    .WithMany(p => p.InGameUsername)
                    .HasForeignKey(d => d.BroadcasterId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__InGameUsername__Broadcaster");

                entity.HasOne(d => d.Game)
                    .WithMany(p => p.InGameUsername)
                    .HasForeignKey(d => d.GameId)
                    .HasConstraintName("FK__InGameUsername__GameId");
            });

            modelBuilder.Entity<PartyUp>(entity =>
            {
                entity.Property(e => e.PartyMemberName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.HasOne(d => d.Broadcaster)
                    .WithMany(p => p.PartyUp)
                    .HasForeignKey(d => d.BroadcasterId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PartyUp_Broadcaster");

                entity.HasOne(d => d.Game)
                    .WithMany(p => p.PartyUp)
                    .HasForeignKey(d => d.GameId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PartyUp_TwitchGameCategory");
            });

            modelBuilder.Entity<PartyUpRequest>(entity =>
            {
                entity.Property(e => e.TimeRequested)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.HasOne(d => d.PartyMember)
                    .WithMany(p => p.PartyUpRequest)
                    .HasForeignKey(d => d.PartyMemberId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PartyUpRequest_PartyUp");
            });

            modelBuilder.Entity<Quote>(entity =>
            {
                entity.Property(e => e.TimeCreated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.UserQuote)
                    .IsRequired()
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.Broadcaster)
                    .WithMany(p => p.Quote)
                    .HasForeignKey(d => d.BroadcasterId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Quote_Broadcasters");
            });

            modelBuilder.Entity<Rank>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.Broadcaster)
                    .WithMany(p => p.Rank)
                    .HasForeignKey(d => d.BroadcasterId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Rank_Broadcaster");
            });

            modelBuilder.Entity<RankFollower>(entity =>
            {
                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.HasOne(d => d.Broadcaster)
                    .WithMany(p => p.RankFollower)
                    .HasForeignKey(d => d.BroadcasterId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_RankFollower_Broadcaster");
            });

            modelBuilder.Entity<Reminder>(entity =>
            {
                entity.Property(e => e.ExpirationDateUtc).HasColumnType("datetime");

                entity.Property(e => e.Message)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.HasOne(d => d.Broadcaster)
                    .WithMany(p => p.Reminder)
                    .HasForeignKey(d => d.BroadcasterId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Reminder_Broadcaster");

                entity.HasOne(d => d.Game)
                    .WithMany(p => p.Reminder)
                    .HasForeignKey(d => d.GameId)
                    .HasConstraintName("FK_Reminder_TwitchGameCategory");
            });

            modelBuilder.Entity<SongRequest>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Username)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.HasOne(d => d.Broadcaster)
                    .WithMany(p => p.SongRequest)
                    .HasForeignKey(d => d.BroadcasterId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SongRequest_Broadcaster");
            });

            modelBuilder.Entity<SongRequestIgnore>(entity =>
            {
                entity.Property(e => e.Artist)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Title)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.HasOne(d => d.Broadcaster)
                    .WithMany(p => p.SongRequestIgnore)
                    .HasForeignKey(d => d.BroadcasterId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SongRequestIgnore_Broadcaster");
            });

            modelBuilder.Entity<SongRequestSetting>(entity =>
            {
                entity.Property(e => e.PersonalPlaylistId)
                    .HasMaxLength(34)
                    .IsUnicode(false);

                entity.Property(e => e.RequestPlaylistId)
                    .IsRequired()
                    .HasMaxLength(34)
                    .IsUnicode(false);

                entity.HasOne(d => d.Broadcaster)
                    .WithMany(p => p.SongRequestSetting)
                    .HasForeignKey(d => d.BroadcasterId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SongRequestSetting_Broadcaster");
            });

            modelBuilder.Entity<TwitchGameCategory>(entity =>
            {
                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });
        }
    }
}
