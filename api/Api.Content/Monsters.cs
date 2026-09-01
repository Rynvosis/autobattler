using Api.Combat.Abilities;
using Api.Combat.Effects;
using Api.Combat.Events;
using Api.Combat.Scopes;
using Api.Combat.Units;

namespace Api.Content;

public static class Monsters
{
    private static readonly Kind GoblinKind = new("goblin");

    public static readonly UnitDefinition Golem = new()
    {
        Kind = new Kind("golem"),
        Name = "Golem",
        Icon = "🗿",
        Description = "No ability.",
        Attack = 5,
        Health = 10,
        Tier = 1,
        Ability = null
    };

    public static readonly UnitDefinition Ghoul = new()
    {
        Kind = new Kind("ghoul"),
        Name = "Ghoul",
        Icon = "🧟",
        Description = "When another unit dies: +2/+1 to self.",
        Attack = 2,
        Health = 5,
        Tier = 1,
        Ability = new Ability<UnitDeathEvent>
        {
            Trigger = new UnitTrigger<UnitDeathEvent>
            {
                Participant = new EventTarget(),

                // Every enemy, and every ally in front of or behind it — which is every ally
                // but itself, because Ahead and Behind both exclude their anchor.
                Scopes =
                [
                    Any<BattleEvent>.Of(new FromHead { Side = ScopeSide.Enemy }),
                    Any<BattleEvent>.Of(new Ahead<BattleEvent> { Anchor = One<BattleEvent>.Of(new Self()) }),
                    Any<BattleEvent>.Of(new Behind<BattleEvent> { Anchor = One<BattleEvent>.Of(new Self()) })
                ]
            },
            Effects =
            [
                new ScopedEffect<UnitDeathEvent>
                {
                    Effect = new StatChange<UnitDeathEvent>
                    {
                        Attack = Literal.Of(2),
                        Health = Literal.Of(1)
                    },
                    Scopes = [Every<UnitDeathEvent>.Of(new Self())]
                }
            ]
        }
    };

    public static readonly UnitDefinition Wyrm = new()
    {
        Kind = new Kind("wyrm"),
        Name = "Wyrm",
        Icon = "🐉",
        Description = "On attack: deals its attack to the enemy behind.",
        Attack = 2,
        Health = 5,
        Tier = 1,
        Ability = new Ability<UnitAttackEvent>
        {
            Trigger = new UnitTrigger<UnitAttackEvent>
            {
                Participant = new EventSource(),
                Scopes = [Any<BattleEvent>.Of(new Self())]
            },
            Effects =
            [
                new ScopedEffect<UnitAttackEvent>
                {
                    Effect = new Damage<UnitAttackEvent>
                    {
                        Value = new UnitStat<UnitAttackEvent>
                        {
                            Subject = One<UnitAttackEvent>.Of(new Self()),
                            Stat = Stat.Attack
                        }
                    },
                    Scopes =
                    [
                        new Every<UnitAttackEvent>
                        {
                            Relation = new FromHead { Side = ScopeSide.Enemy },
                            Range = ScopeRange.At(1)
                        }
                    ]
                }
            ]
        }
    };

    public static readonly UnitDefinition Vampire = new()
    {
        Kind = new Kind("vampire"),
        Name = "Vampire",
        Icon = "🧛",
        Description = "On attack: +0/+3 to itself.",
        Attack = 3,
        Health = 7,
        Tier = 1,
        Ability = new Ability<UnitAttackEvent>
        {
            Trigger = new UnitTrigger<UnitAttackEvent>
            {
                Participant = new EventSource(),
                Scopes = [Any<BattleEvent>.Of(new Self())]
            },
            Effects =
            [
                new ScopedEffect<UnitAttackEvent>
                {
                    Effect = new StatChange<UnitAttackEvent>
                    {
                        Attack = Literal.Of(0),
                        Health = Literal.Of(3)
                    },
                    Scopes = [Every<UnitAttackEvent>.Of(new Self())]
                }
            ]
        }
    };

    public static readonly UnitDefinition Goblin = new()
    {
        Kind = GoblinKind,
        Name = "Goblin",
        Icon = "👺",
        // The stats carry the +2/+2 a Goblin used to give itself, so a band of them ends up
        // exactly where it did when the buff included the caster.
        Description = "At the start of battle: +2/+2 to every other allied Goblin.",
        Attack = 3,
        Health = 6,
        Tier = 1,
        Ability = new Ability<StartEvent>
        {
            Trigger = new RoundTrigger<StartEvent>(),
            Effects =
            [
                new ScopedEffect<StartEvent>
                {
                    Effect = new StatChange<StartEvent>
                    {
                        Attack = Literal.Of(2),
                        Health = Literal.Of(2)
                    },

                    // Ahead and Behind both exclude their anchor, so together they are every
                    // ally but this one.
                    Scopes =
                    [
                        Every<StartEvent>.Of(new OfKind<StartEvent>
                        {
                            Relation = new Ahead<StartEvent> { Anchor = One<StartEvent>.Of(new Self()) },
                            Kind = GoblinKind
                        }),
                        Every<StartEvent>.Of(new OfKind<StartEvent>
                        {
                            Relation = new Behind<StartEvent> { Anchor = One<StartEvent>.Of(new Self()) },
                            Kind = GoblinKind
                        })
                    ]
                }
            ]
        }
    };

