USE [twitchbotdb]
GO

/****** Object: Table [dbo].[tblCountdown] Script Date: 9/8/2016 2:09:42 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[tblCountdown] (
    [Id]          INT          IDENTITY (1, 1) NOT NULL,
    [dueDate]     DATETIME     NOT NULL,
    [message]     VARCHAR (50) NOT NULL,
    [broadcaster] INT          NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tblCountdown_tblBroadcasters] FOREIGN KEY ([broadcaster]) REFERENCES [dbo].[tblBroadcasters] ([Id])
);


