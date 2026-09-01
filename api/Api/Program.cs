using Api.Content;
using Api.Ghosts;
using Api.Runs;
using Api.Runs.Shop;
using Api.Serialization;
using Api.Storage;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options => ApiJson.Configure(options.SerializerOptions));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDynamoDb(builder.Configuration);
builder.Services.AddRuns();
builder.Services.AddGhosts();

WebApplication app = builder.Build();

await app.EnsureLocalTablesCreatedAsync();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseExceptionHandler();

app.MapContent();
app.MapRuns();
app.MapMoves();

app.Run();

public partial class Program;
