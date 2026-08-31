namespace Api.Content;

public static class ContentEndpoints
{
    private static readonly ContentResponse Response = ContentResponses.From(Monsters.Manifest);

    public static void MapContent(this WebApplication app)
    {
        app.MapGet("/content", GetContent);
    }

    private static IResult GetContent()
    {
        return Results.Ok(Response);
    }
}
