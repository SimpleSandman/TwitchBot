USE [twitchbotdb]
GO

/****** Object: Table [dbo].[BossFightBossStats] Script Date: 12/24/2017 1:10:25 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[BossFightBossStats] (
    [Id]               INT          IDENTITY (1, 1) NOT NULL,
    [settingsId]       INT          NOT NULL,
    [name1]            VARCHAR (50) DEFAULT ('Boss1') NOT NULL,
    [maxUsers1]        INT          DEFAULT ((9)) NOT NULL,
    [attack1]          INT          DEFAULT ((15)) NOT NULL,
    [defense1]         INT          DEFAULT ((0)) NOT NULL,
    [evasion1]         INT          DEFAULT ((5)) NOT NULL,
    [health1]          INT          DEFAULT ((100)) NOT NULL,
    [turnLimit1]       INT          DEFAULT ((20)) NOT NULL,
    [loot1]            INT          DEFAULT ((50)) NOT NULL,
    [lastAttackBonus1] INT          DEFAULT ((100)) NOT NULL,
    [name2]            VARCHAR (50) DEFAULT ('Boss2') NOT NULL,
    [maxUsers2]        INT          DEFAULT ((19)) NOT NULL,
    [attack2]          INT          DEFAULT ((15)) NOT NULL,
    [defense2]         INT          DEFAULT ((0)) NOT NULL,
    [evasion2]         INT          DEFAULT ((5)) NOT NULL,
    [health2]          INT          DEFAULT ((100)) NOT NULL,
    [turnLimit2]       INT          DEFAULT ((20)) NOT NULL,
    [loot2]            INT          DEFAULT ((50)) NOT NULL,
    [lastAttackBonus2] INT          DEFAULT ((100)) NOT NULL,
    [name3]            VARCHAR (50) DEFAULT ('Boss3') NOT NULL,
    [maxUsers3]        INT          DEFAULT ((29)) NOT NULL,
    [attack3]          INT          DEFAULT ((15)) NOT NULL,
    [defense3]         INT          DEFAULT ((0)) NOT NULL,
    [evasion3]         INT          DEFAULT ((5)) NOT NULL,
    [health3]          INT          DEFAULT ((100)) NOT NULL,
    [turnLimit3]       INT          DEFAULT ((20)) NOT NULL,
    [loot3]            INT          DEFAULT ((50)) NOT NULL,
    [lastAttackBonus3] INT          DEFAULT ((100)) NOT NULL,
    [name4]            VARCHAR (50) DEFAULT ('Boss4') NOT NULL,
    [maxUsers4]        INT          DEFAULT ((39)) NOT NULL,
    [attack4]          INT          DEFAULT ((15)) NOT NULL,
    [defense4]         INT          DEFAULT ((0)) NOT NULL,
    [evasion4]         INT          DEFAULT ((5)) NOT NULL,
    [health4]          INT          DEFAULT ((100)) NOT NULL,
    [turnLimit4]       INT          DEFAULT ((20)) NOT NULL,
    [loot4]            INT          DEFAULT ((50)) NOT NULL,
    [lastAttackBonus4] INT          DEFAULT ((100)) NOT NULL,
    [name5]            VARCHAR (50) DEFAULT ('Boss5') NOT NULL,
    [maxUsers5]        INT          DEFAULT ((49)) NOT NULL,
    [attack5]          INT          DEFAULT ((15)) NOT NULL,
    [defense5]         INT          DEFAULT ((0)) NOT NULL,
    [evasion5]         INT          DEFAULT ((5)) NOT NULL,
    [health5]          INT          DEFAULT ((100)) NOT NULL,
    [turnLimit5]       INT          DEFAULT ((20)) NOT NULL,
    [loot5]            INT          DEFAULT ((50)) NOT NULL,
    [lastAttackBonus5] INT          DEFAULT ((100)) NOT NULL,
    [gameId]           INT          NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tblBossFightBossStats] FOREIGN KEY ([gameId]) REFERENCES [dbo].[GameList] ([Id]),
    CONSTRAINT [FK_tblBossFightBossStats_tblBroadcaster] FOREIGN KEY ([settingsId]) REFERENCES [dbo].[BossFightSettings] ([Id])
);

