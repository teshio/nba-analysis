using Microsoft.ML.Data;

namespace Nba.Api.DataAccess
{
    public class GameData
    {
        public float HomeTeamID { get; set; }
        public float AwayTeamID { get; set; }
        public float HomeScore { get; set; }
        public float AwayScore { get; set; }
        public string HomeTeamName { get; set; } = string.Empty;
        public string AwayTeamName { get; set; } = string.Empty;
    }

    public class ScorePrediction
    {
        [ColumnName("Score")]
        public float PredictedScore { get; set; }
    }
}
