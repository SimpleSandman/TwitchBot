CREATE PROCEDURE [dbo].[GetPartyUpRequestList]
	@GameId INT,
	@BroadcasterId INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON

	SELECT pur.Id AS PartyRequestId
	     , pur.Username
		 , pu.PartyMemberName
		 , pur.PartyMemberId
		 , pur.TimeRequested
	FROM dbo.PartyUpRequest AS pur 
	INNER JOIN dbo.PartyUp AS pu 
	    ON pur.PartyMemberId = pu.Id
	WHERE GameId = @GameId AND BroadcasterId = @BroadcasterId
	ORDER BY pur.TimeRequested
END
GO


