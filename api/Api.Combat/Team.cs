namespace Api.Combat;

public class Team
{
    private readonly List<Unit> _units;

    public Team(List<Unit> units) => _units = [.. units];

    public IReadOnlyList<Unit> Units => _units;

    public bool IsEmpty => _units.Count == 0;
    public int Count => _units.Count;

    public Unit Head => _units[0];

    public void Remove(Unit unit) => _units.Remove(unit);
}
