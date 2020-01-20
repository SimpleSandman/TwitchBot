CREATE SCHEMA Cmd;
GO

/****** Object:  Table [Cmd].[CustomCommand]    Script Date: 12/15/2019 8:25:50 PM ******/
SET ANSI_NULLS ON;
GO

SET QUOTED_IDENTIFIER ON;
GO

CREATE TABLE Cmd.CustomCommand
(
    Id INT IDENTITY(1, 1) NOT NULL
  , Name NVARCHAR(30) NOT NULL
  , Message NVARCHAR(500) NOT NULL
  , IsSound BIT NOT NULL
  , IsGlobalCooldown BIT NOT NULL
  , CooldownSec INT NOT NULL
  , CurrencyCost INT NOT NULL
  , GameId INT NULL
  , BroadcasterId INT NOT NULL
  , CONSTRAINT PK_Cmd_CustomCommand
        PRIMARY KEY CLUSTERED (Id ASC)
        WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY];
GO

/****** Object:  Table [Cmd].[Whitelist]    Script Date: 12/15/2019 8:25:50 PM ******/
SET ANSI_NULLS ON;
GO

SET QUOTED_IDENTIFIER ON;
GO

CREATE TABLE Cmd.Whitelist
(
    Id INT IDENTITY(1, 1) NOT NULL
  , CustomCommandId INT NOT NULL
  , Username VARCHAR(500) NOT NULL
  , TwitchId INT NOT NULL
  , CONSTRAINT PK_Cmd_Whitelist
        PRIMARY KEY CLUSTERED (Id ASC)
        WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY];
GO

/****** Object:  Table [dbo].[Bank]    Script Date: 12/15/2019 8:25:50 PM ******/
SET ANSI_NULLS ON;
GO

SET QUOTED_IDENTIFIER ON;
GO

CREATE TABLE dbo.Bank
(
    Id INT IDENTITY(1, 1) NOT NULL
  , Username VARCHAR(30) NOT NULL
  , TwitchId INT NULL
  , Wallet INT NOT NULL
  , Broadcaster INT NOT NULL
  , CONSTRAINT PK_Bank
        PRIMARY KEY CLUSTERED (Id ASC)
        WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY];
GO

/****** Object:  Table [dbo].[BankHeistSetting]    Script Date: 12/15/2019 8:25:51 PM ******/
SET ANSI_NULLS ON;
GO

SET QUOTED_IDENTIFIER ON;
GO

CREATE TABLE dbo.BankHeistSetting
(
    Id INT IDENTITY(1, 1) NOT NULL
  , BroadcasterId INT NOT NULL
  , CooldownPeriodMin INT NOT NULL
  , EntryPeriodSec INT NOT NULL
  , EntryMessage VARCHAR(150) NOT NULL
  , MaxGamble INT NOT NULL
  , MaxGambleText VARCHAR(100) NOT NULL
  , EntryInstructions VARCHAR(50) NOT NULL
  , CooldownEntry VARCHAR(150) NOT NULL
  , CooldownOver VARCHAR(150) NOT NULL
  , NextLevelMessage2 VARCHAR(200) NOT NULL
  , NextLevelMessage3 VARCHAR(200) NOT NULL
  , NextLevelMessage4 VARCHAR(200) NOT NULL
  , NextLevelMessage5 VARCHAR(200) NOT NULL
  , GameStart VARCHAR(200) NOT NULL
  , SingleUserSuccess VARCHAR(200) NOT NULL
  , SingleUserFail VARCHAR(200) NOT NULL
  , ResultsMessage VARCHAR(50) NOT NULL
  , Success100 VARCHAR(200) NOT NULL
  , Success34 VARCHAR(200) NOT NULL
  , Success1 VARCHAR(200) NOT NULL
  , Success0 VARCHAR(200) NOT NULL
  , LevelName1 VARCHAR(50) NOT NULL
  , LevelMaxUsers1 INT NOT NULL
  , LevelName2 VARCHAR(50) NOT NULL
  , LevelMaxUsers2 INT NOT NULL
  , LevelName3 VARCHAR(50) NOT NULL
  , LevelMaxUsers3 INT NOT NULL
  , LevelName4 VARCHAR(50) NOT NULL
  , LevelMaxUsers4 INT NOT NULL
  , LevelName5 VARCHAR(50) NOT NULL
  , LevelMaxUsers5 INT NOT NULL
  , PayoutSuccessRate1 DECIMAL(5, 2) NOT NULL
  , PayoutMultiplier1 DECIMAL(3, 2) NOT NULL
  , PayoutSuccessRate2 DECIMAL(5, 2) NOT NULL
  , PayoutMultiplier2 DECIMAL(3, 2) NOT NULL
  , PayoutSuccessRate3 DECIMAL(5, 2) NOT NULL
  , PayoutMultiplier3 DECIMAL(3, 2) NOT NULL
  , PayoutSuccessRate4 DECIMAL(5, 2) NOT NULL
  , PayoutMultiplier4 DECIMAL(3, 2) NOT NULL
  , PayoutSuccessRate5 DECIMAL(5, 2) NOT NULL
  , PayoutMultiplier5 DECIMAL(3, 2) NOT NULL
  , CONSTRAINT PK_BankHeistSetting
        PRIMARY KEY CLUSTERED (Id ASC)
        WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY];
GO

/****** Object:  Table [dbo].[BossFightBossStats]    Script Date: 12/15/2019 8:25:51 PM ******/
SET ANSI_NULLS ON;
GO

SET QUOTED_IDENTIFIER ON;
GO

CREATE TABLE dbo.BossFightBossStats
(
    Id INT IDENTITY(1, 1) NOT NULL
  , SettingsId INT NOT NULL
  , GameId INT NULL
  , Name1 VARCHAR(50) NOT NULL
  , MaxUsers1 INT NOT NULL
  , Attack1 INT NOT NULL
  , Defense1 INT NOT NULL
  , Evasion1 INT NOT NULL
  , Health1 INT NOT NULL
  , TurnLimit1 INT NOT NULL
  , Loot1 INT NOT NULL
  , LastAttackBonus1 INT NOT NULL
  , Name2 VARCHAR(50) NOT NULL
  , MaxUsers2 INT NOT NULL
  , Attack2 INT NOT NULL
  , Defense2 INT NOT NULL
  , Evasion2 INT NOT NULL
  , Health2 INT NOT NULL
  , TurnLimit2 INT NOT NULL
  , Loot2 INT NOT NULL
  , LastAttackBonus2 INT NOT NULL
  , Name3 VARCHAR(50) NOT NULL
  , MaxUsers3 INT NOT NULL
  , Attack3 INT NOT NULL
  , Defense3 INT NOT NULL
  , Evasion3 INT NOT NULL
  , Health3 INT NOT NULL
  , TurnLimit3 INT NOT NULL
  , Loot3 INT NOT NULL
  , LastAttackBonus3 INT NOT NULL
  , Name4 VARCHAR(50) NOT NULL
  , MaxUsers4 INT NOT NULL
  , Attack4 INT NOT NULL
  , Defense4 INT NOT NULL
  , Evasion4 INT NOT NULL
  , Health4 INT NOT NULL
  , TurnLimit4 INT NOT NULL
  , Loot4 INT NOT NULL
  , LastAttackBonus4 INT NOT NULL
  , Name5 VARCHAR(50) NOT NULL
  , MaxUsers5 INT NOT NULL
  , Attack5 INT NOT NULL
  , Defense5 INT NOT NULL
  , Evasion5 INT NOT NULL
  , Health5 INT NOT NULL
  , TurnLimit5 INT NOT NULL
  , Loot5 INT NOT NULL
  , LastAttackBonus5 INT NOT NULL
  , CONSTRAINT PK_BossFightBossStats
        PRIMARY KEY CLUSTERED (Id ASC)
        WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY];
