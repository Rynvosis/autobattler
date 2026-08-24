namespace Api.Combat.Scopes;

public record ScopeRange
{
    public int? Start { get; init; }
    public int? End { get; init; }

    public static ScopeRange At(int slot) => new() { Start = slot, End = slot + 1 };

    public static ScopeRange Between(int start, int end) => new() { Start = start, End = end };

    public static ScopeRange From(int start) => new() { Start = start };

    public static ScopeRange Before(int end) => new() { End = end };

    public IReadOnlyList<Unit> Slice(IReadOnlyList<Unit> units)
    {
        int start = Math.Max(0, Start ?? 0);
        int end = Math.Min(units.Count, End ?? units.Count);

        return start >= end ? [] : [.. units.Take(start..end)];
    }
}
