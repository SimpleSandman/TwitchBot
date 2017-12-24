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
    [followerAttack]    INT DEFAULT ((10)) NOT NULL,
    [followerDefense]   INT DEFAULT ((5)) NOT NULL,
    [followerEvasion]   INT DEFAULT ((10)) NOT NULL,
    [followerHealth]    INT DEFAULT ((100)) NOT NULL,
    [regularAttack]     INT DEFAULT ((10)) NOT NULL,
    [regularDefense]    INT DEFAULT ((5)) NOT NULL,
    [regularEvasion]    INT DEFAULT ((10)) NOT NULL,
    [regularHealth]     INT DEFAULT ((100)) NOT NULL,
    [moderatorAttack]   INT DEFAULT ((10)) NOT NULL,
    [moderatorDefense]  INT DEFAULT ((5)) NOT NULL,
    [moderatorEvasion]  INT DEFAULT ((10)) NOT NULL,
    [moderatorHealth]   INT DEFAULT ((100)) NOT NULL,
    [subscriberAttack]  INT DEFAULT ((10)) NOT NULL,
    [subscriberDefense] INT DEFAULT ((5)) NOT NULL,
    [subscriberEvasion] INT DEFAULT ((10)) NOT NULL,
    [subscriberHealth]  INT DEFAULT ((100)) NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tblBossFightClassStats_tblBroadcaster] FOREIGN KEY ([settingsId]) REFERENCES [dbo].[BossFightSettings] ([Id])
);

