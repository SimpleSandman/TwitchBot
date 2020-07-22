CREATE PROCEDURE [dbo].[CreateDefaultRanks]
    @BroadcasterId INT
AS
BEGIN
    -- SET NOCOUNT ON added to prevent extra result sets from
    -- interfering with SELECT statements.
    SET NOCOUNT ON

    IF NOT EXISTS (SELECT Name FROM dbo.Rank WHERE BroadcasterId = @BroadcasterId)
    BEGIN
        INSERT INTO dbo.Rank (Name, ExpCap, BroadcasterId) VALUES ('Recruit', 120, @BroadcasterId)
        INSERT INTO dbo.Rank (Name, ExpCap, BroadcasterId) VALUES ('Private', 240, @BroadcasterId)
        INSERT INTO dbo.Rank (Name, ExpCap, BroadcasterId) VALUES ('Private First Class', 480, @BroadcasterId)
        INSERT INTO dbo.Rank (Name, ExpCap, BroadcasterId) VALUES ('Sergeant', 960, @BroadcasterId)
        INSERT INTO dbo.Rank (Name, ExpCap, BroadcasterId) VALUES ('Staff Sergeant', 1920, @BroadcasterId)
        INSERT INTO dbo.Rank (Name, ExpCap, BroadcasterId) VALUES ('Technical Sergeant', 3840, @BroadcasterId)
        INSERT INTO dbo.Rank (Name, ExpCap, BroadcasterId) VALUES ('Master Sergeant', 7680, @BroadcasterId)
        INSERT INTO dbo.Rank (Name, ExpCap, BroadcasterId) VALUES ('First Master Sergeant', 11520, @BroadcasterId)
        INSERT INTO dbo.Rank (Name, ExpCap, BroadcasterId) VALUES ('Senior Master Sergeant', 14400, @BroadcasterId)
        INSERT INTO dbo.Rank (Name, ExpCap, BroadcasterId) VALUES ('First Senior Master Sergeant', 18000, @BroadcasterId)
        INSERT INTO dbo.Rank (Name, ExpCap, BroadcasterId) VALUES ('Chief Master Sergeant', 22500, @BroadcasterId)
        INSERT INTO dbo.Rank (Name, ExpCap, BroadcasterId) VALUES ('First Chief Master Sergeant', 28140, @BroadcasterId)
        INSERT INTO dbo.Rank (Name, ExpCap, BroadcasterId) VALUES ('Sergeant Major', 35160, @BroadcasterId)
        INSERT INTO dbo.Rank (Name, ExpCap, BroadcasterId) VALUES ('Sergeant Major of the Army', 43920, @BroadcasterId)
        INSERT INTO dbo.Rank (Name, ExpCap, BroadcasterId) VALUES ('Warrant Officer 1', 50520, @BroadcasterId)
        INSERT INTO dbo.Rank (Name, ExpCap, BroadcasterId) VALUES ('Warrant Officer 2', 58140, @BroadcasterId)
        INSERT INTO dbo.Rank (Name, ExpCap, BroadcasterId) VALUES ('Warrant Officer 3', 66840, @BroadcasterId)
        INSERT INTO dbo.Rank (Name, ExpCap, BroadcasterId) VALUES ('Warrant Officer 4', 75540, @BroadcasterId)
        INSERT INTO dbo.Rank (Name, ExpCap, BroadcasterId) VALUES ('Chief Warrant Officer', 85320, @BroadcasterId)
        INSERT INTO dbo.Rank (Name, ExpCap, BroadcasterId) VALUES ('Second Lieutenant', 96420, @BroadcasterId)
        INSERT INTO dbo.Rank (Name, ExpCap, BroadcasterId) VALUES ('First Lieutenant', 108960, @BroadcasterId)
        INSERT INTO dbo.Rank (Name, ExpCap, BroadcasterId) VALUES ('Captain', 123120, @BroadcasterId)
        INSERT INTO dbo.Rank (Name, ExpCap, BroadcasterId) VALUES ('Major', 136680, @BroadcasterId)
        INSERT INTO dbo.Rank (Name, ExpCap, BroadcasterId) VALUES ('Lieutenant Colonel', 151740, @BroadcasterId)
        INSERT INTO dbo.Rank (Name, ExpCap, BroadcasterId) VALUES ('Colonel', 165360, @BroadcasterId)
        INSERT INTO dbo.Rank (Name, ExpCap, BroadcasterId) VALUES ('Brigadier General', 180240, @BroadcasterId)
        INSERT INTO dbo.Rank (Name, ExpCap, BroadcasterId) VALUES ('Major General', 196500, @BroadcasterId)
        INSERT INTO dbo.Rank (Name, ExpCap, BroadcasterId) VALUES ('Lieutenant General', 214140, @BroadcasterId)
        INSERT INTO dbo.Rank (Name, ExpCap, BroadcasterId) VALUES ('General of the Army', 233460, @BroadcasterId)

        SELECT Id, Name, ExpCap, BroadcasterId FROM dbo.Rank WHERE BroadcasterId = @BroadcasterId
    END
END
GO