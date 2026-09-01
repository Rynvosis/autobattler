using Api.Content;
using Api.Ghosts;
using Api.Runs;
using Api.Runs.Shop;
using Api.Serialization;
using Api.Storage;
using Microsoft.Extensions.FileProviders;

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

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseExceptionHandler();

// The client's build, served same-origin so it needs no CORS. Only the build: pointing this at
// web/ itself would also serve the sources, package.json and node_modules. Absent until someone
// runs `npm run build`, so the API has to boot without it.
string clientRoot = Path.GetFullPath(
    Path.Combine(app.Environment.ContentRootPath, "..", "..", "web", "dist"));

if (Directory.Exists(clientRoot))
{
    app.UseFileServer(new FileServerOptions { FileProvider = new PhysicalFileProvider(clientRoot) });
}

app.MapContent();
app.MapRuns();
app.MapMoves();

app.Run();

public partial class Program;
