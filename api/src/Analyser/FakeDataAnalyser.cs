using System.Text.Json;

namespace Nba.Api.Analyser
{
    /// <summary>
    /// A fake class which returns the same output, to reduce openai calls
    /// </summary>
    public class FakeDataAnalyser : IDataAnalyser
    {
        const string fake = """
                        
            {
              "topTeams": [
                { "name": "Denver Nuggets", "winPct": 0.6875 },
                { "name": "Toronto Raptors", "winPct": 0.6875 },
                { "name": "San Antonio Spurs", "winPct": 0.5625 },
                { "name": "Miami Heat", "winPct": 0.500 },
                { "name": "Chicago Bulls", "winPct": 0.500 }
              ],
              "anomalies": [
                "Houston Rockets have a very low win percentage of 31.25% despite having a balanced home and away game schedule.",
                "San Antonio Spurs recorded an extremely dominant win with a 48-point margin (116-68) against the Chicago Bulls.",
                "Oklahoma City Thunder and Houston Rockets both share the lowest win percentage (31.25%) despite being known as competitive teams."
              ],
              "insights": [
                "Both Denver Nuggets and Toronto Raptors share the best win percentage at 68.75%.",
                "The league has shown a curious balance with most teams having equal home and away games.",
                "Despite competitive strengths, Oklahoma City Thunder and Houston Rockets are underperforming this season."
              ]
            }
            """;

        string fakeSingle = """
{
  "summary": "<your 2-3 sentence narrative>",
  "tone": "professional and analytical"
}
""";

        public Task<NbaSingleTeamAnalsysis?> AnalyseSingleTeamData(string teamName)
        {
            var result = JsonSerializer.Deserialize<NbaSingleTeamAnalsysis>(fakeSingle);
            return Task.FromResult(result);
        }

        public Task<NbaAnalysis?> AnalyzeData()
        {
            var result = JsonSerializer.Deserialize<NbaAnalysis>(fake);
            return Task.FromResult(result);
        }

        public Task<NbaPredictionResult> PredictNextOutcome(int homeId, int awayId)
        {
            throw new NotImplementedException();
        }
    }
}
