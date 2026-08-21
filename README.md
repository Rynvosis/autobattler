# autobattler

A small, server-authoritive, asynchronously matched autobattler game hosted on AWS. Super auto pets is the reference
used for this game, credit to Team Wood Games for many of the design decisions I have made here.

## What it is

- Buy monsters in a shop, order them into a queue, then watch the queue fight automatically.
- Opponents are stored snapshots of other players' teams, not live players.
- Lose and your player health drops. Win and you take victory points.
- Units are emoji. The whole roster is a JSON file.

## How it works

- The game is Server authoritative, the Combat, shop rolls and rewards all resolve server-side. The client shows two teams, sends
  "fight", and animates the reply for an recieved event log.
- Battles are asynchronous, there are no live opponent and no realtime networking.
 
Rules in [DESIGN.md](DESIGN.md).

## Stack

| Layer   | Choice                                  |
|---------|-----------------------------------------|
| API     | C# on .NET 8, ASP.NET Core Minimal APIs |
| Compute | AWS Lambda behind API Gateway           |
| Infra   | AWS SAM                                 |
| Storage | DynamoDB                                |
| Client  | Plain HTML, CSS and JS                  |
| CI      | GitHub Actions                          |

.NET 8 because that is what Lambda's managed runtime supports.

## Layout

| Directory | Contents                                            |
|-----------|-----------------------------------------------------|
| `api/`    | The C# service. Combat, rosters, progression        |
| `web/`    | The browser client. No framework, no bundler, no npm |
| `infra/`  | SAM template, tables, deployment                    |

## Running it

TODO once there is something to run.

## Use of AI

- `api/` and `infra/` are hand-written. They are the parts this is built to learn.
- `web/` is an AI-assisted frontend, it holds no game logic by design.

