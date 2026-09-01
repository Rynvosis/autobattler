using Api.Combat.Battlefield;

namespace Api.Battles;

public static class UnitRecords
{
    public static IReadOnlyList<UnitRecord> From(Team team)
    {
        return
        [
            .. team.Units.Select(unit => new UnitRecord
            {
                Id = unit.Id,
                Kind = unit.Kind,
                Attack = unit.Attack,
                Health = unit.Health
            })
        ];
    }
}
