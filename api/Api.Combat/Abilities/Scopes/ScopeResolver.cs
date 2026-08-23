namespace Api.Combat.Abilities.Scopes;

public static class ScopeResolver
{
    public static IReadOnlyList<Unit> Resolve<TContext>(IReadOnlyList<IScope<TContext>> scopes, TContext context)
        where TContext : TriggerContext
    {
        HashSet<Unit> selected = [];

        foreach (IScope<TContext> scope in scopes) selected.UnionWith(scope.Resolve(context));

        HashSet<Unit> visible = [.. context.Visible(Side.Player), .. context.Visible(Side.Ghost)];

        return
        [
            .. context.Board.UnitsInIterationOrder()
                .Select(entry => entry.unit)
                .Where(unit => selected.Contains(unit) && visible.Contains(unit))
        ];
    }
}
