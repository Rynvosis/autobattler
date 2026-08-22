namespace Api.Combat;

public class Battle
{
    private readonly Team _player;
    private readonly Team _ghost;

    private readonly Dictionary<int, Unit> _unitsById;

    private readonly List<BattleEvent> _events = [];

    private int _tick;
    private int _subtick;

    private Battle(Team player, Team ghost)
    {
        _player = player;
        _ghost = ghost;
        _unitsById = player.Units.Concat(ghost.Units).ToDictionary(unit => unit.Id);
    }

    public static BattleResult Resolve(Team player, Team ghost) =>
        new Battle(player, ghost).Run();

    private static void ResolveDeaths(Team team)
    {
        foreach (Unit unit in team.Units.ToList())
        {
            if (unit.Health > 0) continue;

            unit.Dead = true;
            team.Remove(unit);
        }
    }

    private Unit? Find(int id) => _unitsById.GetValueOrDefault(id);

    private BattleResult Run()
    {
        while (RunTick())
        { }

        bool playerEmpty = _player.MaybeHead == null;
        bool ghostEmpty = _ghost.MaybeHead == null;

        BattleOutcome outcome = (playerEmpty, ghostEmpty) switch
        {
            (true, false) => BattleOutcome.Loss,
            (false, true) => BattleOutcome.Win,
            _ => BattleOutcome.Draw
        };

        return new BattleResult { Outcome = outcome, Events = _events };
    }

    private bool RunTick()
    {
        Unit? maybePlayerHead = _player.MaybeHead;
        Unit? maybeGhostHead = _ghost.MaybeHead;
        if (maybePlayerHead == null || maybeGhostHead == null) return false;  // todo: handle empty teams better
        Unit playerHead = maybePlayerHead;
        Unit ghostHead = maybeGhostHead;

        if (_tick >= 64) return false; // todo: move magic numbers to config

        //manual attack logic, todo: queue into first subtick when implement subtick
        playerHead.Health -= ghostHead.Attack;
        ghostHead.Health -= playerHead.Attack;

        //death check, todo: move into dedicated step at subtick end:
        ResolveDeaths(_player);
        ResolveDeaths(_ghost);

        _tick++;
        return true;
    }
}
