namespace Nba.Api.Analyser
{
    public interface IDataAnalyser
    {
        Task<NbaAnalysis?> AnalyzeData();

        Task<NbaSingleTeamAnalsysis?> AnalyseSingleTeamData(string teamName);

        Task<NbaPredictionResult> PredictNextOutcome(int homeId, int awayId);
    }

    public class NbaPredictionResult 
    { 
        public string PredictionSummary { get; set; } = string.Empty;
    }
}