namespace Api.Combat;

public class Team
{
    public Team(List<Unit> units) => Units = units;

    public List<Unit> Units { get; }

    public bool IsEmpty => Units.Count == 0;

    public Unit Head => Units[0];
}