GO

/****** Object:  Table [dbo].[BossFightClassStats]    Script Date: 12/15/2019 8:25:51 PM ******/
SET ANSI_NULLS ON;
GO

SET QUOTED_IDENTIFIER ON;
GO

CREATE TABLE dbo.BossFightClassStats
(
    Id INT IDENTITY(1, 1) NOT NULL
  , SettingsId INT NOT NULL
  , ViewerAttack INT NOT NULL
  , ViewerDefense INT NOT NULL
  , ViewerEvasion INT NOT NULL
  , ViewerHealth INT NOT NULL
  , FollowerAttack INT NOT NULL
  , FollowerDefense INT NOT NULL
  , FollowerEvasion INT NOT NULL
  , FollowerHealth INT NOT NULL
  , RegularAttack INT NOT NULL
  , RegularDefense INT NOT NULL
  , RegularEvasion INT NOT NULL
  , RegularHealth INT NOT NULL
  , ModeratorAttack INT NOT NULL
  , ModeratorDefense INT NOT NULL
  , ModeratorEvasion INT NOT NULL
  , ModeratorHealth INT NOT NULL
  , SubscriberAttack INT NOT NULL
  , SubscriberDefense INT NOT NULL
  , SubscriberEvasion INT NOT NULL
  , SubscriberHealth INT NOT NULL
  , CONSTRAINT PK_BossFightClassStats
        PRIMARY KEY CLUSTERED (Id ASC)
        WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY];
GO

/****** Object:  Table [dbo].[BossFightSetting]    Script Date: 12/15/2019 8:25:51 PM ******/
SET ANSI_NULLS ON;
GO

SET QUOTED_IDENTIFIER ON;
GO

CREATE TABLE dbo.BossFightSetting
(
    Id INT IDENTITY(1, 1) NOT NULL
  , CooldownPeriodMin INT NOT NULL
  , EntryPeriodSec INT NOT NULL
  , EntryMessage VARCHAR(150) NOT NULL
  , Cost INT NOT NULL
  , CooldownEntry VARCHAR(150) NOT NULL
  , CooldownOver VARCHAR(150) NOT NULL
  , NextLevelMessage2 VARCHAR(200) NOT NULL
  , NextLevelMessage3 VARCHAR(200) NOT NULL
  , NextLevelMessage4 VARCHAR(200) NOT NULL
  , NextLevelMessage5 VARCHAR(200) NOT NULL
  , GameStart VARCHAR(200) NOT NULL
  , ResultsMessage VARCHAR(50) NOT NULL
  , SingleUserSuccess VARCHAR(200) NOT NULL
  , SingleUserFail VARCHAR(200) NOT NULL
  , Success100 VARCHAR(200) NOT NULL
  , Success34 VARCHAR(200) NOT NULL
  , Success1 VARCHAR(200) NOT NULL
  , Success0 VARCHAR(200) NOT NULL
  , BroadcasterId INT NOT NULL
  , CONSTRAINT PK_BossFightSetting
        PRIMARY KEY CLUSTERED (Id ASC)
        WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY];
GO

/****** Object:  Table [dbo].[BotModerator]    Script Date: 12/15/2019 8:25:51 PM ******/
SET ANSI_NULLS ON;
GO

SET QUOTED_IDENTIFIER ON;
GO

CREATE TABLE dbo.BotModerator
(
    Id INT IDENTITY(1, 1) NOT NULL
  , Username VARCHAR(30) NOT NULL
  , TwitchId INT NOT NULL
  , BroadcasterId INT NOT NULL
  , CONSTRAINT PK_BotModerator
        PRIMARY KEY CLUSTERED (Id ASC)
        WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY];
GO

/****** Object:  Table [dbo].[BotTimeout]    Script Date: 12/15/2019 8:25:51 PM ******/
SET ANSI_NULLS ON;
GO

SET QUOTED_IDENTIFIER ON;
GO

CREATE TABLE dbo.BotTimeout
(
    Id INT IDENTITY(1, 1) NOT NULL
  , Username VARCHAR(30) NOT NULL
  , TwitchId INT NOT NULL
  , Timeout DATETIME NOT NULL
  , TimeAdded DATETIME NOT NULL
  , BroadcasterId INT NOT NULL
  , CONSTRAINT PK_BotTimeout
        PRIMARY KEY CLUSTERED (Id ASC)
        WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY];
GO

/****** Object:  Table [dbo].[Broadcaster]    Script Date: 12/15/2019 8:25:51 PM ******/
SET ANSI_NULLS ON;
GO

SET QUOTED_IDENTIFIER ON;
GO

CREATE TABLE dbo.Broadcaster
(
    Id INT IDENTITY(1, 1) NOT NULL
  , Username VARCHAR(30) NOT NULL
  , TwitchId INT NOT NULL
  , LastUpdated DATETIME NOT NULL
  , CONSTRAINT PK_Broadcaster
        PRIMARY KEY CLUSTERED (Id ASC)
        WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY];
GO

/****** Object:  Table [dbo].[ErrorLog]    Script Date: 12/15/2019 8:25:51 PM ******/
SET ANSI_NULLS ON;
GO

SET QUOTED_IDENTIFIER ON;
GO

CREATE TABLE dbo.ErrorLog
(
    ID INT IDENTITY(1, 1) NOT NULL
  , ErrorTime DATETIME NOT NULL
  , ErrorLine INT NOT NULL
  , ErrorClass VARCHAR(100) NULL
  , ErrorMethod VARCHAR(100) NULL
  , ErrorMsg VARCHAR(4000) NOT NULL
  , BroadcasterId INT NOT NULL
  , Command VARCHAR(50) NULL
  , UserMsg VARCHAR(500) NULL
  , CONSTRAINT PK_ErrorLog
        PRIMARY KEY CLUSTERED (ID ASC)
        WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY];
GO

