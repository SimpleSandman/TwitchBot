CREATE PROCEDURE [dbo].[GetPartyUpRequestList]
	@GameId INT,
	@Broadcaster INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON

	SELECT pur.Id
		 , pur.Username
		 , pu.PartyMember AS PartyMemberName
		 , pu.Broadcaster
		 , pu.GameId
		 , pur.TimeRequested
	FROM dbo.PartyUpRequest AS pur 
	INNER JOIN dbo.PartyUp AS pu 
		ON pur.PartyMember = pu.Id
	WHERE GameId = @GameId AND Broadcaster = @Broadcaster
END
GO


