namespace Api.Combat.Battlefield;

public class Team(List<Unit> units)
{
    private readonly List<Unit> _units = [.. units];

    public IReadOnlyList<Unit> Units => _units;

    public bool IsEmpty => _units.Count == 0;
    public int Count => _units.Count;

    public Unit Head => _units[0];

    public bool Remove(Unit unit) => _units.Remove(unit);

    public int? IndexOf(Unit unit)
    {
        int index = _units.IndexOf(unit);
        return index >= 0 ? index : null;
    }
}
