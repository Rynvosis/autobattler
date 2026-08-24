using Api.Runs;
using Api.Storage;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDynamoDb(builder.Configuration);
builder.Services.AddRuns();

WebApplication app = builder.Build();

await app.EnsureLocalTablesCreatedAsync();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();

public partial class Program;
