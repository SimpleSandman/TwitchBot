USE [twitchbotdb]
GO

/****** Object: Table [dbo].[tblSongRequestBlacklist] Script Date: 9/28/2017 11:05:21 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[SongRequestBlacklist] (
    [Id]          INT           IDENTITY (1, 1) NOT NULL,
    [artist]      VARCHAR (100) NOT NULL,
    [title]       VARCHAR (100) NULL,
    [broadcaster] INT           NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tblSongRequestBlacklist_tblBroadcasters] FOREIGN KEY ([broadcaster]) REFERENCES [dbo].[Broadcasters] ([Id])
);

