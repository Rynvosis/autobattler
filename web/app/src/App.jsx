import { Tooltip } from "react-tooltip";
import { PixelButton } from "./components/PixelButton.jsx";
import { useGame } from "./useGame.js";
import { ShopScreen } from "./components/ShopScreen.jsx";
import { BattleScreen } from "./components/BattleScreen.jsx";

const TOTAL_STAGES = 5;

export default function App() {
  const game = useGame();

  return (
    <>
      <TopBar game={game} />
      <main className="max-w-6xl mx-auto px-5 py-8">
        {game.screen === "start" && <StartScreen game={game} />}
        {game.screen === "shop" && <ShopScreen game={game} />}
        {game.screen === "battle" && <BattleScreen game={game} />}
        {game.error && <p className="text-blood text-xs text-center mt-6">{game.error}</p>}
      </main>
      <Tooltip
        id="unit-tip"
        className="tooltip-retro"
        place="top"
        delayShow={120}
        render={({ activeAnchor }) => describe(activeAnchor, game.definition)}
      />
    </>
  );
}

// react-tooltip v6 dropped the `html` prop, so rich content comes from `render` instead.
function describe(anchor, definition) {
  const kind = anchor?.getAttribute("data-kind");
  if (!kind) return anchor?.getAttribute("data-tooltip-content");

  const monster = definition(kind);
  return (
    <div className="text-left">
      <div className="text-sm mb-2">{monster.icon} {monster.name}</div>
      {/* The swatches match the badges on the card, so which corner is which is never a guess. */}
      <div className="flex items-center gap-3 mb-2 text-[11px] tabular-nums">
        <span className="flex items-center gap-1">
          <i className="w-3 h-3 bg-[#ff9f43] border-2 border-black inline-block" />
          {anchor.getAttribute("data-attack")} attack
        </span>
        <span className="flex items-center gap-1">
          <i className="w-3 h-3 bg-blood border-2 border-black inline-block" />
          {anchor.getAttribute("data-health")} health
        </span>
      </div>
      {monster.tier > 1 && (
        <div className="text-[10px] text-parchment/60 mb-1">tier {monster.tier}</div>
      )}
      <div className="text-[11px] leading-relaxed">{monster.description}</div>
    </div>
  );
}

function TopBar({ game }) {
  const { run } = game;

  return (
    <header className="flex items-center gap-4 px-5 py-3 bg-panel border-b-4 border-black">
      <h1 className="text-sm text-gold flex-1">Autobattler</h1>
      {run && (
        <div className="flex gap-2">
          <Pip tip="Stage">🏁 {Math.min(run.stage, TOTAL_STAGES)}/{TOTAL_STAGES}</Pip>
          <Pip tip="Victories">🏆 {run.victories}</Pip>
          <Pip tip="Upgrade credits, banked by selling">✨ {run.upgradeCredits}</Pip>
          <Pip tip="Gold" gold>🪙 {run.gold}</Pip>
        </div>
      )}
      {run && (
        <PixelButton tone="ghost" onClick={() => game.abandonRun()}>Abandon</PixelButton>
      )}
    </header>
  );
}

function Pip({ children, tip, gold }) {
  return (
    <span
      data-tooltip-id="unit-tip"
      data-tooltip-content={tip}
      className={`px-3 py-1 text-sm tabular-nums border-4 border-black
        ${gold ? "bg-gold text-void" : "bg-slate text-parchment"}`}
      style={{ boxShadow: "3px 3px 0 #000" }}
    >
      {children}
    </span>
  );
}

function StartScreen({ game }) {
  return (
    <div
      className="max-w-lg mx-auto px-8 py-7 border-4 border-black bg-panel text-center"
      style={{ boxShadow: "8px 8px 0 #000" }}
    >
      <p className="text-5xl mb-4">🗿🐉🍄</p>
      <p className="text-sm mb-6 text-parchment/80">Buy monsters, order the queue, watch it fight.</p>
      <div className="flex gap-3 justify-center">
        <PixelButton tone="gold" onClick={game.start}>New run</PixelButton>
        {game.hasSavedRun && (
          <PixelButton onClick={game.resume}>Resume run</PixelButton>
        )}
      </div>
    </div>
  );
}
