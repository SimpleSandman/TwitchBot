CREATE TABLE [dbo].[tblSongRequestBlacklist] (
    [Id]          INT           IDENTITY (1, 1) NOT NULL,
    [artist]      VARCHAR (100) NOT NULL,
    [title]       VARCHAR (100) NULL,
    [broadcaster] INT           NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tblSongRequestBlacklist_tblBroadcasters] FOREIGN KEY ([broadcaster]) REFERENCES [dbo].[tblBroadcasters] ([Id])
);