/****** Object:  Table [dbo].[InGameUsername]    Script Date: 12/15/2019 8:25:51 PM ******/
SET ANSI_NULLS ON;
GO

SET QUOTED_IDENTIFIER ON;
GO

CREATE TABLE dbo.InGameUsername
(
    Id INT IDENTITY(1, 1) NOT NULL
  , Message VARCHAR(500) NOT NULL
  , BroadcasterId INT NOT NULL
  , GameId INT NULL
  , CONSTRAINT PK_InGameUsername
        PRIMARY KEY CLUSTERED (Id ASC)
        WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY];
GO

/****** Object:  Table [dbo].[PartyUp]    Script Date: 12/15/2019 8:25:51 PM ******/
SET ANSI_NULLS ON;
GO

SET QUOTED_IDENTIFIER ON;
GO

CREATE TABLE dbo.PartyUp
(
    Id INT IDENTITY(1, 1) NOT NULL
  , PartyMemberName VARCHAR(100) NOT NULL
  , GameId INT NOT NULL
  , BroadcasterId INT NOT NULL
  , CONSTRAINT PK_PartyUp
        PRIMARY KEY CLUSTERED (Id ASC)
        WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY];
GO

/****** Object:  Table [dbo].[PartyUpRequest]    Script Date: 12/15/2019 8:25:52 PM ******/
SET ANSI_NULLS ON;
GO

SET QUOTED_IDENTIFIER ON;
GO

CREATE TABLE dbo.PartyUpRequest
(
    Id INT IDENTITY(1, 1) NOT NULL
  , Username VARCHAR(30) NOT NULL
  , TwitchId INT NULL
  , PartyMemberId INT NOT NULL
  , TimeRequested DATETIME NOT NULL
  , CONSTRAINT PK_PartyUpRequest
        PRIMARY KEY CLUSTERED (Id ASC)
        WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY];
GO

/****** Object:  Table [dbo].[Quote]    Script Date: 12/15/2019 8:25:52 PM ******/
SET ANSI_NULLS ON;
GO

SET QUOTED_IDENTIFIER ON;
GO

CREATE TABLE dbo.Quote
(
    Id INT IDENTITY(1, 1) NOT NULL
  , UserQuote VARCHAR(500) NOT NULL
  , Username VARCHAR(50) NOT NULL
  , TimeCreated DATETIME NOT NULL
  , BroadcasterId INT NOT NULL
  , CONSTRAINT PK_Quote
        PRIMARY KEY CLUSTERED (Id ASC)
        WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY];
GO

/****** Object:  Table [dbo].[Rank]    Script Date: 12/15/2019 8:25:52 PM ******/
SET ANSI_NULLS ON;
GO

SET QUOTED_IDENTIFIER ON;
GO

CREATE TABLE dbo.Rank
(
    Id INT IDENTITY(1, 1) NOT NULL
  , Name VARCHAR(50) NOT NULL
  , ExpCap INT NOT NULL
  , BroadcasterId INT NOT NULL
  , CONSTRAINT PK_Rank
        PRIMARY KEY CLUSTERED (Id ASC)
        WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY];
GO

/****** Object:  Table [dbo].[RankFollower]    Script Date: 12/15/2019 8:25:52 PM ******/
SET ANSI_NULLS ON;
GO

SET QUOTED_IDENTIFIER ON;
GO

CREATE TABLE dbo.RankFollower
(
    Id INT IDENTITY(1, 1) NOT NULL
  , Username VARCHAR(30) NOT NULL
  , TwitchId INT NULL
  , Experience INT NOT NULL
  , BroadcasterId INT NOT NULL
  , CONSTRAINT PK_RankFollower
        PRIMARY KEY CLUSTERED (Id ASC)
        WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY];
GO

/****** Object:  Table [dbo].[Reminder]    Script Date: 12/15/2019 8:25:52 PM ******/
SET ANSI_NULLS ON;
GO

SET QUOTED_IDENTIFIER ON;
GO

CREATE TABLE dbo.Reminder
(
    Id INT IDENTITY(1, 1) NOT NULL
  , Sunday BIT NOT NULL
  , Monday BIT NOT NULL
  , Tuesday BIT NOT NULL
  , Wednesday BIT NOT NULL
  , Thursday BIT NOT NULL
  , Friday BIT NOT NULL
  , Saturday BIT NOT NULL
  , ReminderSec1 INT NULL
  , ReminderSec2 INT NULL
  , ReminderSec3 INT NULL
  , ReminderSec4 INT NULL
  , ReminderSec5 INT NULL
  , RemindEveryMin INT NULL
  , TimeOfEventUtc TIME(7) NULL
  , ExpirationDateUtc DATETIME NULL
  , IsCountdownEvent BIT NOT NULL
  , HasCountdownTicker BIT NOT NULL
  , Message VARCHAR(500) NULL
  , BroadcasterId INT NOT NULL
  , GameId INT NULL
  , CONSTRAINT PK_Reminder
        PRIMARY KEY CLUSTERED (Id ASC)
        WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY];
GO

/****** Object:  Table [dbo].[SongRequest]    Script Date: 12/15/2019 8:25:52 PM ******/
SET ANSI_NULLS ON;
GO

SET QUOTED_IDENTIFIER ON;
GO

CREATE TABLE dbo.SongRequest
(
    Id INT IDENTITY(1, 1) NOT NULL
  , Name VARCHAR(100) NOT NULL
  , Username VARCHAR(30) NULL
  , TwitchId INT NULL
  , BroadcasterId INT NOT NULL
  , CONSTRAINT PK_SongRequest
        PRIMARY KEY CLUSTERED (Id ASC)
        WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY];
GO

/****** Object:  Table [dbo].[SongRequestIgnore]    Script Date: 12/15/2019 8:25:52 PM ******/
SET ANSI_NULLS ON;
GO

SET QUOTED_IDENTIFIER ON;
GO

CREATE TABLE dbo.SongRequestIgnore
(
    Id INT IDENTITY(1, 1) NOT NULL
  , Artist VARCHAR(100) NOT NULL
  , Title VARCHAR(100) NULL
  , BroadcasterId INT NOT NULL
  , CONSTRAINT PK_SongRequestIgnore
        PRIMARY KEY CLUSTERED (Id ASC)
        WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY];
GO

/****** Object:  Table [dbo].[SongRequestSetting]    Script Date: 12/15/2019 8:25:52 PM ******/
SET ANSI_NULLS ON;
GO

SET QUOTED_IDENTIFIER ON;
GO

CREATE TABLE dbo.SongRequestSetting
(
    Id INT IDENTITY(1, 1) NOT NULL
  , RequestPlaylistId VARCHAR(34) NOT NULL
  , PersonalPlaylistId VARCHAR(34) NULL
  , DjMode BIT NOT NULL
  , BroadcasterId INT NOT NULL
  , CONSTRAINT PK_SongRequestSetting
        PRIMARY KEY CLUSTERED (Id ASC)
        WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY];
