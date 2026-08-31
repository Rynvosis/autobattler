namespace Api.Content;

public static class ContentEndpoints
{
    public static void MapContent(this WebApplication app)
    {
        app.MapGet("/content", GetContent);
    }

    private static IResult GetContent()
    {
        return Results.Ok(ContentResponses.From(Monsters.Manifest));
    }
}
