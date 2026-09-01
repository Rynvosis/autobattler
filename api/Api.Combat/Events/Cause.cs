namespace Api.Combat.Events;

public enum CauseKind
{
    None,
    Board,
    Attack,
    Ability
}

public readonly record struct Cause(CauseKind Kind, Unit? Owner)
{
    public static Cause Board => new(CauseKind.Board, null);

    public static Cause Attack(Unit owner)
    {
        return new Cause(CauseKind.Attack, owner);
    }

    public static Cause Ability(Unit owner)
    {
        return new Cause(CauseKind.Ability, owner);
    }
}
