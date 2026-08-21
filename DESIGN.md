# Design

Backend spec for a monster autobattler with asynchronous matchmaking, inspired by Super Auto Pets.

Monsters have an Attack stat, a Max_Health stat and an Ability.

- Shop phase: buy monsters, reorder the team, sell monsters back.
- Battle phase: monsters fight in a queue. Heads attack each tick, abilities react.
- Player health drops on a loss. Victory points on a win.

## Scope

**In**

- Server authoritative: all state and resolution live on the server.
- Mutations are idempotent and rejected if the client is out of date, with resync.
- Asynchronous battles against stored team snapshots.
- Battles return an event log to replay in browser.

**Later**

- Shop abilities, and the run-scoped triggers they need.
- Freezing shop slots.
- Content version resolution.
- Damage reduction.
- Items, food, merging.
- Art instead of emoji.
- Arithmetic in values, and generated ability text.

**Out**

- Real time multiplayer.
- Matchmaking fairness: you get any run on the same stage.
- Authentication: a run GUID in local storage, resumed if valid.

## Theme

Monsters and undead, drawn as emoji.

## Terminology

| Term           | Means                                                  |
|----------------|--------------------------------------------------------|
| Run            | One playthrough, addressed by an id                    |
| Stage          | One round: a shop phase, then a battle                 |
| Unit           | A monster with its own stats                           |
| Team           | An ordered queue of units                              |
| Ghost          | A stored snapshot of another player's team             |
| Slot           | A position in a queue, 0 is the head                   |
| Content        | The versioned data defining units and abilities        |
| Ability        | The rules text on a unit. One trigger and one effect   |
| Trigger        | An event and a scope                                   |
| Effect         | A change and a scope                                   |
| Scope          | A set of units, evaluated when its trigger fires       |
| `self`         | The unit an ability belongs to                         |
| `event.source` | The unit that caused an event                          |
| `event.target` | The unit an event happened to                          |
| Attack         | A head striking the opposing head, queued by the scheduler |
| Event          | A record of something that happened                    |
| Tick           | One exchange between the two heads                     |
| Subtick        | One wave of resolution. One animation batch            |

## Combat

Teams are queues. The two heads fight each other. Combat runs on a copy of both teams; only the outcome goes back to
the run.

### Loop

1. Battle starts.
2. **Tick** — repeat until one side is empty or the tick cap is reached:
    1. Queue an `attack` and a `damage` effect from each head at the other.
    2. **Subtick** — repeat until the queue is empty or the subtick cap is reached:
        1. For each effect in the queue, in order:
            1. Apply it.
            2. Scan every unit and test its triggers against this effect. Queue results into the next subtick.
        2. Mark every unit at or below 0 health as dead.
        3. Fire on-faint triggers, queuing results into the next subtick.
        4. Compact both queues.
        5. Emit this subtick's effects as one batch.
3. Resolve the outcome.

### Rules

- The scheduler is the sole source of `attack`.
- Health changes accumulate through a subtick and are read once at its boundary.
- On-faint triggers fire while the unit still holds its slot, before compaction.
- A unit stops firing triggers when it dies.
- A dead unit stays readable, so its stats still resolve. Anything targeting it is dropped.
- An effect resolves even if its owner is dead.
- An effect whose targets are all dead passes silently.
- Iteration order is P1, G1, P2, G2, and applies to every scan and application.
- A battle ends when one side is empty, or in a draw on the tick cap or a mutual wipe.
- Subticks and ticks are capped. Hitting a cap discards the queue and logs server-side.
- Battles are reproducible from the teams, content version, seed and run state read.

## Events

| Event          | Emitted when                  | source   | target       | value        |
|----------------|-------------------------------|----------|--------------|--------------|
| `OnStart`      | Once, before the first attack | -        | -            | -            |
| `OnUnitAttack` | An `attack` effect resolves   | Attacker | Struck unit  | Damage dealt |
| `OnUnitHurt`   | A unit takes damage           | Dealer   | Damaged unit | Damage dealt |
| `OnUnitFaint`  | A unit is marked dead         | Killer   | Dead unit    | -            |

- Records carry tick, subtick, and the slots of source and target.
- The log is flat. The client groups by tick and subtick.

## Triggers

- A trigger is an event and a scope.
- Round triggers fire once. Unit triggers fire per matching event.
- All triggers are combat-scoped. Stage rewards are handler logic.

## Scopes

- Used as "any" in TRIGGERs and "for each" in EFFECTs
- primary scopes: self, absolute(side,range), relative(scopes,range)
- event scopes for effects: event.source, event.target, random(count,scopes)
- ranges are singletons: [0], lists: [0,1], or open ended "..2"
- scopes are always handled as arrays, unioned, and resolved in iteration order

## Values

- An effect's magnitude is a literal or a single field read.
- Readable: self, event.source, event.value, run state.

## Abilities

| Ability | Trigger | Effect |
|---------|---------|--------|
| TODO    |         |        |

## Client

- Renders the event log and holds no game logic.
- A subtick plays as one batch. Movers interpolate from their last slot to this one.
- Subtick duration decays per subtick and per tick, with a floor.
- Start at 30 frames, 0.90 per subtick, 0.95 per tick, floor 10 frames. All tunable.
- Skip pins every duration to the floor and keeps playing.

## Data model

- DynamoDB, keys designed from access patterns.
- Stores runs, team snapshots, resolved battles, and content.
- A run holds team, shop, currency, stage and version.
- A stored battle holds everything needed to reproduce it.
- Content is loaded into memory at startup and cached.

## Concurrency

- Every mutation carries a version and an idempotency key.
- The version check is atomic with the write, not a read then a write.
- A stale mutation is rejected with the current run state, so the client resyncs in one trip.
- A repeated key returns the original response.

## API

| Method | Path                      | Does                                                       |
|--------|---------------------------|------------------------------------------------------------|
| GET    | `/content`                | The manifest and its version                               |
| POST   | `/runs`                   | Start a run                                                |
| GET    | `/runs/{id}`              | The run. Serves resume and resync                          |
| POST   | `/runs/{id}/battle`       | End the stage: match, resolve, return the log and rewards  |
| POST   | `/runs/{id}/shop/roll`    | Reroll                                                     |
| POST   | `/runs/{id}/shop/buy`     | Buy into a team slot                                       |
| POST   | `/runs/{id}/team/reorder` | Set the queue order                                        |
| POST   | `/runs/{id}/team/sell`    | Sell for currency                                          |

- A route group per module. Version and idempotency are filters on the shared parent.
- The client sends intents, never outcomes.
