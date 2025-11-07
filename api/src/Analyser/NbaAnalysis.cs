namespace Nba.Api.Analyser
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public class NbaSingleTeamAnalsysis
    {
        [JsonPropertyName("summary")]
        public string Summary { get; set; } = string.Empty;
    }

    public class NbaAnalysis
    {
        [JsonPropertyName("topTeams")]
        public List<Team> TopTeams { get; set; } = new();

        [JsonPropertyName("anomalies")]
        public List<string> Anomalies { get; set; } = new();

        [JsonPropertyName("insights")]
        public List<string> Insights { get; set; } = new();
    }

    public class Team
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("winPct")]
        public double WinPct { get; set; }
    }

}
