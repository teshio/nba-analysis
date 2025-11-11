using Microsoft.Data.SqlClient;
using Nba.Api.Analyser;
using Nba.Api.DataAccess;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

var connStr = builder.Configuration.GetConnectionString("DefaultConnection");
var openAIEnabled = builder.Configuration.GetSection("OpenAI").GetValue<bool>("Enabled");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IDbConnection>(_ => new SqlConnection(connStr));
builder.Services.AddScoped<IDataRepository, DataRepository>();

if (openAIEnabled)
{
    builder.Services.AddScoped<IDataAnalyser, DataAnalyser>();
}
else
{
    builder.Services.AddScoped<IDataAnalyser, FakeDataAnalyser>();
}

const string DevCors = "DevCors";
builder.Services.AddCors(options =>
{
    options.AddPolicy(DevCors, policy =>
        policy.WithOrigins("http://localhost:5173", "https://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors(DevCors);
app.MapGet("/", () => Results.Redirect("/swagger"));

app.MapGet("/api/teams/summary", async (IDataRepository db) =>
{
    return Results.Ok(await db.GetTeamSummariesAsync());
});

app.MapPost("/api/ai/analyse", async (IDataAnalyser analyser, TeamAnalyseRequest req) =>
{
    var result = await analyser.AnalyseSingleTeamData(req.TeamName);
    if (result == null)
    {
        return Results.InternalServerError();
    }

    return Results.Ok(new AnalyseResponse(result.Summary));
});

app.MapGet("/api/analyse-all", async (IDataAnalyser analyser) =>
{
    var result = await analyser.AnalyzeData();

    if (result == null)
    {
        return Results.InternalServerError();
    }

    return Results.Ok(new { analysis = result });
});

app.MapPost("/api/predict-next", async (
    IDataRepository dataRepository, 
    IDataAnalyser analyser, 
    string homeTeam, 
    string awayTeam) =>
{
    var summary = await dataRepository.GetTeamSummariesAsync();
    var homeTeamId = summary.FirstOrDefault(x => x.TeamName == homeTeam)?.TeamId;
    var awayTeamId = summary.FirstOrDefault(x => x.TeamName == awayTeam)?.TeamId;

    if(!homeTeamId.HasValue)
    {
        return Results.InternalServerError("home team is invalid");
    }

    if (!awayTeamId.HasValue)
    {
        return Results.InternalServerError("away team is invalid");
    }

    var result = await analyser.PredictNextOutcome(homeTeamId.Value, awayTeamId.Value);
    return Results.Ok(result);
});

app.Run();