    public static readonly UnitDefinition Devourer = new()
    {
        Kind = new Kind("devourer"),
        Name = "Devourer",
        Icon = "👹",
        Description = "At the start of battle: eats the ally in front, taking its attack and health.",
        Attack = 2,
        Health = 4,
        Tier = 1,
        Ability = DevourerAbility()
    };

    public static readonly UnitDefinition Wraithblade = new()
    {
        Kind = new Kind("wraithblade"),
        Name = "Wraithblade",
        Icon = "⚔️",
        Description = "On death: gives its attack to the ally behind.",
        Attack = 4,
        Health = 1,
        Tier = 1,
        Ability = new Ability<UnitDeathEvent>
        {
            Trigger = new UnitTrigger<UnitDeathEvent>
            {
                Participant = new EventTarget(),
                Scopes = [Any<BattleEvent>.Of(new Self())]
            },
            Effects =
            [
                new ScopedEffect<UnitDeathEvent>
                {
                    Effect = new StatChange<UnitDeathEvent>
                    {
                        Attack = new UnitStat<UnitDeathEvent>
                        {
                            Subject = One<UnitDeathEvent>.Of(new Self()),
                            Stat = Stat.Attack
                        },
                        Health = Literal.Of(0)
                    },
                    Scopes =
                    [
                        new Every<UnitDeathEvent>
                        {
                            Relation = new Behind<UnitDeathEvent>
                            {
                                Anchor = One<UnitDeathEvent>.Of(new Self())
                            },
                            Range = ScopeRange.At(0)
                        }
                    ]
                }
            ]
        }
    };

    public static readonly UnitDefinition Deathcap = new()
    {
        Kind = new Kind("deathcap"),
        Name = "Deathcap",
        Icon = "🍄",
        Description = "On death: destroys the enemy in front.",
        Attack = 1,
        Health = 4,
        Tier = 1,
        Ability = new Ability<UnitDeathEvent>
        {
            Trigger = new UnitTrigger<UnitDeathEvent>
            {
                Participant = new EventTarget(),
                Scopes = [Any<BattleEvent>.Of(new Self())]
            },
            Effects =
            [
                new ScopedEffect<UnitDeathEvent>
                {
                    // Lethal by construction: damage equals the recipient's health.
                    Effect = new Damage<UnitDeathEvent> { Value = new RecipientStat { Stat = Stat.Health } },
                    Scopes =
                    [
                        new Every<UnitDeathEvent>
                        {
                            Relation = new FromHead { Side = ScopeSide.Enemy },
                            Range = ScopeRange.At(0)
                        }
                    ]
                }
            ]
        }
    };

    // TODO: Necromancer 3/5 🧙, on-death, target ally: summon a 1/1 skeleton in the dead unit's
    // slot, or give +1/+1 to the skeleton that already exists.
    // Needs summoning: an effect that adds a unit to a team mid-battle.
    // Needs a token kind: a 1/1 skeleton 💀 the shop never offers, held in its own manifest
    // collection rather than marked by a flag on UnitDefinition.
    // Needs an existence condition: a trigger tests a scope against a participant, not whether
    // the board holds a unit. One ally death fires every necromancer on the team, and
    // capture-at-queue means each sees no skeleton, so each summons one.

    // TODO: Basilisk 3/5 🐍, on-attack, target self: petrify the attacker, which skips one attack.
    // Needs status effects: unit state the scheduler reads.

    // TODO: Coinbug 1/4 🪲, on-death, target self: +1 gold.
    // Needs the run side effect system: an effect that changes the run.

    // TODO: Vulture 2/5 🦅, on-kill, source self: +1 gold.
    // Needs the run side effect system.

    public static readonly ContentManifest Manifest = new()
    {
        Version = "2",
        Units = [Golem, Ghoul, Wyrm, Vampire, Goblin, Devourer, Wraithblade, Deathcap]
    };

    public static readonly Roster Roster = Manifest.ToRoster();

    // Random.Shared.GetItems takes an array, which Units is not.
    public static readonly UnitDefinition[] Pool = [.. Manifest.Units];

    private static Ability DevourerAbility()
    {
        Ahead<StartEvent> ahead = new() { Anchor = One<StartEvent>.Of(new Self()) };
        One<StartEvent> eaten = One<StartEvent>.Of(ahead);

        return new Ability<StartEvent>
        {
            Trigger = new RoundTrigger<StartEvent>(),
            Effects =
            [
                new ScopedEffect<StartEvent>
                {
                    Effect = new StatChange<StartEvent>
                    {
                        Attack = new UnitStat<StartEvent> { Subject = eaten, Stat = Stat.Attack },
                        Health = new UnitStat<StartEvent> { Subject = eaten, Stat = Stat.Health }
                    },
                    Scopes = [Every<StartEvent>.Of(new Self())]
                },
                new ScopedEffect<StartEvent>
                {
                    // Lethal by construction: damage equals the recipient's health.
                    Effect = new Damage<StartEvent> { Value = new RecipientStat { Stat = Stat.Health } },
                    Scopes = [new Every<StartEvent> { Relation = ahead, Range = ScopeRange.At(0) }]
                }
            ]
        };
    }
}
