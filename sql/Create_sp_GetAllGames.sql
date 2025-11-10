USE [NBA];
GO

IF OBJECT_ID('dbo.sp_GetAllGames') IS NOT NULL
    DROP PROC dbo.sp_GetAllGames;
GO

CREATE PROC dbo.sp_GetAllGames
AS
BEGIN

SELECT HomeTeamID, AwayTeamID, HomeScore, AwayScore, tH.Name as HomeTeamName , tA.Name as AwayTeamName
FROM Games g 
LEFT JOIN Teams tH on tH.TeamID = g.HomeTeamID
LEFT JOIN Teams tA on tA.TeamID = g.AwayTeamID

END


