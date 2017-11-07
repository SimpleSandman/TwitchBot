USE [twitchbotdb]
GO

/****** Object: Table [dbo].[tblErrorLog] Script Date: 9/8/2016 2:09:49 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ErrorLog] (
    [ID]          INT            IDENTITY (1, 1) NOT NULL,
    [errorTime]   DATETIME       NOT NULL,
    [errorLine]   INT            NOT NULL,
    [errorClass]  VARCHAR (100)  NULL,
    [errorMethod] VARCHAR (100)  NULL,
    [errorMsg]    VARCHAR (4000) NOT NULL,
    [broadcaster] INT            NOT NULL,
    [command]     VARCHAR (50)   NULL,
    [userMsg]     VARCHAR (500)  NULL,
    PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_tblErrorLog_tblBroadcaster] FOREIGN KEY ([broadcaster]) REFERENCES [dbo].[Broadcasters] ([Id])
);

