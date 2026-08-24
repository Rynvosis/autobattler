using Api.Ghosts;
using Api.Runs;
using Api.Storage;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

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

app.MapRuns();

app.Run();

public partial class Program;