GO

/****** Object:  Table [dbo].[TwitchGameCategory]    Script Date: 12/15/2019 8:25:52 PM ******/
SET ANSI_NULLS ON;
GO

SET QUOTED_IDENTIFIER ON;
GO

CREATE TABLE dbo.TwitchGameCategory
(
    Id INT IDENTITY(1, 1) NOT NULL
  , Title VARCHAR(100) NOT NULL
  , Multiplayer BIT NOT NULL
  , CONSTRAINT PK_TwitchGameCategory
        PRIMARY KEY NONCLUSTERED (Id ASC)
        WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY];
GO

ALTER TABLE Cmd.CustomCommand
ADD CONSTRAINT DF__CustomCom__IsSou__1352D76D
    DEFAULT ((0)) FOR IsSound;
GO

ALTER TABLE Cmd.CustomCommand
ADD CONSTRAINT DF_CustomCommand_IsGlobalCooldown
    DEFAULT ((0)) FOR IsGlobalCooldown;
GO

ALTER TABLE Cmd.CustomCommand
ADD CONSTRAINT DF_CustomCommand_Cooldown
    DEFAULT ((0)) FOR CooldownSec;
GO

ALTER TABLE Cmd.CustomCommand
ADD CONSTRAINT DF_CustomCommand_CurrencyCost
    DEFAULT ((0)) FOR CurrencyCost;
GO

ALTER TABLE dbo.Bank
ADD CONSTRAINT DF__tblBank__wallet__5441852A
    DEFAULT ((0)) FOR Wallet;
GO

ALTER TABLE dbo.BankHeistSetting
ADD
    DEFAULT ((10)) FOR CooldownPeriodMin;
GO

ALTER TABLE dbo.BankHeistSetting ADD DEFAULT ((120)) FOR EntryPeriodSec;
GO

ALTER TABLE dbo.BankHeistSetting
ADD
    DEFAULT ('@user@ has started planning a bank heist! Looking for a bigger crew for a bigger score. Join in!') FOR EntryMessage;
GO

ALTER TABLE dbo.BankHeistSetting ADD DEFAULT ((5000)) FOR MaxGamble;
GO

ALTER TABLE dbo.BankHeistSetting
ADD
    DEFAULT ('Buy-in to''ps out at @maxbet@ @pointsname@') FOR MaxGambleText;
GO

ALTER TABLE dbo.BankHeistSetting
ADD
    DEFAULT ('Type @command@ [x] to enter') FOR EntryInstructions;
GO

ALTER TABLE dbo.BankHeistSetting
ADD
    DEFAULT ('The cops are on high alert after the last job, we have to lay low for a bit. Call me again after @timeleft@ minutes') FOR CooldownEntry;
GO

ALTER TABLE dbo.BankHeistSetting
ADD
    DEFAULT ('Looks like the cops have given up the search … the banks are ripe for hitting!') FOR CooldownOver;
GO

ALTER TABLE dbo.BankHeistSetting
ADD
    DEFAULT ('With this crew, we can now hit the @bankname@. Lets see if we can get a bigger crew to hit the @nextbankname@!') FOR NextLevelMessage2;
GO

ALTER TABLE dbo.BankHeistSetting
ADD
    DEFAULT ('Oh yeah! With this crew, we can now hit the @bankname@. Lets see if we can get a bigger crew to hit the @nextbankname@!') FOR NextLevelMessage3;
GO

ALTER TABLE dbo.BankHeistSetting
ADD
    DEFAULT ('Hell yeah! We can now hit the @bankname@. A few more, and we could hit the @nextbankname@!') FOR NextLevelMessage4;
GO

ALTER TABLE dbo.BankHeistSetting
ADD
    DEFAULT ('Epic crew! We are going to hit the @bankname@ guys! Gear up and get ready to head out.') FOR NextLevelMessage5;
GO

ALTER TABLE dbo.BankHeistSetting
ADD
    DEFAULT ('Alright guys, check your guns. We are storming into the @bankname@ through all entrances. Let''s get the cash and get out before the cops get here.') FOR GameStart;
GO

ALTER TABLE dbo.BankHeistSetting
ADD
    DEFAULT ('@user@ executed the heist flawlessly, sneaking into the @bankname@ through the back entrance and looting @winamount@ @pointsname@ from the vault.') FOR SingleUserSuccess;
GO

ALTER TABLE dbo.BankHeistSetting
ADD
    DEFAULT ('Local security caught @user@ trying to sneak into the @bankname@ through the back entrance and opened fire.') FOR SingleUserFail;
GO

ALTER TABLE dbo.BankHeistSetting
ADD
    DEFAULT ('The heist payouts are:') FOR ResultsMessage;
GO

ALTER TABLE dbo.BankHeistSetting
ADD
    DEFAULT ('The execution was flawless, in and out before the first cop arrived on scene.') FOR Success100;
GO

ALTER TABLE dbo.BankHeistSetting
ADD
    DEFAULT ('The crew suffered a few losses engaging the local security team.') FOR Success34;
GO

ALTER TABLE dbo.BankHeistSetting
ADD
    DEFAULT ('The crew suffered major losses as they engaged the SWAT backup.') FOR Success1;
GO

ALTER TABLE dbo.BankHeistSetting
ADD
    DEFAULT ('SWAT teams nearby stormed the bank and killed the entire crew. Not a single soul survived…') FOR Success0;
GO

ALTER TABLE dbo.BankHeistSetting
ADD
    DEFAULT ('Simple Municipal Bank') FOR LevelName1;
GO

ALTER TABLE dbo.BankHeistSetting ADD DEFAULT ((9)) FOR LevelMaxUsers1;
GO

ALTER TABLE dbo.BankHeistSetting
ADD
    DEFAULT ('Simple City Bank') FOR LevelName2;
GO

ALTER TABLE dbo.BankHeistSetting ADD DEFAULT ((19)) FOR LevelMaxUsers2;
GO

ALTER TABLE dbo.BankHeistSetting
ADD
    DEFAULT ('Simple State Bank') FOR LevelName3;
GO

ALTER TABLE dbo.BankHeistSetting ADD DEFAULT ((29)) FOR LevelMaxUsers3;
GO

ALTER TABLE dbo.BankHeistSetting
ADD
    DEFAULT ('Simple National Reserve') FOR LevelName4;
GO

ALTER TABLE dbo.BankHeistSetting ADD DEFAULT ((39)) FOR LevelMaxUsers4;
GO

ALTER TABLE dbo.BankHeistSetting
ADD
    DEFAULT ('Simple Federal Reserve') FOR LevelName5;
GO

ALTER TABLE dbo.BankHeistSetting
ADD
    DEFAULT ((9001)) FOR LevelMaxUsers5;
GO

ALTER TABLE dbo.BankHeistSetting
ADD
    DEFAULT ((54.00)) FOR PayoutSuccessRate1;
