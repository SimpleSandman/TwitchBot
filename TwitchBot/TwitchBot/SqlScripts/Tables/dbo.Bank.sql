USE [twitchbotdb]
GO

/****** Object: Table [dbo].[tblBank] Script Date: 9/8/2016 2:08:32 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Bank] (
    [Id]          INT          IDENTITY (1, 1) NOT NULL,
    [username]    VARCHAR (30) NOT NULL,
    [wallet]      INT          DEFAULT ((0)) NOT NULL,
    [broadcaster] INT          NOT NULL,
    [timeAdded]   DATETIME     DEFAULT (getdate()) NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tblBank_tblBroadcaster] FOREIGN KEY ([broadcaster]) REFERENCES [dbo].[Broadcasters] ([Id])
);

