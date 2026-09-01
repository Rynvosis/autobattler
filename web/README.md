# web

The browser client: React and Vite, styled with Tailwind, holding no game rules of its own. It
renders the run the server sends, sends intents, and replays the battle log.

## Running it

The API serves this directory, so there is one origin and no CORS.

```sh
npm install                     # once
npm run build                   # writes web/dist, which the API serves
docker compose up -d
dotnet run --project api/Api
```

- `http://localhost:5023/` — the game.
- `http://localhost:5023/swagger` — the API.

`npm run dev` serves the source with hot reload on Vite's own port and proxies `/runs` and
`/content` to `localhost:5023`, which is the better loop while building.

## Layout

Source lives in `app/`; the build lands in `web/dist`, which is the directory the API serves.

| Path                         | Holds                                                            |
|------------------------------|------------------------------------------------------------------|
| `app/src/api.js`             | The endpoints, one function each. Maps 400/404/409 to errors      |
| `app/src/useGame.js`         | The run, every intent, and the optimistic placement on a buy      |
| `app/src/replay.js`          | Plays the battle log by tick and subtick, and the GSAP animation  |
| `app/src/App.jsx`            | Screen flow, the top bar and the tooltip                          |
| `app/src/components/`        | The board, the shop, the arena, the unit and the button           |

## Third party

`@dnd-kit` for dragging, `gsap` for the replay, `react-tooltip`, `canvas-confetti`, and
`pixel-retroui` for its Minecraft font. Its stylesheet is deliberately not imported: it ships an
unlayered Tailwind preflight that overrides this project's own utilities.

## Notes

- The client never computes a price or a rule. A buy is optimistic only about *where* the monster
  lands, never about the gold; the server's response settles everything else.
- Buying into a chosen slot is a `buy` followed by a `reorder`, since the API's buy appends and
  takes no slot.
