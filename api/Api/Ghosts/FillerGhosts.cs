namespace Api.Ghosts;

public static class FillerGhosts
{
    // Ids are fixed per index, so concurrent top-ups converge on the same items.
    // The prefix sorts after every GUID, so a query page fills with real ghosts first.
    public static string IdFor(int stage, int index)
    {
        return $"zz-filler-{stage}-{index}";
    }
}