GO

ALTER TABLE dbo.BankHeistSetting
ADD
    DEFAULT ((1.50)) FOR PayoutMultiplier1;
GO

ALTER TABLE dbo.BankHeistSetting
ADD
    DEFAULT ((48.80)) FOR PayoutSuccessRate2;
GO

ALTER TABLE dbo.BankHeistSetting
ADD
    DEFAULT ((1.70)) FOR PayoutMultiplier2;
GO

ALTER TABLE dbo.BankHeistSetting
ADD
    DEFAULT ((42.50)) FOR PayoutSuccessRate3;
GO

ALTER TABLE dbo.BankHeistSetting
ADD
    DEFAULT ((2.00)) FOR PayoutMultiplier3;
GO

ALTER TABLE dbo.BankHeistSetting
ADD
    DEFAULT ((38.70)) FOR PayoutSuccessRate4;
GO

ALTER TABLE dbo.BankHeistSetting
ADD
    DEFAULT ((2.25)) FOR PayoutMultiplier4;
GO

ALTER TABLE dbo.BankHeistSetting
ADD
    DEFAULT ((32.40)) FOR PayoutSuccessRate5;
GO

ALTER TABLE dbo.BankHeistSetting
ADD
    DEFAULT ((2.75)) FOR PayoutMultiplier5;
GO

ALTER TABLE dbo.BossFightBossStats
ADD CONSTRAINT DF__BossFight__name1__37FA4C37
    DEFAULT ('Boss 1') FOR Name1;
GO

ALTER TABLE dbo.BossFightBossStats
ADD CONSTRAINT DF__BossFight__maxUs__38EE7070
    DEFAULT ((9)) FOR MaxUsers1;
GO

ALTER TABLE dbo.BossFightBossStats
ADD CONSTRAINT DF__BossFight__attac__39E294A9
    DEFAULT ((15)) FOR Attack1;
GO

ALTER TABLE dbo.BossFightBossStats
ADD CONSTRAINT DF__BossFight__defen__3AD6B8E2
    DEFAULT ((0)) FOR Defense1;
GO

ALTER TABLE dbo.BossFightBossStats
ADD CONSTRAINT DF__BossFight__evasi__3BCADD1B
    DEFAULT ((5)) FOR Evasion1;
GO

ALTER TABLE dbo.BossFightBossStats
ADD CONSTRAINT DF__BossFight__healt__3CBF0154
    DEFAULT ((200)) FOR Health1;
GO

ALTER TABLE dbo.BossFightBossStats
ADD CONSTRAINT DF__BossFight__turnL__3DB3258D
    DEFAULT ((20)) FOR TurnLimit1;
GO

ALTER TABLE dbo.BossFightBossStats
ADD CONSTRAINT DF__BossFight__loot1__3EA749C6
    DEFAULT ((300)) FOR Loot1;
GO

ALTER TABLE dbo.BossFightBossStats
ADD CONSTRAINT DF__BossFight__lastA__3F9B6DFF
    DEFAULT ((150)) FOR LastAttackBonus1;
GO

ALTER TABLE dbo.BossFightBossStats
ADD CONSTRAINT DF__BossFight__name2__408F9238
    DEFAULT ('Boss 2') FOR Name2;
GO

ALTER TABLE dbo.BossFightBossStats
ADD CONSTRAINT DF__BossFight__maxUs__4183B671
    DEFAULT ((19)) FOR MaxUsers2;
GO

ALTER TABLE dbo.BossFightBossStats
ADD CONSTRAINT DF__BossFight__attac__4277DAAA
    DEFAULT ((25)) FOR Attack2;
GO

ALTER TABLE dbo.BossFightBossStats
ADD CONSTRAINT DF__BossFight__defen__436BFEE3
    DEFAULT ((10)) FOR Defense2;
GO

ALTER TABLE dbo.BossFightBossStats
ADD CONSTRAINT DF__BossFight__evasi__4460231C
    DEFAULT ((15)) FOR Evasion2;
GO

ALTER TABLE dbo.BossFightBossStats
ADD CONSTRAINT DF__BossFight__healt__45544755
    DEFAULT ((750)) FOR Health2;
GO

ALTER TABLE dbo.BossFightBossStats
ADD CONSTRAINT DF__BossFight__turnL__46486B8E
    DEFAULT ((20)) FOR TurnLimit2;
GO

ALTER TABLE dbo.BossFightBossStats
ADD CONSTRAINT DF__BossFight__loot2__473C8FC7
    DEFAULT ((750)) FOR Loot2;
GO

ALTER TABLE dbo.BossFightBossStats
ADD CONSTRAINT DF__BossFight__lastA__4830B400
    DEFAULT ((300)) FOR LastAttackBonus2;
GO

ALTER TABLE dbo.BossFightBossStats
ADD CONSTRAINT DF__BossFight__name3__4924D839
    DEFAULT ('Boss 3') FOR Name3;
GO

ALTER TABLE dbo.BossFightBossStats
ADD CONSTRAINT DF__BossFight__maxUs__4A18FC72
    DEFAULT ((29)) FOR MaxUsers3;
GO

ALTER TABLE dbo.BossFightBossStats
ADD CONSTRAINT DF__BossFight__attac__4B0D20AB
    DEFAULT ((35)) FOR Attack3;
GO

ALTER TABLE dbo.BossFightBossStats
ADD CONSTRAINT DF__BossFight__defen__4C0144E4
    DEFAULT ((20)) FOR Defense3;
GO

ALTER TABLE dbo.BossFightBossStats
ADD CONSTRAINT DF__BossFight__evasi__4CF5691D
    DEFAULT ((20)) FOR Evasion3;
GO

ALTER TABLE dbo.BossFightBossStats
ADD CONSTRAINT DF__BossFight__healt__4DE98D56
    DEFAULT ((1500)) FOR Health3;
GO

ALTER TABLE dbo.BossFightBossStats
ADD CONSTRAINT DF__BossFight__turnL__4EDDB18F
    DEFAULT ((20)) FOR TurnLimit3;
GO

ALTER TABLE dbo.BossFightBossStats
ADD CONSTRAINT DF__BossFight__loot3__4FD1D5C8
    DEFAULT ((2000)) FOR Loot3;
GO

ALTER TABLE dbo.BossFightBossStats
ADD CONSTRAINT DF__BossFight__lastA__50C5FA01
    DEFAULT ((600)) FOR LastAttackBonus3;
GO

ALTER TABLE dbo.BossFightBossStats
ADD CONSTRAINT DF__BossFight__name4__51BA1E3A
    DEFAULT ('Boss 4') FOR Name4;
GO

ALTER TABLE dbo.BossFightBossStats
ADD CONSTRAINT DF__BossFight__maxUs__52AE4273
    DEFAULT ((39)) FOR MaxUsers4;
GO

