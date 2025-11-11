using Microsoft.ML;
using Nba.Api.DataAccess;
using OpenAI.Chat;
using System.Data;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nba.Api.Analyser
{

    public class DataAnalyser : IDataAnalyser
    {
        private readonly ChatClient _client;
        private readonly IDataRepository _dataRepository;

        private Lazy<string> _outputJsonSchema = new(() =>
        {
            return File.ReadAllText("Schemas/analyse-all-schema.json");
        });

        public DataAnalyser(IConfiguration configuration, IDataRepository dataRepository)
        {
            var key = configuration.GetSection("OpenAI")["ApiKey"];
            _client = new("gpt-4o", key);
            _dataRepository = dataRepository;
        }

        public async Task<NbaSingleTeamAnalsysis?> AnalyseSingleTeamData(string teamName)
        {
            var data = await _dataRepository.GetTeamSummariesAsync();
            var teamData = data.SingleOrDefault(x => x.TeamName == teamName);

            if (teamData == null)
            {
                throw new ArgumentException($"Team '{teamName}' not found.");
            }

            var json = JsonSerializer.Serialize(
                    teamData,
                    new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                        WriteIndented = false
                    });

            // 3) Clear instructions + ask for JSON output
            var prompt = """
You are an expert NBA analyst with strong data interpretation skills.

Below is structured data for one NBA team from a season summary dataset.
Use it to provide a short, insightful analysis (no more than 3 sentences) about the team’s performance, recent form, and any interesting trends.

Be factual and concise, but conversational — something a sports reporter might say.
Avoid speculation, clichés, or repeating the numbers verbatim; interpret them instead.

Return your response in this JSON format:
{
  "summary": "<your 2-3 sentence narrative>",
  "tone": "professional and analytical"
}
""";
            var input = $"{prompt}\n\nDATA=\n{json}";

            // Send the request
            ChatCompletion completion = await _client.CompleteChatAsync(input);
            var analysisResult = completion.Content[0].Text;
            var stripped = analysisResult.Replace("```json", "").Replace("```", "").Trim();
            var result = JsonSerializer.Deserialize<NbaSingleTeamAnalsysis>(stripped);

            return result;
        }

        public async Task<NbaAnalysis?> AnalyzeData()
        {
            try
            {
                var data = await _dataRepository.GetTeamSummariesAsync();

                var compact = data.Select(r => new
                {
                    r.TeamName,
                    r.TeamStadiumName,
                    r.SeasonMVPOnTeam,
                    r.NumberOfGamesPlayed,
                    r.NumberOfGamesWon,
                    r.NumberOfGamesLost,
                    r.NumberOfGamesPlayedAtHome,
                    r.NumberOfGamesPlayedAway,
                    r.PointsInBiggestWin,
                    r.PointsInBiggestLoss,
                    r.LastGameStadiumName,
                    LastGameDate = r.LastGameDate?.ToString("yyyy-MM-dd")
                });

                var json = JsonSerializer.Serialize(
                    compact,
                    new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                        WriteIndented = false
                    });

                // 3) Clear instructions + ask for JSON output
                var prompt = """
You are an NBA analyst. Given an array of team summaries, produce:
- top 5 teams by win %,
- 3 most interesting anomalies or outliers,
- 3 succinct league-wide insights (one sentence each).

Return strict JSON with:
{
  "topTeams": [{ "name": string, "winPct": number }],
  "anomalies": [string],
  "insights": [string]
}
""";
                var input = prompt + "\n\nDATA=\n" + json;

                ChatCompletionOptions options = new()
                {
                    ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                        jsonSchemaFormatName: "analysis-all-schema",
                        jsonSchema: BinaryData.FromBytes(Encoding.UTF8.GetBytes(_outputJsonSchema.Value)),
                        jsonSchemaIsStrict: true)
                };

                // Send the request
                ChatCompletion completion = await _client.CompleteChatAsync(input);
                var analysisResult = completion.Content[0].Text;
                var stripped = analysisResult.Replace("```json", "").Replace("```", "").Trim();
                var result = JsonSerializer.Deserialize<NbaAnalysis>(stripped);

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during the API call: {ex.Message}");
                throw;
            }
        }

        public async Task<NbaPredictionResult> PredictNextOutcome(int homeId, int awayId)
        {
            var allGames = await _dataRepository.GetAllGameDataAsync();

            var homeName = allGames.FirstOrDefault(x => x.HomeTeamID == homeId)?.HomeTeamName ?? "Home Team";
            var awayName = allGames.FirstOrDefault(x => x.AwayTeamID == awayId)?.AwayTeamName ?? "Away Team";

            var mlContext = new MLContext();

            var dataView = mlContext.Data.LoadFromEnumerable(allGames);

            // TOOD - verify model with test data
            var split = mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);

            // Home model: predict HomeScore using only HomeTeamID
            var pipelineHome = mlContext.Transforms.Categorical
                .OneHotEncoding("HomeTeamEncoded", "HomeTeamID")
                .Append(mlContext.Transforms.CopyColumns("Features", "HomeTeamEncoded"))
                .Append(mlContext.Regression.Trainers.Sdca(labelColumnName: "HomeScore"));

            // Away model: predict AwayScore using only AwayTeamID
            var pipelineAway = mlContext.Transforms.Categorical
                .OneHotEncoding("AwayTeamEncoded", "AwayTeamID")
                .Append(mlContext.Transforms.CopyColumns("Features", "AwayTeamEncoded"))
                .Append(mlContext.Regression.Trainers.Sdca(labelColumnName: "AwayScore"));

            // train model from training data
            var modelHome = pipelineHome.Fit(split.TrainSet);
            var modelAway = pipelineAway.Fit(split.TrainSet);

            var nextGame = new GameData
            {
                HomeTeamID = homeId,
                AwayTeamID = awayId
            };

            var predictorHome = mlContext.Model.CreatePredictionEngine<GameData, ScorePrediction>(modelHome);
            var predictorAway = mlContext.Model.CreatePredictionEngine<GameData, ScorePrediction>(modelAway);

            // Predict scores
            float predictedHome = predictorHome.Predict(nextGame).PredictedScore;
            float predictedAway = predictorAway.Predict(nextGame).PredictedScore;

            var homeMetrics = mlContext.Regression.Evaluate(modelHome.Transform(split.TestSet), labelColumnName: "HomeScore");
            var awayMetrics = mlContext.Regression.Evaluate(modelAway.Transform(split.TestSet), labelColumnName: "AwayScore");

            return new NbaPredictionResult
            {
                PredictionSummary = $"{homeName} vs {awayName}: " +
                                    $"Predicted Score {predictedHome:F0} - {predictedAway:F0}, " +
                                    $"Model Fit (R²): Home={homeMetrics.RSquared:F2}, Away={awayMetrics.RSquared:F2}"
            };
        }
    }
}
