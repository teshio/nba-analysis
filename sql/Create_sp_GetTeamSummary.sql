USE [NBA];
GO

IF OBJECT_ID('dbo.sp_GetTeamSummary') IS NOT NULL
    DROP PROC dbo.sp_GetTeamSummary;
GO

CREATE PROC dbo.sp_GetTeamSummary
AS
BEGIN

SELECT * 
FROM dbo.vw_TeamSummary 
ORDER BY TeamName

END