ALTER TABLE dbo.BossFightBossStats
ADD CONSTRAINT DF__BossFight__attac__53A266AC
    DEFAULT ((40)) FOR Attack4;
GO

ALTER TABLE dbo.BossFightBossStats
ADD CONSTRAINT DF__BossFight__defen__54968AE5
    DEFAULT ((25)) FOR Defense4;
GO

ALTER TABLE dbo.BossFightBossStats
ADD CONSTRAINT DF__BossFight__evasi__558AAF1E
    DEFAULT ((25)) FOR Evasion4;
GO

ALTER TABLE dbo.BossFightBossStats
ADD CONSTRAINT DF__BossFight__healt__567ED357
    DEFAULT ((3000)) FOR Health4;
GO

ALTER TABLE dbo.BossFightBossStats
ADD CONSTRAINT DF__BossFight__turnL__5772F790
    DEFAULT ((20)) FOR TurnLimit4;
GO

ALTER TABLE dbo.BossFightBossStats
ADD CONSTRAINT DF__BossFight__loot4__58671BC9
    DEFAULT ((5000)) FOR Loot4;
GO

ALTER TABLE dbo.BossFightBossStats
ADD CONSTRAINT DF__BossFight__lastA__595B4002
    DEFAULT ((1000)) FOR LastAttackBonus4;
GO

ALTER TABLE dbo.BossFightBossStats
ADD CONSTRAINT DF__BossFight__name5__5A4F643B
    DEFAULT ('Boss 5') FOR Name5;
GO

ALTER TABLE dbo.BossFightBossStats
ADD CONSTRAINT DF__BossFight__maxUs__5B438874
    DEFAULT ((49)) FOR MaxUsers5;
GO

ALTER TABLE dbo.BossFightBossStats
ADD CONSTRAINT DF__BossFight__attac__5C37ACAD
    DEFAULT ((50)) FOR Attack5;
GO

ALTER TABLE dbo.BossFightBossStats
ADD CONSTRAINT DF__BossFight__defen__5D2BD0E6
    DEFAULT ((30)) FOR Defense5;
GO

ALTER TABLE dbo.BossFightBossStats
ADD CONSTRAINT DF__BossFight__evasi__5E1FF51F
    DEFAULT ((35)) FOR Evasion5;
GO

ALTER TABLE dbo.BossFightBossStats
ADD CONSTRAINT DF__BossFight__healt__5F141958
    DEFAULT ((5000)) FOR Health5;
GO

ALTER TABLE dbo.BossFightBossStats
ADD CONSTRAINT DF__BossFight__turnL__60083D91
    DEFAULT ((20)) FOR TurnLimit5;
GO

ALTER TABLE dbo.BossFightBossStats
ADD CONSTRAINT DF__BossFight__loot5__60FC61CA
    DEFAULT ((10000)) FOR Loot5;
GO

ALTER TABLE dbo.BossFightBossStats
ADD CONSTRAINT DF__BossFight__lastA__61F08603
    DEFAULT ((2500)) FOR LastAttackBonus5;
GO

ALTER TABLE dbo.BossFightClassStats ADD DEFAULT ((10)) FOR ViewerAttack;
GO

ALTER TABLE dbo.BossFightClassStats ADD DEFAULT ((5)) FOR ViewerDefense;
GO

ALTER TABLE dbo.BossFightClassStats
ADD
    DEFAULT ((10)) FOR ViewerEvasion;
GO

ALTER TABLE dbo.BossFightClassStats
ADD
    DEFAULT ((100)) FOR ViewerHealth;
GO

ALTER TABLE dbo.BossFightClassStats
ADD
    DEFAULT ((15)) FOR FollowerAttack;
GO

ALTER TABLE dbo.BossFightClassStats
ADD
    DEFAULT ((7)) FOR FollowerDefense;
GO

ALTER TABLE dbo.BossFightClassStats
ADD
    DEFAULT ((15)) FOR FollowerEvasion;
GO

ALTER TABLE dbo.BossFightClassStats
ADD
    DEFAULT ((125)) FOR FollowerHealth;
GO

ALTER TABLE dbo.BossFightClassStats
ADD
    DEFAULT ((20)) FOR RegularAttack;
GO

ALTER TABLE dbo.BossFightClassStats
ADD
    DEFAULT ((10)) FOR RegularDefense;
GO

ALTER TABLE dbo.BossFightClassStats
ADD
    DEFAULT ((20)) FOR RegularEvasion;
GO

ALTER TABLE dbo.BossFightClassStats
ADD
    DEFAULT ((175)) FOR RegularHealth;
GO

ALTER TABLE dbo.BossFightClassStats
ADD
    DEFAULT ((35)) FOR ModeratorAttack;
GO

ALTER TABLE dbo.BossFightClassStats
ADD
    DEFAULT ((20)) FOR ModeratorDefense;
GO

ALTER TABLE dbo.BossFightClassStats
ADD
    DEFAULT ((35)) FOR ModeratorEvasion;
GO

ALTER TABLE dbo.BossFightClassStats
ADD
    DEFAULT ((225)) FOR ModeratorHealth;
GO

ALTER TABLE dbo.BossFightClassStats
ADD
    DEFAULT ((25)) FOR SubscriberAttack;
GO

ALTER TABLE dbo.BossFightClassStats
ADD
    DEFAULT ((15)) FOR SubscriberDefense;
GO

ALTER TABLE dbo.BossFightClassStats
ADD
    DEFAULT ((25)) FOR SubscriberEvasion;
GO

ALTER TABLE dbo.BossFightClassStats
ADD
    DEFAULT ((200)) FOR SubscriberHealth;
GO

ALTER TABLE dbo.BossFightSetting
ADD
    DEFAULT ((10)) FOR CooldownPeriodMin;
GO

ALTER TABLE dbo.BossFightSetting ADD DEFAULT ((60)) FOR EntryPeriodSec;
GO

ALTER TABLE dbo.BossFightSetting
ADD
    DEFAULT ('@user@ is trying to get a group of adventurers ready to fight a boss... Will you join them? Type !raid to join!') FOR EntryMessage;
GO

ALTER TABLE dbo.BossFightSetting ADD DEFAULT ((100)) FOR Cost;
GO

ALTER TABLE dbo.BossFightSetting
ADD
    DEFAULT ('The boss floor is currently being cleaned up and won''t be available for at least @timeleft@ minutes') FOR CooldownEntry;
GO

ALTER TABLE dbo.BossFightSetting
ADD
    DEFAULT ('The boss floor has been cleaned up... Want to go again?! Type !raid to start!') FOR CooldownOver;
GO

ALTER TABLE dbo.BossFightSetting
ADD
    DEFAULT ('With this raid party, we can now attack @bossname@. Let''s see if we can get a bigger party to attack @nextbossname@!') FOR NextLevelMessage2;
GO

ALTER TABLE dbo.BossFightSetting
ADD
    DEFAULT ('Oh yeah! With this raid party, we can now attack @bossname@. Let''s see if we can get a bigger party to attack @nextbossname@!') FOR NextLevelMessage3;
