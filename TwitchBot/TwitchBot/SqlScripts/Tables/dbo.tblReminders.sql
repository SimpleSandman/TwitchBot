USE [twitchbotdb]
GO

/****** Object: Table [dbo].[tblReminders] Script Date: 9/28/2017 11:05:21 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[tblReminders] (
    [Id]                 INT           IDENTITY (1, 1) NOT NULL,
    [sunday]             BIT           DEFAULT ((0)) NOT NULL,
    [monday]             BIT           DEFAULT ((0)) NOT NULL,
    [tuesday]            BIT           DEFAULT ((0)) NOT NULL,
    [wednesday]          BIT           DEFAULT ((0)) NOT NULL,
    [thursday]           BIT           DEFAULT ((0)) NOT NULL,
    [friday]             BIT           DEFAULT ((0)) NOT NULL,
    [saturday]           BIT           DEFAULT ((0)) NOT NULL,
    [reminderSec1]       INT           NULL,
    [reminderSec2]       INT           NULL,
    [reminderSec3]       INT           NULL,
    [reminderSec4]       INT           NULL,
    [reminderSec5]       INT           NULL,
    [remindEveryMin]     INT           NULL,
    [timeOfEventUtc]     TIME (7)      NULL,
    [expirationDateUtc]  DATETIME      DEFAULT (getdate()) NULL,
    [isCountdownEvent]   BIT           DEFAULT ((0)) NOT NULL,
    [hasCountdownTicker] BIT           DEFAULT ((0)) NOT NULL,
    [message]            VARCHAR (150) NOT NULL,
    [broadcaster]        INT           NOT NULL,
    [game]               INT           NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tblReminders_tblGameList] FOREIGN KEY ([game]) REFERENCES [dbo].[tblGameList] ([Id]),
    CONSTRAINT [FK_tblReminders_tblBroadcasters] FOREIGN KEY ([broadcaster]) REFERENCES [dbo].[tblBroadcasters] ([Id])
);
