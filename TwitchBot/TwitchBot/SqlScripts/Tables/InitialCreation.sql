/****** Object:  Table [dbo].[Bank]    Script Date: 9/6/2018 12:10:41 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Bank](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Username] [varchar](30) NOT NULL,
	[Wallet] [int] NOT NULL,
	[TwitchId] [int] NULL,
	[Broadcaster] [int] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_Bank] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[BankHeistSetting]    Script Date: 9/6/2018 12:10:41 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[BankHeistSetting](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Broadcaster] [int] NOT NULL,
	[CooldownPeriodMin] [int] NOT NULL,
	[EntryPeriodSec] [int] NOT NULL,
	[EntryMessage] [varchar](150) NOT NULL,
	[MaxGamble] [int] NOT NULL,
	[MaxGambleText] [varchar](100) NOT NULL,
	[EntryInstructions] [varchar](50) NOT NULL,
	[CooldownEntry] [varchar](150) NOT NULL,
	[CooldownOver] [varchar](150) NOT NULL,
	[NextLevelMessage2] [varchar](200) NOT NULL,
	[NextLevelMessage3] [varchar](200) NOT NULL,
	[NextLevelMessage4] [varchar](200) NOT NULL,
	[NextLevelMessage5] [varchar](200) NOT NULL,
	[GameStart] [varchar](200) NOT NULL,
	[SingleUserSuccess] [varchar](200) NOT NULL,
	[SingleUserFail] [varchar](200) NOT NULL,
	[ResultsMessage] [varchar](50) NOT NULL,
	[Success100] [varchar](200) NOT NULL,
	[Success34] [varchar](200) NOT NULL,
	[Success1] [varchar](200) NOT NULL,
	[Success0] [varchar](200) NOT NULL,
	[LevelName1] [varchar](50) NOT NULL,
	[LevelMaxUsers1] [int] NOT NULL,
	[LevelName2] [varchar](50) NOT NULL,
	[LevelMaxUsers2] [int] NOT NULL,
	[LevelName3] [varchar](50) NOT NULL,
	[LevelMaxUsers3] [int] NOT NULL,
	[LevelName4] [varchar](50) NOT NULL,
	[LevelMaxUsers4] [int] NOT NULL,
	[LevelName5] [varchar](50) NOT NULL,
	[LevelMaxUsers5] [int] NOT NULL,
	[PayoutSuccessRate1] [decimal](5, 2) NOT NULL,
	[PayoutMultiplier1] [decimal](3, 2) NOT NULL,
	[PayoutSuccessRate2] [decimal](5, 2) NOT NULL,
	[PayoutMultiplier2] [decimal](3, 2) NOT NULL,
	[PayoutSuccessRate3] [decimal](5, 2) NOT NULL,
	[PayoutMultiplier3] [decimal](3, 2) NOT NULL,
	[PayoutSuccessRate4] [decimal](5, 2) NOT NULL,
	[PayoutMultiplier4] [decimal](3, 2) NOT NULL,
	[PayoutSuccessRate5] [decimal](5, 2) NOT NULL,
	[PayoutMultiplier5] [decimal](3, 2) NOT NULL,
 CONSTRAINT [PK_BankHeistSetting] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[BossFightBossStats]    Script Date: 9/6/2018 12:10:41 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[BossFightBossStats](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SettingsId] [int] NOT NULL,
	[GameId] [int] NULL,
	[Name1] [varchar](50) NOT NULL,
	[MaxUsers1] [int] NOT NULL,
	[Attack1] [int] NOT NULL,
	[Defense1] [int] NOT NULL,
	[Evasion1] [int] NOT NULL,
	[Health1] [int] NOT NULL,
	[TurnLimit1] [int] NOT NULL,
	[Loot1] [int] NOT NULL,
	[LastAttackBonus1] [int] NOT NULL,
	[Name2] [varchar](50) NOT NULL,
	[MaxUsers2] [int] NOT NULL,
	[Attack2] [int] NOT NULL,
	[Defense2] [int] NOT NULL,
	[Evasion2] [int] NOT NULL,
	[Health2] [int] NOT NULL,
	[TurnLimit2] [int] NOT NULL,
	[Loot2] [int] NOT NULL,
	[LastAttackBonus2] [int] NOT NULL,
	[Name3] [varchar](50) NOT NULL,
	[MaxUsers3] [int] NOT NULL,
	[Attack3] [int] NOT NULL,
	[Defense3] [int] NOT NULL,
	[Evasion3] [int] NOT NULL,
	[Health3] [int] NOT NULL,
	[TurnLimit3] [int] NOT NULL,
	[Loot3] [int] NOT NULL,
	[LastAttackBonus3] [int] NOT NULL,
	[Name4] [varchar](50) NOT NULL,
	[MaxUsers4] [int] NOT NULL,
	[Attack4] [int] NOT NULL,
	[Defense4] [int] NOT NULL,
	[Evasion4] [int] NOT NULL,
	[Health4] [int] NOT NULL,
	[TurnLimit4] [int] NOT NULL,
	[Loot4] [int] NOT NULL,
	[LastAttackBonus4] [int] NOT NULL,
	[Name5] [varchar](50) NOT NULL,
	[MaxUsers5] [int] NOT NULL,
	[Attack5] [int] NOT NULL,
	[Defense5] [int] NOT NULL,
	[Evasion5] [int] NOT NULL,
	[Health5] [int] NOT NULL,
	[TurnLimit5] [int] NOT NULL,
	[Loot5] [int] NOT NULL,
	[LastAttackBonus5] [int] NOT NULL,
 CONSTRAINT [PK_BossFightBossStats] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[BossFightClassStats]    Script Date: 9/6/2018 12:10:42 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[BossFightClassStats](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SettingsId] [int] NOT NULL,
	[ViewerAttack] [int] NOT NULL,
	[ViewerDefense] [int] NOT NULL,
	[ViewerEvasion] [int] NOT NULL,
	[ViewerHealth] [int] NOT NULL,
	[FollowerAttack] [int] NOT NULL,
	[FollowerDefense] [int] NOT NULL,
	[FollowerEvasion] [int] NOT NULL,
	[FollowerHealth] [int] NOT NULL,
	[RegularAttack] [int] NOT NULL,
	[RegularDefense] [int] NOT NULL,
	[RegularEvasion] [int] NOT NULL,
	[RegularHealth] [int] NOT NULL,
	[ModeratorAttack] [int] NOT NULL,
	[ModeratorDefense] [int] NOT NULL,
	[ModeratorEvasion] [int] NOT NULL,
	[ModeratorHealth] [int] NOT NULL,
	[SubscriberAttack] [int] NOT NULL,
	[SubscriberDefense] [int] NOT NULL,
	[SubscriberEvasion] [int] NOT NULL,
	[SubscriberHealth] [int] NOT NULL,
 CONSTRAINT [PK_BossFightClassStats] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[BossFightSetting]    Script Date: 9/6/2018 12:10:42 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[BossFightSetting](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CooldownPeriodMin] [int] NOT NULL,
	[EntryPeriodSec] [int] NOT NULL,
	[EntryMessage] [varchar](150) NOT NULL,
	[Cost] [int] NOT NULL,
	[CooldownEntry] [varchar](150) NOT NULL,
	[CooldownOver] [varchar](150) NOT NULL,
	[NextLevelMessage2] [varchar](200) NOT NULL,
	[NextLevelMessage3] [varchar](200) NOT NULL,
	[NextLevelMessage4] [varchar](200) NOT NULL,
	[NextLevelMessage5] [varchar](200) NOT NULL,
	[GameStart] [varchar](200) NOT NULL,
	[ResultsMessage] [varchar](50) NOT NULL,
	[SingleUserSuccess] [varchar](200) NOT NULL,
	[SingleUserFail] [varchar](200) NOT NULL,
	[Success100] [varchar](200) NOT NULL,
	[Success34] [varchar](200) NOT NULL,
	[Success1] [varchar](200) NOT NULL,
	[Success0] [varchar](200) NOT NULL,
	[Broadcaster] [int] NOT NULL,
 CONSTRAINT [PK_BossFightSetting] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[BotTimeout]    Script Date: 9/6/2018 12:10:42 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[BotTimeout](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Username] [varchar](30) NOT NULL,
	[Timeout] [datetime] NOT NULL,
	[TimeAdded] [datetime] NOT NULL,
	[Broadcaster] [int] NOT NULL,
 CONSTRAINT [PK_BotTimeout] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[Broadcaster]    Script Date: 9/6/2018 12:10:42 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Broadcaster](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Username] [varchar](30) NOT NULL,
	[TwitchId] [int] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_Broadcaster] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[ErrorLog]    Script Date: 9/6/2018 12:10:42 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ErrorLog](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ErrorTime] [datetime] NOT NULL,
	[ErrorLine] [int] NOT NULL,
	[ErrorClass] [varchar](100) NULL,
	[ErrorMethod] [varchar](100) NULL,
	[ErrorMsg] [varchar](4000) NOT NULL,
	[Broadcaster] [int] NOT NULL,
	[Command] [varchar](50) NULL,
	[UserMsg] [varchar](500) NULL,
 CONSTRAINT [PK_ErrorLog] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[PartyUp]    Script Date: 9/6/2018 12:10:43 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[PartyUp](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PartyMember] [varchar](100) NOT NULL,
	[GameId] [int] NOT NULL,
	[Broadcaster] [int] NOT NULL,
 CONSTRAINT [PK_PartyUp] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[PartyUpRequest]    Script Date: 9/6/2018 12:10:43 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[PartyUpRequest](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Username] [varchar](30) NOT NULL,
	[PartyMember] [int] NOT NULL,
	[TimeRequested] [datetime] NOT NULL,
 CONSTRAINT [PK_PartyUpRequest] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[Quote]    Script Date: 9/6/2018 12:10:43 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Quote](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserQuote] [varchar](500) NOT NULL,
	[Username] [varchar](50) NOT NULL,
	[TimeCreated] [datetime] NOT NULL,
	[Broadcaster] [int] NOT NULL,
 CONSTRAINT [PK_Quote] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[Rank]    Script Date: 9/6/2018 12:10:43 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Rank](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](50) NOT NULL,
	[ExpCap] [int] NOT NULL,
	[Broadcaster] [int] NOT NULL,
 CONSTRAINT [PK_Rank] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[RankFollower]    Script Date: 9/6/2018 12:10:43 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[RankFollower](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Username] [varchar](30) NOT NULL,
	[Experience] [int] NOT NULL,
	[TwitchId] [int] NULL,
	[Broadcaster] [int] NOT NULL,
 CONSTRAINT [PK_RankFollower] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[Reminder]    Script Date: 9/6/2018 12:10:44 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Reminder](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Sunday] [bit] NOT NULL,
	[Monday] [bit] NOT NULL,
	[Tuesday] [bit] NOT NULL,
	[Wednesday] [bit] NOT NULL,
	[Thursday] [bit] NOT NULL,
	[Friday] [bit] NOT NULL,
	[Saturday] [bit] NOT NULL,
	[ReminderSec1] [int] NULL,
	[ReminderSec2] [int] NULL,
	[ReminderSec3] [int] NULL,
	[ReminderSec4] [int] NULL,
	[ReminderSec5] [int] NULL,
	[RemindEveryMin] [int] NULL,
	[TimeOfEventUtc] [time](7) NULL,
	[ExpirationDateUtc] [datetime] NULL,
	[IsCountdownEvent] [bit] NOT NULL,
	[HasCountdownTicker] [bit] NOT NULL,
	[Message] [varchar](500) NULL,
	[Broadcaster] [int] NOT NULL,
	[GameId] [int] NULL,
 CONSTRAINT [PK_Reminder] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[SongRequest]    Script Date: 9/6/2018 12:10:44 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[SongRequest](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Requests] [varchar](100) NOT NULL,
	[Chatter] [varchar](30) NULL,
	[Broadcaster] [int] NOT NULL,
 CONSTRAINT [PK_SongRequest] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[SongRequestIgnore]    Script Date: 9/6/2018 12:10:44 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[SongRequestIgnore](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Artist] [varchar](100) NOT NULL,
	[Title] [varchar](100) NULL,
	[Broadcaster] [int] NOT NULL,
 CONSTRAINT [PK_SongRequestIgnore] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[TwitchGameCategory]    Script Date: 9/6/2018 12:10:44 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[TwitchGameCategory](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Title] [varchar](100) NOT NULL,
	[Multiplayer] [bit] NOT NULL,
 CONSTRAINT [PK_TwitchGameCategory] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Bank] ADD  CONSTRAINT [DF__tblBank__wallet__5441852A]  DEFAULT ((0)) FOR [Wallet]
GO

ALTER TABLE [dbo].[Bank] ADD  CONSTRAINT [DF__tblBank__timeAdd__5AEE82B9]  DEFAULT (getdate()) FOR [LastUpdated]
GO

ALTER TABLE [dbo].[BankHeistSetting] ADD  DEFAULT ((10)) FOR [CooldownPeriodMin]
GO

ALTER TABLE [dbo].[BankHeistSetting] ADD  DEFAULT ((120)) FOR [EntryPeriodSec]
GO

ALTER TABLE [dbo].[BankHeistSetting] ADD  DEFAULT ('@user@ has started planning a bank heist! Looking for a bigger crew for a bigger score. Join in!') FOR [EntryMessage]
GO

ALTER TABLE [dbo].[BankHeistSetting] ADD  DEFAULT ((5000)) FOR [MaxGamble]
GO

ALTER TABLE [dbo].[BankHeistSetting] ADD  DEFAULT ('Buy-in to''ps out at @maxbet@ @pointsname@') FOR [MaxGambleText]
GO

ALTER TABLE [dbo].[BankHeistSetting] ADD  DEFAULT ('Type @command@ [x] to enter') FOR [EntryInstructions]
GO

ALTER TABLE [dbo].[BankHeistSetting] ADD  DEFAULT ('The cops are on high alert after the last job, we have to lay low for a bit. Call me again after @timeleft@ minutes') FOR [CooldownEntry]
GO

ALTER TABLE [dbo].[BankHeistSetting] ADD  DEFAULT ('Looks like the cops have given up the search … the banks are ripe for hitting!') FOR [CooldownOver]
GO

ALTER TABLE [dbo].[BankHeistSetting] ADD  DEFAULT ('With this crew, we can now hit the @bankname@. Lets see if we can get a bigger crew to hit the @nextbankname@!') FOR [NextLevelMessage2]
GO

ALTER TABLE [dbo].[BankHeistSetting] ADD  DEFAULT ('Oh yeah! With this crew, we can now hit the @bankname@. Lets see if we can get a bigger crew to hit the @nextbankname@!') FOR [NextLevelMessage3]
GO

ALTER TABLE [dbo].[BankHeistSetting] ADD  DEFAULT ('Hell yeah! We can now hit the @bankname@. A few more, and we could hit the @nextbankname@!') FOR [NextLevelMessage4]
GO

ALTER TABLE [dbo].[BankHeistSetting] ADD  DEFAULT ('Epic crew! We are going to hit the @bankname@ guys! Gear up and get ready to head out.') FOR [NextLevelMessage5]
GO

ALTER TABLE [dbo].[BankHeistSetting] ADD  DEFAULT ('Alright guys, check your guns. We are storming into the @bankname@ through all entrances. Let''s get the cash and get out before the cops get here.') FOR [GameStart]
GO

ALTER TABLE [dbo].[BankHeistSetting] ADD  DEFAULT ('@user@ executed the heist flawlessly, sneaking into the @bankname@ through the back entrance and looting @winamount@ @pointsname@ from the vault.') FOR [SingleUserSuccess]
GO

ALTER TABLE [dbo].[BankHeistSetting] ADD  DEFAULT ('Local security caught @user@ trying to sneak into the @bankname@ through the back entrance and opened fire.') FOR [SingleUserFail]
GO

ALTER TABLE [dbo].[BankHeistSetting] ADD  DEFAULT ('The heist payouts are:') FOR [ResultsMessage]
GO

ALTER TABLE [dbo].[BankHeistSetting] ADD  DEFAULT ('The execution was flawless, in and out before the first cop arrived on scene.') FOR [Success100]
GO

ALTER TABLE [dbo].[BankHeistSetting] ADD  DEFAULT ('The crew suffered a few losses engaging the local security team.') FOR [Success34]
GO

ALTER TABLE [dbo].[BankHeistSetting] ADD  DEFAULT ('The crew suffered major losses as they engaged the SWAT backup.') FOR [Success1]
GO

ALTER TABLE [dbo].[BankHeistSetting] ADD  DEFAULT ('SWAT teams nearby stormed the bank and killed the entire crew. Not a single soul survived…') FOR [Success0]
GO

ALTER TABLE [dbo].[BankHeistSetting] ADD  DEFAULT ('Simple Municipal Bank') FOR [LevelName1]
GO

ALTER TABLE [dbo].[BankHeistSetting] ADD  DEFAULT ((9)) FOR [LevelMaxUsers1]
GO

ALTER TABLE [dbo].[BankHeistSetting] ADD  DEFAULT ('Simple City Bank') FOR [LevelName2]
GO

ALTER TABLE [dbo].[BankHeistSetting] ADD  DEFAULT ((19)) FOR [LevelMaxUsers2]
GO

ALTER TABLE [dbo].[BankHeistSetting] ADD  DEFAULT ('Simple State Bank') FOR [LevelName3]
GO

ALTER TABLE [dbo].[BankHeistSetting] ADD  DEFAULT ((29)) FOR [LevelMaxUsers3]
GO

ALTER TABLE [dbo].[BankHeistSetting] ADD  DEFAULT ('Simple National Reserve') FOR [LevelName4]
GO

ALTER TABLE [dbo].[BankHeistSetting] ADD  DEFAULT ((39)) FOR [LevelMaxUsers4]
GO

ALTER TABLE [dbo].[BankHeistSetting] ADD  DEFAULT ('Simple Federal Reserve') FOR [LevelName5]
GO

ALTER TABLE [dbo].[BankHeistSetting] ADD  DEFAULT ((9001)) FOR [LevelMaxUsers5]
GO

ALTER TABLE [dbo].[BankHeistSetting] ADD  DEFAULT ((54.00)) FOR [PayoutSuccessRate1]
GO

ALTER TABLE [dbo].[BankHeistSetting] ADD  DEFAULT ((1.50)) FOR [PayoutMultiplier1]
GO

ALTER TABLE [dbo].[BankHeistSetting] ADD  DEFAULT ((48.80)) FOR [PayoutSuccessRate2]
GO

ALTER TABLE [dbo].[BankHeistSetting] ADD  DEFAULT ((1.70)) FOR [PayoutMultiplier2]
GO

ALTER TABLE [dbo].[BankHeistSetting] ADD  DEFAULT ((42.50)) FOR [PayoutSuccessRate3]
GO

ALTER TABLE [dbo].[BankHeistSetting] ADD  DEFAULT ((2.00)) FOR [PayoutMultiplier3]
GO

ALTER TABLE [dbo].[BankHeistSetting] ADD  DEFAULT ((38.70)) FOR [PayoutSuccessRate4]
GO

ALTER TABLE [dbo].[BankHeistSetting] ADD  DEFAULT ((2.25)) FOR [PayoutMultiplier4]
GO

ALTER TABLE [dbo].[BankHeistSetting] ADD  DEFAULT ((32.40)) FOR [PayoutSuccessRate5]
GO

ALTER TABLE [dbo].[BankHeistSetting] ADD  DEFAULT ((2.75)) FOR [PayoutMultiplier5]
GO

ALTER TABLE [dbo].[BossFightBossStats] ADD  CONSTRAINT [DF__BossFight__name1__37FA4C37]  DEFAULT ('Boss 1') FOR [Name1]
GO

ALTER TABLE [dbo].[BossFightBossStats] ADD  CONSTRAINT [DF__BossFight__maxUs__38EE7070]  DEFAULT ((9)) FOR [MaxUsers1]
GO

ALTER TABLE [dbo].[BossFightBossStats] ADD  CONSTRAINT [DF__BossFight__attac__39E294A9]  DEFAULT ((15)) FOR [Attack1]
GO

ALTER TABLE [dbo].[BossFightBossStats] ADD  CONSTRAINT [DF__BossFight__defen__3AD6B8E2]  DEFAULT ((0)) FOR [Defense1]
GO

ALTER TABLE [dbo].[BossFightBossStats] ADD  CONSTRAINT [DF__BossFight__evasi__3BCADD1B]  DEFAULT ((5)) FOR [Evasion1]
GO

ALTER TABLE [dbo].[BossFightBossStats] ADD  CONSTRAINT [DF__BossFight__healt__3CBF0154]  DEFAULT ((200)) FOR [Health1]
GO

ALTER TABLE [dbo].[BossFightBossStats] ADD  CONSTRAINT [DF__BossFight__turnL__3DB3258D]  DEFAULT ((20)) FOR [TurnLimit1]
GO

ALTER TABLE [dbo].[BossFightBossStats] ADD  CONSTRAINT [DF__BossFight__loot1__3EA749C6]  DEFAULT ((300)) FOR [Loot1]
GO

ALTER TABLE [dbo].[BossFightBossStats] ADD  CONSTRAINT [DF__BossFight__lastA__3F9B6DFF]  DEFAULT ((150)) FOR [LastAttackBonus1]
GO

ALTER TABLE [dbo].[BossFightBossStats] ADD  CONSTRAINT [DF__BossFight__name2__408F9238]  DEFAULT ('Boss 2') FOR [Name2]
GO

ALTER TABLE [dbo].[BossFightBossStats] ADD  CONSTRAINT [DF__BossFight__maxUs__4183B671]  DEFAULT ((19)) FOR [MaxUsers2]
GO

ALTER TABLE [dbo].[BossFightBossStats] ADD  CONSTRAINT [DF__BossFight__attac__4277DAAA]  DEFAULT ((25)) FOR [Attack2]
GO

ALTER TABLE [dbo].[BossFightBossStats] ADD  CONSTRAINT [DF__BossFight__defen__436BFEE3]  DEFAULT ((10)) FOR [Defense2]
GO

ALTER TABLE [dbo].[BossFightBossStats] ADD  CONSTRAINT [DF__BossFight__evasi__4460231C]  DEFAULT ((15)) FOR [Evasion2]
GO

ALTER TABLE [dbo].[BossFightBossStats] ADD  CONSTRAINT [DF__BossFight__healt__45544755]  DEFAULT ((750)) FOR [Health2]
GO

ALTER TABLE [dbo].[BossFightBossStats] ADD  CONSTRAINT [DF__BossFight__turnL__46486B8E]  DEFAULT ((20)) FOR [TurnLimit2]
GO

ALTER TABLE [dbo].[BossFightBossStats] ADD  CONSTRAINT [DF__BossFight__loot2__473C8FC7]  DEFAULT ((750)) FOR [Loot2]
GO

ALTER TABLE [dbo].[BossFightBossStats] ADD  CONSTRAINT [DF__BossFight__lastA__4830B400]  DEFAULT ((300)) FOR [LastAttackBonus2]
GO

ALTER TABLE [dbo].[BossFightBossStats] ADD  CONSTRAINT [DF__BossFight__name3__4924D839]  DEFAULT ('Boss 3') FOR [Name3]
GO

ALTER TABLE [dbo].[BossFightBossStats] ADD  CONSTRAINT [DF__BossFight__maxUs__4A18FC72]  DEFAULT ((29)) FOR [MaxUsers3]
GO

ALTER TABLE [dbo].[BossFightBossStats] ADD  CONSTRAINT [DF__BossFight__attac__4B0D20AB]  DEFAULT ((35)) FOR [Attack3]
GO

ALTER TABLE [dbo].[BossFightBossStats] ADD  CONSTRAINT [DF__BossFight__defen__4C0144E4]  DEFAULT ((20)) FOR [Defense3]
GO

ALTER TABLE [dbo].[BossFightBossStats] ADD  CONSTRAINT [DF__BossFight__evasi__4CF5691D]  DEFAULT ((20)) FOR [Evasion3]
GO

ALTER TABLE [dbo].[BossFightBossStats] ADD  CONSTRAINT [DF__BossFight__healt__4DE98D56]  DEFAULT ((1500)) FOR [Health3]
GO

ALTER TABLE [dbo].[BossFightBossStats] ADD  CONSTRAINT [DF__BossFight__turnL__4EDDB18F]  DEFAULT ((20)) FOR [TurnLimit3]
GO

ALTER TABLE [dbo].[BossFightBossStats] ADD  CONSTRAINT [DF__BossFight__loot3__4FD1D5C8]  DEFAULT ((2000)) FOR [Loot3]
GO

ALTER TABLE [dbo].[BossFightBossStats] ADD  CONSTRAINT [DF__BossFight__lastA__50C5FA01]  DEFAULT ((600)) FOR [LastAttackBonus3]
GO

ALTER TABLE [dbo].[BossFightBossStats] ADD  CONSTRAINT [DF__BossFight__name4__51BA1E3A]  DEFAULT ('Boss 4') FOR [Name4]
GO

ALTER TABLE [dbo].[BossFightBossStats] ADD  CONSTRAINT [DF__BossFight__maxUs__52AE4273]  DEFAULT ((39)) FOR [MaxUsers4]
GO

ALTER TABLE [dbo].[BossFightBossStats] ADD  CONSTRAINT [DF__BossFight__attac__53A266AC]  DEFAULT ((40)) FOR [Attack4]
GO

ALTER TABLE [dbo].[BossFightBossStats] ADD  CONSTRAINT [DF__BossFight__defen__54968AE5]  DEFAULT ((25)) FOR [Defense4]
GO

ALTER TABLE [dbo].[BossFightBossStats] ADD  CONSTRAINT [DF__BossFight__evasi__558AAF1E]  DEFAULT ((25)) FOR [Evasion4]
GO

ALTER TABLE [dbo].[BossFightBossStats] ADD  CONSTRAINT [DF__BossFight__healt__567ED357]  DEFAULT ((3000)) FOR [Health4]
GO

ALTER TABLE [dbo].[BossFightBossStats] ADD  CONSTRAINT [DF__BossFight__turnL__5772F790]  DEFAULT ((20)) FOR [TurnLimit4]
GO

ALTER TABLE [dbo].[BossFightBossStats] ADD  CONSTRAINT [DF__BossFight__loot4__58671BC9]  DEFAULT ((5000)) FOR [Loot4]
GO

ALTER TABLE [dbo].[BossFightBossStats] ADD  CONSTRAINT [DF__BossFight__lastA__595B4002]  DEFAULT ((1000)) FOR [LastAttackBonus4]
GO

ALTER TABLE [dbo].[BossFightBossStats] ADD  CONSTRAINT [DF__BossFight__name5__5A4F643B]  DEFAULT ('Boss 5') FOR [Name5]
GO

ALTER TABLE [dbo].[BossFightBossStats] ADD  CONSTRAINT [DF__BossFight__maxUs__5B438874]  DEFAULT ((49)) FOR [MaxUsers5]
GO

ALTER TABLE [dbo].[BossFightBossStats] ADD  CONSTRAINT [DF__BossFight__attac__5C37ACAD]  DEFAULT ((50)) FOR [Attack5]
GO

ALTER TABLE [dbo].[BossFightBossStats] ADD  CONSTRAINT [DF__BossFight__defen__5D2BD0E6]  DEFAULT ((30)) FOR [Defense5]
GO

ALTER TABLE [dbo].[BossFightBossStats] ADD  CONSTRAINT [DF__BossFight__evasi__5E1FF51F]  DEFAULT ((35)) FOR [Evasion5]
GO

ALTER TABLE [dbo].[BossFightBossStats] ADD  CONSTRAINT [DF__BossFight__healt__5F141958]  DEFAULT ((5000)) FOR [Health5]
GO

ALTER TABLE [dbo].[BossFightBossStats] ADD  CONSTRAINT [DF__BossFight__turnL__60083D91]  DEFAULT ((20)) FOR [TurnLimit5]
GO

ALTER TABLE [dbo].[BossFightBossStats] ADD  CONSTRAINT [DF__BossFight__loot5__60FC61CA]  DEFAULT ((10000)) FOR [Loot5]
GO

ALTER TABLE [dbo].[BossFightBossStats] ADD  CONSTRAINT [DF__BossFight__lastA__61F08603]  DEFAULT ((2500)) FOR [LastAttackBonus5]
GO

ALTER TABLE [dbo].[BossFightClassStats] ADD  DEFAULT ((10)) FOR [ViewerAttack]
GO

ALTER TABLE [dbo].[BossFightClassStats] ADD  DEFAULT ((5)) FOR [ViewerDefense]
GO

ALTER TABLE [dbo].[BossFightClassStats] ADD  DEFAULT ((10)) FOR [ViewerEvasion]
GO

ALTER TABLE [dbo].[BossFightClassStats] ADD  DEFAULT ((100)) FOR [ViewerHealth]
GO

ALTER TABLE [dbo].[BossFightClassStats] ADD  DEFAULT ((15)) FOR [FollowerAttack]
GO

ALTER TABLE [dbo].[BossFightClassStats] ADD  DEFAULT ((7)) FOR [FollowerDefense]
GO

ALTER TABLE [dbo].[BossFightClassStats] ADD  DEFAULT ((15)) FOR [FollowerEvasion]
GO

ALTER TABLE [dbo].[BossFightClassStats] ADD  DEFAULT ((125)) FOR [FollowerHealth]
GO

ALTER TABLE [dbo].[BossFightClassStats] ADD  DEFAULT ((20)) FOR [RegularAttack]
GO

ALTER TABLE [dbo].[BossFightClassStats] ADD  DEFAULT ((10)) FOR [RegularDefense]
GO

ALTER TABLE [dbo].[BossFightClassStats] ADD  DEFAULT ((20)) FOR [RegularEvasion]
GO

ALTER TABLE [dbo].[BossFightClassStats] ADD  DEFAULT ((175)) FOR [RegularHealth]
GO

ALTER TABLE [dbo].[BossFightClassStats] ADD  DEFAULT ((35)) FOR [ModeratorAttack]
GO

ALTER TABLE [dbo].[BossFightClassStats] ADD  DEFAULT ((20)) FOR [ModeratorDefense]
GO

ALTER TABLE [dbo].[BossFightClassStats] ADD  DEFAULT ((35)) FOR [ModeratorEvasion]
GO

ALTER TABLE [dbo].[BossFightClassStats] ADD  DEFAULT ((225)) FOR [ModeratorHealth]
GO

ALTER TABLE [dbo].[BossFightClassStats] ADD  DEFAULT ((25)) FOR [SubscriberAttack]
GO

ALTER TABLE [dbo].[BossFightClassStats] ADD  DEFAULT ((15)) FOR [SubscriberDefense]
GO

ALTER TABLE [dbo].[BossFightClassStats] ADD  DEFAULT ((25)) FOR [SubscriberEvasion]
GO

ALTER TABLE [dbo].[BossFightClassStats] ADD  DEFAULT ((200)) FOR [SubscriberHealth]
GO

ALTER TABLE [dbo].[BossFightSetting] ADD  DEFAULT ((10)) FOR [CooldownPeriodMin]
GO

ALTER TABLE [dbo].[BossFightSetting] ADD  DEFAULT ((60)) FOR [EntryPeriodSec]
GO

ALTER TABLE [dbo].[BossFightSetting] ADD  DEFAULT ('@user@ is trying to get a group of adventurers ready to fight a boss... Will you join them? Type !raid to join!') FOR [EntryMessage]
GO

ALTER TABLE [dbo].[BossFightSetting] ADD  DEFAULT ((100)) FOR [Cost]
GO

ALTER TABLE [dbo].[BossFightSetting] ADD  DEFAULT ('The boss floor is currently being cleaned up and won''t be available for at least @timeleft@ minutes') FOR [CooldownEntry]
GO

ALTER TABLE [dbo].[BossFightSetting] ADD  DEFAULT ('The boss floor has been cleaned up... Want to go again?! Type !raid to start!') FOR [CooldownOver]
GO

ALTER TABLE [dbo].[BossFightSetting] ADD  DEFAULT ('With this raid party, we can now attack @bossname@. Let''s see if we can get a bigger party to attack @nextbossname@!') FOR [NextLevelMessage2]
GO

ALTER TABLE [dbo].[BossFightSetting] ADD  DEFAULT ('Oh yeah! With this raid party, we can now attack @bossname@. Let''s see if we can get a bigger party to attack @nextbossname@!') FOR [NextLevelMessage3]
GO

ALTER TABLE [dbo].[BossFightSetting] ADD  DEFAULT ('Hell yeah! We can now attack @bossname@. A few more and we could attack @nextbossname@!') FOR [NextLevelMessage4]
GO

ALTER TABLE [dbo].[BossFightSetting] ADD  DEFAULT ('Epic raid party! We are going to attack @bossname@ guys! Gear up and get ready to head out.') FOR [NextLevelMessage5]
GO

ALTER TABLE [dbo].[BossFightSetting] ADD  DEFAULT ('The combatants have stepped into the boss room... Will they be able to defeat @bossname@?!') FOR [GameStart]
GO

ALTER TABLE [dbo].[BossFightSetting] ADD  DEFAULT ('The survivors are:') FOR [ResultsMessage]
GO

ALTER TABLE [dbo].[BossFightSetting] ADD  DEFAULT ('@user@ executed the raid flawlessly, soloing @bossname@ with ease and looting @winamount@ @pointsname@ with a last attack bonus of @lastattackbonus@ @pointsname@.') FOR [SingleUserSuccess]
GO

ALTER TABLE [dbo].[BossFightSetting] ADD  DEFAULT ('@user@ thought they could try to solo @bossname@, but was deleted immediately...RIP') FOR [SingleUserFail]
GO

ALTER TABLE [dbo].[BossFightSetting] ADD  DEFAULT ('The raid was a complete success. No one fell into the hands of @bossname@.') FOR [Success100]
GO

ALTER TABLE [dbo].[BossFightSetting] ADD  DEFAULT ('The raid party suffered a few casualties as they fought valiantly.') FOR [Success34]
GO

ALTER TABLE [dbo].[BossFightSetting] ADD  DEFAULT ('The raid party suffered major casualties as they fought valiantly.') FOR [Success1]
GO

ALTER TABLE [dbo].[BossFightSetting] ADD  DEFAULT ('It was absolute hell. The field is covered with the blood of the fallen...no one survived') FOR [Success0]
GO

ALTER TABLE [dbo].[BotTimeout] ADD  CONSTRAINT [DF__tblTimeou__timeA__66603565]  DEFAULT (getdate()) FOR [TimeAdded]
GO

ALTER TABLE [dbo].[Broadcaster] ADD  CONSTRAINT [DF_tblBroadcasters_twitchId]  DEFAULT ((0)) FOR [TwitchId]
GO

ALTER TABLE [dbo].[Broadcaster] ADD  CONSTRAINT [DF__Table__timeAdded__5812160E]  DEFAULT (getdate()) FOR [LastUpdated]
GO

ALTER TABLE [dbo].[PartyUpRequest] ADD  CONSTRAINT [DF__tblGroupU__timeR__4BAC3F29]  DEFAULT (getdate()) FOR [TimeRequested]
GO

ALTER TABLE [dbo].[Quote] ADD  DEFAULT (getdate()) FOR [TimeCreated]
GO

ALTER TABLE [dbo].[Reminder] ADD  DEFAULT ((0)) FOR [Sunday]
GO

ALTER TABLE [dbo].[Reminder] ADD  DEFAULT ((0)) FOR [Monday]
GO

ALTER TABLE [dbo].[Reminder] ADD  DEFAULT ((0)) FOR [Tuesday]
GO

ALTER TABLE [dbo].[Reminder] ADD  DEFAULT ((0)) FOR [Wednesday]
GO

ALTER TABLE [dbo].[Reminder] ADD  DEFAULT ((0)) FOR [Thursday]
GO

ALTER TABLE [dbo].[Reminder] ADD  DEFAULT ((0)) FOR [Friday]
GO

ALTER TABLE [dbo].[Reminder] ADD  DEFAULT ((0)) FOR [Saturday]
GO

ALTER TABLE [dbo].[Reminder] ADD  DEFAULT ((0)) FOR [IsCountdownEvent]
GO

ALTER TABLE [dbo].[Reminder] ADD  DEFAULT ((0)) FOR [HasCountdownTicker]
GO

ALTER TABLE [dbo].[TwitchGameCategory] ADD  CONSTRAINT [DF_tblGameList_multiplayer]  DEFAULT ((0)) FOR [Multiplayer]
GO

ALTER TABLE [dbo].[Bank]  WITH CHECK ADD  CONSTRAINT [FK_Bank_Broadcaster] FOREIGN KEY([Broadcaster])
REFERENCES [dbo].[Broadcaster] ([Id])
GO

ALTER TABLE [dbo].[Bank] CHECK CONSTRAINT [FK_Bank_Broadcaster]
GO

ALTER TABLE [dbo].[BankHeistSetting]  WITH CHECK ADD  CONSTRAINT [FK_BankHeistSetting_Broadcaster] FOREIGN KEY([Broadcaster])
REFERENCES [dbo].[Broadcaster] ([Id])
GO

ALTER TABLE [dbo].[BankHeistSetting] CHECK CONSTRAINT [FK_BankHeistSetting_Broadcaster]
GO

ALTER TABLE [dbo].[BossFightBossStats]  WITH CHECK ADD  CONSTRAINT [FK_BossFightBossStats_Broadcaster] FOREIGN KEY([SettingsId])
REFERENCES [dbo].[BossFightSetting] ([Id])
GO

ALTER TABLE [dbo].[BossFightBossStats] CHECK CONSTRAINT [FK_BossFightBossStats_Broadcaster]
GO

ALTER TABLE [dbo].[BossFightBossStats]  WITH CHECK ADD  CONSTRAINT [FK_BossFightBossStats_TwitchGameCategory] FOREIGN KEY([GameId])
REFERENCES [dbo].[TwitchGameCategory] ([Id])
GO

ALTER TABLE [dbo].[BossFightBossStats] CHECK CONSTRAINT [FK_BossFightBossStats_TwitchGameCategory]
GO

ALTER TABLE [dbo].[BossFightClassStats]  WITH CHECK ADD  CONSTRAINT [FK_BossFightClassStats_Broadcaster] FOREIGN KEY([SettingsId])
REFERENCES [dbo].[BossFightSetting] ([Id])
GO

ALTER TABLE [dbo].[BossFightClassStats] CHECK CONSTRAINT [FK_BossFightClassStats_Broadcaster]
GO

ALTER TABLE [dbo].[BossFightSetting]  WITH CHECK ADD  CONSTRAINT [FK_BossFightSetting_Broadcaster] FOREIGN KEY([Broadcaster])
REFERENCES [dbo].[Broadcaster] ([Id])
GO

ALTER TABLE [dbo].[BossFightSetting] CHECK CONSTRAINT [FK_BossFightSetting_Broadcaster]
GO

ALTER TABLE [dbo].[BotTimeout]  WITH CHECK ADD  CONSTRAINT [FK_BotTimeout_Broadcaster] FOREIGN KEY([Broadcaster])
REFERENCES [dbo].[Broadcaster] ([Id])
GO

ALTER TABLE [dbo].[BotTimeout] CHECK CONSTRAINT [FK_BotTimeout_Broadcaster]
GO

ALTER TABLE [dbo].[ErrorLog]  WITH CHECK ADD  CONSTRAINT [FK_ErrorLog_Broadcaster] FOREIGN KEY([Broadcaster])
REFERENCES [dbo].[Broadcaster] ([Id])
GO

ALTER TABLE [dbo].[ErrorLog] CHECK CONSTRAINT [FK_ErrorLog_Broadcaster]
GO

ALTER TABLE [dbo].[PartyUp]  WITH CHECK ADD  CONSTRAINT [FK_PartyUp_Broadcaster] FOREIGN KEY([Broadcaster])
REFERENCES [dbo].[Broadcaster] ([Id])
GO

ALTER TABLE [dbo].[PartyUp] CHECK CONSTRAINT [FK_PartyUp_Broadcaster]
GO

ALTER TABLE [dbo].[PartyUp]  WITH CHECK ADD  CONSTRAINT [FK_PartyUp_TwitchGameCategory] FOREIGN KEY([GameId])
REFERENCES [dbo].[TwitchGameCategory] ([Id])
GO

ALTER TABLE [dbo].[PartyUp] CHECK CONSTRAINT [FK_PartyUp_TwitchGameCategory]
GO

ALTER TABLE [dbo].[PartyUpRequest]  WITH CHECK ADD  CONSTRAINT [FK_PartyUpRequest_PartyUp] FOREIGN KEY([PartyMember])
REFERENCES [dbo].[PartyUp] ([Id])
GO

ALTER TABLE [dbo].[PartyUpRequest] CHECK CONSTRAINT [FK_PartyUpRequest_PartyUp]
GO

ALTER TABLE [dbo].[PartyUpRequest]  WITH CHECK ADD  CONSTRAINT [FK_PartyUpRequest_PartyUpRequest] FOREIGN KEY([Id])
REFERENCES [dbo].[PartyUpRequest] ([Id])
GO

ALTER TABLE [dbo].[PartyUpRequest] CHECK CONSTRAINT [FK_PartyUpRequest_PartyUpRequest]
GO

ALTER TABLE [dbo].[Quote]  WITH CHECK ADD  CONSTRAINT [FK_Quote_Broadcasters] FOREIGN KEY([Broadcaster])
REFERENCES [dbo].[Broadcaster] ([Id])
GO

ALTER TABLE [dbo].[Quote] CHECK CONSTRAINT [FK_Quote_Broadcasters]
GO

ALTER TABLE [dbo].[Rank]  WITH CHECK ADD  CONSTRAINT [FK_Rank_Broadcaster] FOREIGN KEY([Broadcaster])
REFERENCES [dbo].[Broadcaster] ([Id])
GO

ALTER TABLE [dbo].[Rank] CHECK CONSTRAINT [FK_Rank_Broadcaster]
GO

ALTER TABLE [dbo].[RankFollower]  WITH CHECK ADD  CONSTRAINT [FK_RankFollower_Broadcaster] FOREIGN KEY([Broadcaster])
REFERENCES [dbo].[Broadcaster] ([Id])
GO

ALTER TABLE [dbo].[RankFollower] CHECK CONSTRAINT [FK_RankFollower_Broadcaster]
GO

ALTER TABLE [dbo].[Reminder]  WITH CHECK ADD  CONSTRAINT [FK_Reminder_Broadcaster] FOREIGN KEY([Broadcaster])
REFERENCES [dbo].[Broadcaster] ([Id])
GO

ALTER TABLE [dbo].[Reminder] CHECK CONSTRAINT [FK_Reminder_Broadcaster]
GO

ALTER TABLE [dbo].[Reminder]  WITH CHECK ADD  CONSTRAINT [FK_Reminder_TwitchGameCategory] FOREIGN KEY([GameId])
REFERENCES [dbo].[TwitchGameCategory] ([Id])
GO

ALTER TABLE [dbo].[Reminder] CHECK CONSTRAINT [FK_Reminder_TwitchGameCategory]
GO

ALTER TABLE [dbo].[SongRequest]  WITH CHECK ADD  CONSTRAINT [FK_SongRequest_Broadcaster] FOREIGN KEY([Broadcaster])
REFERENCES [dbo].[Broadcaster] ([Id])
GO

ALTER TABLE [dbo].[SongRequest] CHECK CONSTRAINT [FK_SongRequest_Broadcaster]
GO

ALTER TABLE [dbo].[SongRequestIgnore]  WITH CHECK ADD  CONSTRAINT [FK_SongRequestIgnore_Broadcaster] FOREIGN KEY([Broadcaster])
REFERENCES [dbo].[Broadcaster] ([Id])
GO

ALTER TABLE [dbo].[SongRequestIgnore] CHECK CONSTRAINT [FK_SongRequestIgnore_Broadcaster]
GO