GO

ALTER TABLE dbo.BossFightSetting
ADD
    DEFAULT ('Hell yeah! We can now attack @bossname@. A few more and we could attack @nextbossname@!') FOR NextLevelMessage4;
GO

ALTER TABLE dbo.BossFightSetting
ADD
    DEFAULT ('Epic raid party! We are going to attack @bossname@ guys! Gear up and get ready to head out.') FOR NextLevelMessage5;
GO

ALTER TABLE dbo.BossFightSetting
ADD
    DEFAULT ('The combatants have stepped into the boss room... Will they be able to defeat @bossname@?!') FOR GameStart;
GO

ALTER TABLE dbo.BossFightSetting
ADD
    DEFAULT ('The survivors are:') FOR ResultsMessage;
GO

ALTER TABLE dbo.BossFightSetting
ADD
    DEFAULT ('@user@ executed the raid flawlessly, soloing @bossname@ with ease and looting @winamount@ @pointsname@ with a last attack bonus of @lastattackbonus@ @pointsname@.') FOR SingleUserSuccess;
GO

ALTER TABLE dbo.BossFightSetting
ADD
    DEFAULT ('@user@ thought they could try to solo @bossname@, but was deleted immediately...RIP') FOR SingleUserFail;
GO

ALTER TABLE dbo.BossFightSetting
ADD
    DEFAULT ('The raid was a complete success. No one fell into the hands of @bossname@.') FOR Success100;
GO

ALTER TABLE dbo.BossFightSetting
ADD
    DEFAULT ('The raid party suffered a few casualties as they fought valiantly.') FOR Success34;
GO

ALTER TABLE dbo.BossFightSetting
ADD
    DEFAULT ('The raid party suffered major casualties as they fought valiantly.') FOR Success1;
GO

ALTER TABLE dbo.BossFightSetting
ADD
    DEFAULT ('It was absolute hell. The field is covered with the blood of the fallen...no one survived') FOR Success0;
GO

ALTER TABLE dbo.BotTimeout
ADD CONSTRAINT DF__tblTimeou__timeA__66603565
    DEFAULT (GETDATE()) FOR TimeAdded;
GO

ALTER TABLE dbo.Broadcaster
ADD CONSTRAINT DF_tblBroadcasters_twitchId
    DEFAULT ((0)) FOR TwitchId;
GO

ALTER TABLE dbo.Broadcaster
ADD CONSTRAINT DF__Table__timeAdded__5812160E
    DEFAULT (GETDATE()) FOR LastUpdated;
GO

ALTER TABLE dbo.PartyUpRequest
ADD CONSTRAINT DF__tblGroupU__timeR__4BAC3F29
    DEFAULT (GETDATE()) FOR TimeRequested;
GO

ALTER TABLE dbo.Quote ADD DEFAULT (GETDATE()) FOR TimeCreated;
GO

ALTER TABLE dbo.Reminder ADD DEFAULT ((0)) FOR Sunday;
GO

ALTER TABLE dbo.Reminder ADD DEFAULT ((0)) FOR Monday;
GO

ALTER TABLE dbo.Reminder ADD DEFAULT ((0)) FOR Tuesday;
GO

ALTER TABLE dbo.Reminder ADD DEFAULT ((0)) FOR Wednesday;
GO

ALTER TABLE dbo.Reminder ADD DEFAULT ((0)) FOR Thursday;
GO

ALTER TABLE dbo.Reminder ADD DEFAULT ((0)) FOR Friday;
GO

ALTER TABLE dbo.Reminder ADD DEFAULT ((0)) FOR Saturday;
GO

ALTER TABLE dbo.Reminder ADD DEFAULT ((0)) FOR IsCountdownEvent;
GO

ALTER TABLE dbo.Reminder ADD DEFAULT ((0)) FOR HasCountdownTicker;
GO

ALTER TABLE dbo.SongRequestSetting
ADD CONSTRAINT DF__SongReque__DjMod__5FD33367
    DEFAULT ((0)) FOR DjMode;
GO

ALTER TABLE dbo.TwitchGameCategory
ADD CONSTRAINT DF_tblGameList_multiplayer
    DEFAULT ((0)) FOR Multiplayer;
GO

ALTER TABLE Cmd.CustomCommand WITH CHECK
ADD CONSTRAINT FK_Cmd_CustomCommand_Broadcaster
    FOREIGN KEY (BroadcasterId)
    REFERENCES dbo.Broadcaster (Id);
GO

ALTER TABLE Cmd.CustomCommand CHECK CONSTRAINT FK_Cmd_CustomCommand_Broadcaster;
GO

ALTER TABLE Cmd.CustomCommand WITH CHECK
ADD CONSTRAINT FK_Cmd_CustomCommand_TwitchGameCategory
    FOREIGN KEY (GameId)
    REFERENCES dbo.TwitchGameCategory (Id);
GO

ALTER TABLE Cmd.CustomCommand CHECK CONSTRAINT FK_Cmd_CustomCommand_TwitchGameCategory;
GO

ALTER TABLE Cmd.Whitelist WITH CHECK
ADD CONSTRAINT FK_Cmd_Whitelist_Cmd_CustomCommand
    FOREIGN KEY (CustomCommandId)
    REFERENCES Cmd.CustomCommand (Id);
GO

ALTER TABLE Cmd.Whitelist CHECK CONSTRAINT FK_Cmd_Whitelist_Cmd_CustomCommand;
GO

ALTER TABLE dbo.Bank WITH CHECK
ADD CONSTRAINT FK_Bank_Broadcaster
    FOREIGN KEY (Broadcaster)
    REFERENCES dbo.Broadcaster (Id);
GO

ALTER TABLE dbo.Bank CHECK CONSTRAINT FK_Bank_Broadcaster;
GO

ALTER TABLE dbo.BankHeistSetting WITH CHECK
ADD CONSTRAINT FK_BankHeistSetting_Broadcaster
    FOREIGN KEY (BroadcasterId)
    REFERENCES dbo.Broadcaster (Id);
GO

ALTER TABLE dbo.BankHeistSetting CHECK CONSTRAINT FK_BankHeistSetting_Broadcaster;
GO

ALTER TABLE dbo.BossFightBossStats WITH CHECK
ADD CONSTRAINT FK_BossFightBossStats_Broadcaster
    FOREIGN KEY (SettingsId)
    REFERENCES dbo.BossFightSetting (Id);
GO

ALTER TABLE dbo.BossFightBossStats CHECK CONSTRAINT FK_BossFightBossStats_Broadcaster;
GO

ALTER TABLE dbo.BossFightBossStats WITH CHECK
ADD CONSTRAINT FK_BossFightBossStats_TwitchGameCategory
    FOREIGN KEY (GameId)
    REFERENCES dbo.TwitchGameCategory (Id);
