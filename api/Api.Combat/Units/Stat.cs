namespace Api.Combat.Units;

public enum Stat
{
    Health,
    Attack
}

public static class StatExtensions
{
    public static int Of(this Stat stat, Unit unit)
    {
        return stat == Stat.Attack ? unit.Attack : unit.Health;
    }
}
