using Api.Content;
using Api.Teams;

namespace Api.Runs.Shop;

public static class Moves
{
    public static MoveOutcome Reroll(Run run)
    {
        if (run.Gold < Economy.RerollCost) return MoveOutcome.Refused(MoveError.InsufficientGold);

        return MoveOutcome.Accepted(run with
        {
            Gold = run.Gold - Economy.RerollCost,
            Shop = ShopOffers.Roll()
        });
    }

    public static MoveOutcome Buy(Run run, int shopSlot)
    {
        if (run.Shop.ElementAtOrDefault(shopSlot) is not { } offer) return MoveOutcome.Refused(MoveError.EmptySlot);

        if (run.Gold < Economy.UnitCost) return MoveOutcome.Refused(MoveError.InsufficientGold);

        if (run.Units.Count >= Economy.TeamSize) return MoveOutcome.Refused(MoveError.TeamFull);

        return MoveOutcome.Accepted(run with
        {
            Gold = run.Gold - Economy.UnitCost,
            Units = [.. run.Units, offer],
            Shop = [.. run.Shop.Select((slotOffer, slot) => slot == shopSlot ? null : slotOffer)]
        });
    }

    public static MoveOutcome Duplicate(Run run, int teamSlot)
    {
        if (run.Units.ElementAtOrDefault(teamSlot) is not { } unit) return MoveOutcome.Refused(MoveError.EmptySlot);

        if (run.Duplicated) return MoveOutcome.Refused(MoveError.AlreadyDuplicated);

        if (run.Gold < Economy.DuplicateCost) return MoveOutcome.Refused(MoveError.InsufficientGold);

        if (run.Units.Count >= Economy.TeamSize) return MoveOutcome.Refused(MoveError.TeamFull);

        UnitDefinition definition = Monsters.Manifest.Units.First(candidate => candidate.Kind == unit.Kind);

        return MoveOutcome.Accepted(run with
        {
            Gold = run.Gold - Economy.DuplicateCost,
            Units = [.. run.Units, TeamUnits.From(definition)],
            Duplicated = true
        });
    }

    public static MoveOutcome Upgrade(Run run, int teamSlot)
    {
        if (run.Units.ElementAtOrDefault(teamSlot) is null) return MoveOutcome.Refused(MoveError.EmptySlot);

        bool onCredit = run.UpgradeCredits > 0;

        if (!onCredit && run.Gold < Economy.UpgradeCost) return MoveOutcome.Refused(MoveError.InsufficientGold);

        return MoveOutcome.Accepted(run with
        {
            Gold = onCredit ? run.Gold : run.Gold - Economy.UpgradeCost,
            UpgradeCredits = onCredit ? run.UpgradeCredits - 1 : run.UpgradeCredits,
            Units =
            [
                .. run.Units.Select((unit, slot) => slot == teamSlot
                    ? unit with { Attack = unit.Attack + 1, Health = unit.Health + 1 }
                    : unit)
            ]
        });
    }

    public static MoveOutcome Sell(Run run, int teamSlot)
    {
        if (run.Units.ElementAtOrDefault(teamSlot) is null) return MoveOutcome.Refused(MoveError.EmptySlot);

        if (run.Units.Count <= 1) return MoveOutcome.Refused(MoveError.LastUnit);

        return MoveOutcome.Accepted(run with
        {
            UpgradeCredits = run.UpgradeCredits + 1,
            Units = [.. run.Units.Where((_, slot) => slot != teamSlot)]
        });
    }

    public static MoveOutcome Reorder(Run run, IReadOnlyList<int> order)
    {
        if (!order.Order().SequenceEqual(Enumerable.Range(0, run.Units.Count)))
            return MoveOutcome.Refused(MoveError.NotAPermutation);

        return MoveOutcome.Accepted(run with { Units = [.. order.Select(slot => run.Units[slot])] });
    }
}

public readonly record struct MoveOutcome
{
    private MoveOutcome(Run? run, MoveError error)
    {
        Run = run;
        Error = error;
    }

    public Run? Run { get; }
    public MoveError Error { get; }

    public static MoveOutcome Accepted(Run run)
    {
        return new MoveOutcome(run, MoveError.None);
    }

    public static MoveOutcome Refused(MoveError error)
    {
        return new MoveOutcome(null, error);
    }
}

public enum MoveError
{
    None,
    InsufficientGold,
    TeamFull,
    EmptySlot,
    EmptyTeam,
    LastUnit,
    AlreadyDuplicated,
    NotAPermutation,
    RunFinished
}