GO

ALTER TABLE dbo.BossFightBossStats CHECK CONSTRAINT FK_BossFightBossStats_TwitchGameCategory;
GO

ALTER TABLE dbo.BossFightClassStats WITH CHECK
ADD CONSTRAINT FK_BossFightClassStats_Broadcaster
    FOREIGN KEY (SettingsId)
    REFERENCES dbo.BossFightSetting (Id);
GO

ALTER TABLE dbo.BossFightClassStats CHECK CONSTRAINT FK_BossFightClassStats_Broadcaster;
GO

ALTER TABLE dbo.BossFightSetting WITH CHECK
ADD CONSTRAINT FK_BossFightSetting_Broadcaster
    FOREIGN KEY (BroadcasterId)
    REFERENCES dbo.Broadcaster (Id);
GO

ALTER TABLE dbo.BossFightSetting CHECK CONSTRAINT FK_BossFightSetting_Broadcaster;
GO

ALTER TABLE dbo.BotModerator WITH CHECK
ADD CONSTRAINT FK_BotModerator_Broadcaster
    FOREIGN KEY (BroadcasterId)
    REFERENCES dbo.Broadcaster (Id);
GO

ALTER TABLE dbo.BotModerator CHECK CONSTRAINT FK_BotModerator_Broadcaster;
GO

ALTER TABLE dbo.BotTimeout WITH CHECK
ADD CONSTRAINT FK_BotTimeout_Broadcaster
    FOREIGN KEY (BroadcasterId)
    REFERENCES dbo.Broadcaster (Id);
GO

ALTER TABLE dbo.BotTimeout CHECK CONSTRAINT FK_BotTimeout_Broadcaster;
GO

ALTER TABLE dbo.ErrorLog WITH CHECK
ADD CONSTRAINT FK_ErrorLog_Broadcaster
    FOREIGN KEY (BroadcasterId)
    REFERENCES dbo.Broadcaster (Id);
GO

ALTER TABLE dbo.ErrorLog CHECK CONSTRAINT FK_ErrorLog_Broadcaster;
GO

ALTER TABLE dbo.InGameUsername WITH CHECK
ADD CONSTRAINT FK_InGameUsername_Broadcaster
    FOREIGN KEY (BroadcasterId)
    REFERENCES dbo.Broadcaster (Id);
GO

ALTER TABLE dbo.InGameUsername CHECK CONSTRAINT FK_InGameUsername_Broadcaster;
GO

ALTER TABLE dbo.InGameUsername WITH CHECK
ADD CONSTRAINT FK_InGameUsername_TwitchGameCategory
    FOREIGN KEY (GameId)
    REFERENCES dbo.TwitchGameCategory (Id);
GO

ALTER TABLE dbo.InGameUsername CHECK CONSTRAINT FK_InGameUsername_TwitchGameCategory;
GO

ALTER TABLE dbo.PartyUp WITH CHECK
ADD CONSTRAINT FK_PartyUp_Broadcaster
    FOREIGN KEY (BroadcasterId)
    REFERENCES dbo.Broadcaster (Id);
GO

ALTER TABLE dbo.PartyUp CHECK CONSTRAINT FK_PartyUp_Broadcaster;
GO

ALTER TABLE dbo.PartyUp WITH CHECK
ADD CONSTRAINT FK_PartyUp_TwitchGameCategory
    FOREIGN KEY (GameId)
    REFERENCES dbo.TwitchGameCategory (Id);
GO

ALTER TABLE dbo.PartyUp CHECK CONSTRAINT FK_PartyUp_TwitchGameCategory;
GO

ALTER TABLE dbo.PartyUpRequest WITH CHECK
ADD CONSTRAINT FK_PartyUpRequest_PartyUp
    FOREIGN KEY (PartyMemberId)
    REFERENCES dbo.PartyUp (Id);
GO

ALTER TABLE dbo.PartyUpRequest CHECK CONSTRAINT FK_PartyUpRequest_PartyUp;
GO

ALTER TABLE dbo.Quote WITH CHECK
ADD CONSTRAINT FK_Quote_Broadcasters
    FOREIGN KEY (BroadcasterId)
    REFERENCES dbo.Broadcaster (Id);
GO

ALTER TABLE dbo.Quote CHECK CONSTRAINT FK_Quote_Broadcasters;
GO

ALTER TABLE dbo.Rank WITH CHECK
ADD CONSTRAINT FK_Rank_Broadcaster
    FOREIGN KEY (BroadcasterId)
    REFERENCES dbo.Broadcaster (Id);
GO

ALTER TABLE dbo.Rank CHECK CONSTRAINT FK_Rank_Broadcaster;
GO

ALTER TABLE dbo.RankFollower WITH CHECK
ADD CONSTRAINT FK_RankFollower_Broadcaster
    FOREIGN KEY (BroadcasterId)
    REFERENCES dbo.Broadcaster (Id);
GO

ALTER TABLE dbo.RankFollower CHECK CONSTRAINT FK_RankFollower_Broadcaster;
GO

ALTER TABLE dbo.Reminder WITH CHECK
ADD CONSTRAINT FK_Reminder_Broadcaster
    FOREIGN KEY (BroadcasterId)
    REFERENCES dbo.Broadcaster (Id);
GO

ALTER TABLE dbo.Reminder CHECK CONSTRAINT FK_Reminder_Broadcaster;
GO

ALTER TABLE dbo.Reminder WITH CHECK
ADD CONSTRAINT FK_Reminder_TwitchGameCategory
    FOREIGN KEY (GameId)
    REFERENCES dbo.TwitchGameCategory (Id);
GO

ALTER TABLE dbo.Reminder CHECK CONSTRAINT FK_Reminder_TwitchGameCategory;
GO

ALTER TABLE dbo.SongRequest WITH CHECK
ADD CONSTRAINT FK_SongRequest_Broadcaster
    FOREIGN KEY (BroadcasterId)
    REFERENCES dbo.Broadcaster (Id);
GO

ALTER TABLE dbo.SongRequest CHECK CONSTRAINT FK_SongRequest_Broadcaster;
GO

ALTER TABLE dbo.SongRequestIgnore WITH CHECK
ADD CONSTRAINT FK_SongRequestIgnore_Broadcaster
    FOREIGN KEY (BroadcasterId)
    REFERENCES dbo.Broadcaster (Id);
GO

ALTER TABLE dbo.SongRequestIgnore CHECK CONSTRAINT FK_SongRequestIgnore_Broadcaster;
GO

ALTER TABLE dbo.SongRequestSetting WITH CHECK
ADD CONSTRAINT FK_SongRequestSetting_Broadcaster
    FOREIGN KEY (BroadcasterId)
    REFERENCES dbo.Broadcaster (Id);
GO

ALTER TABLE dbo.SongRequestSetting CHECK CONSTRAINT FK_SongRequestSetting_Broadcaster;
GO


