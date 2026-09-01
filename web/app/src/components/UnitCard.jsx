import { forwardRef } from "react";

// The battle arena fits five a side within half the screen, so its art is smaller than the
// board's.
const SIZES = {
  board: { box: "w-28 h-32", art: "text-[4.5rem]", chip: "w-8 h-8 text-sm" },
  battle: { box: "w-[4.75rem] h-[5.5rem]", art: "text-[3rem]", chip: "w-6 h-6 text-[11px]" },
};

export const UnitCard = forwardRef(function UnitCard(
  { unit, definition, dragging, size = "board", tip = true, className = "", ...rest },
  ref,
) {
  const monster = definition(unit.kind);
  const sizing = SIZES[size];
  const dead = unit.dead;

  return (
    <div
      ref={ref}
      {...rest}
      className={`relative ${sizing.box} shrink-0 select-none flex flex-col items-center justify-end gap-1.5
        ${dragging ? "opacity-25" : ""} ${className}`}
      {...(tip && {
        "data-tooltip-id": "unit-tip",
        "data-kind": unit.kind,
        "data-attack": unit.attack,
        "data-health": unit.health,
      })}
    >
      <span
        className={`unit-art ${sizing.art} leading-none ${dead ? "opacity-30 grayscale" : ""}`}
        style={{ filter: dead ? undefined : "drop-shadow(3px 3px 0 rgb(0 0 0 / 0.55))" }}
      >
        {monster.icon}
      </span>

      {/* The chips sit below the art rather than over it, so nothing covers the monster. */}
      <div className="flex gap-1.5">
        <Chip className={`bg-[#ff9f43] ${sizing.chip}`}>{unit.attack}</Chip>
        <Chip className={`bg-blood ${sizing.chip}`}>{unit.health}</Chip>
      </div>
    </div>
  );
});

function Chip({ className, children }) {
  return (
    <span
      className={`grid place-items-center text-void border-[3px] border-black tabular-nums ${className}`}
      style={{ boxShadow: "2px 2px 0 rgb(0 0 0 / 0.6)" }}
    >
      {children}
    </span>
  );
}

// Always present, so the board keeps its shape as units come and go and a drop target is
// visible before anything is dragged.
export function Plinth({ highlight, label }) {
  return (
    <div
      className={`${SIZES.board.box} shrink-0 grid place-items-center border-4 border-dashed
        transition-colors ${highlight ? "border-gold bg-gold/10" : "border-parchment/35"}`}
    >
      <span className="text-[10px] text-parchment/35">{label}</span>
    </div>
  );
}
