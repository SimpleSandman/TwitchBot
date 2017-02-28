USE [twitchbotdb]
GO

/****** Object: Table [dbo].[tblModerators] Script Date: 9/8/2016 2:10:02 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[tblModerators] (
    [Id]          INT          IDENTITY (1, 1) NOT NULL,
    [username]    VARCHAR (30) NOT NULL,
    [broadcaster] INT          NOT NULL,
    [timeAdded]   DATETIME     DEFAULT (getdate()) NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tblModerators_tblBroadcasters] FOREIGN KEY ([broadcaster]) REFERENCES [dbo].[tblBroadcasters] ([Id])
);


