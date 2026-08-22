namespace Api.Combat;

public class Battle
{
    private readonly Team _player;
    private readonly Team _ghost;

    private readonly List<BattleEvent> _events = [];

    private int _tick;
    private int _subtick;

    private Battle(Team player, Team ghost)
    {
        _player = player;
        _ghost = ghost;
    }

    public static BattleResult Resolve(Team player, Team ghost) =>
        new Battle(player, ghost).Run();

    private BattleResult Run() => throw new NotImplementedException();
}
