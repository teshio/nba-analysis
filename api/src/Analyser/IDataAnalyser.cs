namespace Nba.Api.Analyser
{
    public interface IDataAnalyser
    {
        Task<NbaAnalysis?> AnalyzeData();

        Task<NbaSingleTeamAnalsysis?> AnalyseSingleTeamData(string teamName);
    }
}