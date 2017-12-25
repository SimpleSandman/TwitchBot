USE [twitchbotdb]
GO

/****** Object: Table [dbo].[BossFightClassStats] Script Date: 12/24/2017 1:11:16 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[BossFightClassStats] (
    [Id]                INT IDENTITY (1, 1) NOT NULL,
    [settingsId]        INT NOT NULL,
    [viewerAttack]      INT DEFAULT ((10)) NOT NULL,
    [viewerDefense]     INT DEFAULT ((5)) NOT NULL,
    [viewerEvasion]     INT DEFAULT ((10)) NOT NULL,
    [viewerHealth]      INT DEFAULT ((100)) NOT NULL,
    [followerAttack]    INT DEFAULT ((15)) NOT NULL,
    [followerDefense]   INT DEFAULT ((7)) NOT NULL,
    [followerEvasion]   INT DEFAULT ((15)) NOT NULL,
    [followerHealth]    INT DEFAULT ((125)) NOT NULL,
    [regularAttack]     INT DEFAULT ((20)) NOT NULL,
    [regularDefense]    INT DEFAULT ((10)) NOT NULL,
    [regularEvasion]    INT DEFAULT ((20)) NOT NULL,
    [regularHealth]     INT DEFAULT ((175)) NOT NULL,
    [moderatorAttack]   INT DEFAULT ((35)) NOT NULL,
    [moderatorDefense]  INT DEFAULT ((20)) NOT NULL,
    [moderatorEvasion]  INT DEFAULT ((35)) NOT NULL,
    [moderatorHealth]   INT DEFAULT ((225)) NOT NULL,
    [subscriberAttack]  INT DEFAULT ((25)) NOT NULL,
    [subscriberDefense] INT DEFAULT ((15)) NOT NULL,
    [subscriberEvasion] INT DEFAULT ((25)) NOT NULL,
    [subscriberHealth]  INT DEFAULT ((200)) NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tblBossFightClassStats_tblBroadcaster] FOREIGN KEY ([settingsId]) REFERENCES [dbo].[BossFightSettings] ([Id])
);

