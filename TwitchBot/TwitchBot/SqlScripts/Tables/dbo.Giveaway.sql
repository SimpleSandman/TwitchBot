USE [twitchbotdb]
GO

/****** Object: Table [dbo].[tblGiveaway] Script Date: 1/30/2017 9:53:00 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Giveaway] (
    [Id]          INT          IDENTITY (1, 1) NOT NULL,
    [dueDate]     DATETIME     NOT NULL,
    [message]     VARCHAR (75) NOT NULL,
    [broadcaster] INT          NOT NULL,
    [elgMod]      BIT          NOT NULL,
    [elgReg]      BIT          NOT NULL,
    [elgSub]      BIT          NOT NULL,
    [elgUsr]      BIT          NOT NULL,
    [giveType]    INT          NOT NULL,
    [giveParam1]  VARCHAR (50) NOT NULL,
    [giveParam2]  VARCHAR (50) NULL
);

