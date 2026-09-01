import { useCallback, useEffect, useRef, useState } from "react";
import { PixelButton } from "./PixelButton.jsx";
import confetti from "canvas-confetti";
import { gsap } from "gsap";
import { UnitCard } from "./UnitCard.jsx";
import { useReplay } from "../replay.js";

const TITLES = { win: "Victory", loss: "Defeat", draw: "Draw" };
const SETTLE_MS = 900;

export function BattleScreen({ game }) {
  const { pending, definition } = game;
  const [over, setOver] = useState(false);
  const arena = useRef(null);

  // The board holds for a beat after the last blow, then the result joins it below rather
  // than replacing it, so the final positions stay on screen.
  const onDone = useCallback(() => {
    setTimeout(() => {
      game.settle();
      setOver(true);
    }, SETTLE_MS);
  }, [game]);

  const { board, clock, register } = useReplay(pending.record, onDone);

  useEffect(() => {
    if (!arena.current) return;
    const sides = arena.current.querySelectorAll("[data-army]");
    gsap.from(sides, {
      x: (index) => (index === 0 ? -140 : 140),
      opacity: 0,
      duration: 0.45,
      ease: "power2.out",
    });
  }, []);

  useEffect(() => {
    if (over && pending.record.outcome === "win") {
      confetti({ particleCount: 160, spread: 75, origin: { y: 0.7 }, scalar: 0.9 });
    }
  }, [over, pending.record.outcome]);

  return (
    <>
      <div ref={arena} className="flex items-center justify-center gap-6 mt-4">
        <Army
          title="YOU" ids={board.order.player} board={board}
          definition={definition} register={register} facing="right"
        />
        <span className="text-3xl shrink-0">⚔️</span>
        <Army
          title="ENEMY" ids={board.order.opponent} board={board}
          definition={definition} register={register} facing="left"
        />
      </div>

      <div className="flex items-center justify-center mt-6 h-6">
        {!over && <span className="text-[10px] text-parchment/40">{clock}</span>}
      </div>

      {over && <Result game={game} />}
    </>
  );
}

// Each side takes half the arena and packs toward the middle, so the two front units meet at
// the centre however lopsided the rosters are.
function Army({ title, ids, board, definition, register, facing }) {
  return (
    <div
      data-army={facing}
      className={`flex-1 min-w-0 flex flex-col ${facing === "right" ? "items-end" : "items-start"}`}
    >
      <h2 className={`text-gold text-xs tracking-widest mb-2 ${facing === "right" ? "text-right" : ""}`}>
        {title}
      </h2>
      {/* Fixed to a full team's width so the arena keeps its shape as units die, and the
          survivors stay packed against the middle rather than the whole box collapsing. */}
      <div
        className={`flex items-center gap-3 px-4 py-4 border-4 border-black bg-panel
          w-[29rem] h-[7.5rem] max-w-full shrink-0
          ${facing === "right" ? "flex-row-reverse" : ""}`}
        style={{ boxShadow: "8px 8px 0 #000" }}
      >
        {ids.map((id) => (
          <UnitCard
            key={id}
            ref={(element) => register(id, element)}
            unit={board.units.get(id)}
            definition={definition}
            size="battle"
          />
        ))}
      </div>
    </div>
  );
}

function Result({ game }) {
  const { pending, run } = game;
  const finished = run.finished;
  const outcome = pending.record.outcome;

  return (
    <div
      className="max-w-md mx-auto mt-8 px-6 py-5 border-4 border-black bg-panel text-center"
      style={{ boxShadow: "8px 8px 0 #000" }}
    >
      <h2 className={`text-2xl mb-2 ${outcome === "win" ? "text-moss" : outcome === "loss" ? "text-blood" : "text-gold"}`}>
        {TITLES[outcome] ?? outcome}
      </h2>
      <p className="text-xs text-parchment/70 mb-4">
        {finished
          ? `+${pending.goldEarned} gold. Run over: ${run.victories} of ${run.stage - 1} stages won.`
          : `+${pending.goldEarned} gold. On to stage ${run.stage}.`}
      </p>
      <PixelButton tone="gold" onClick={game.advance}>
        {finished ? "New run" : "Continue"}
      </PixelButton>
    </div>
  );
}
