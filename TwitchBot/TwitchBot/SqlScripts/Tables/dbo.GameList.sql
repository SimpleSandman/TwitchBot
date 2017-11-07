USE [twitchbotdb]
GO

/****** Object: Table [dbo].[tblGameList] Script Date: 9/8/2016 2:09:55 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[GameList] (
    [Id]          INT           IDENTITY (1, 1) NOT NULL,
    [name]        VARCHAR (100) NOT NULL,
    [multiplayer] BIT           DEFAULT ((0)) NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

