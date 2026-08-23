namespace Api.Combat.Abilities.Scopes;

public record ScopeRange
{
    private int? Start { get; init; }
    private int? End { get; init; }

    public static ScopeRange At(int slot) => new() { Start = slot, End = slot + 1 };

    public static ScopeRange Between(int start, int end) => new() { Start = start, End = end };

    public static ScopeRange From(int start) => new() { Start = start };

    public static ScopeRange Before(int end) => new() { End = end };

    public IReadOnlyList<Unit> Slice(IReadOnlyList<Unit> units, int origin = 0)
    {
        int start = Math.Max(0, Start is { } first ? origin + first : 0);
        int end = Math.Min(units.Count, End is { } last ? origin + last : units.Count);

        return start >= end ? [] : [.. units.Take(start..end)];
    }
}
