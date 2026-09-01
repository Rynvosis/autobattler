import { useRef, useState } from "react";
import { PixelButton } from "./PixelButton.jsx";
import {
  DndContext, DragOverlay, PointerSensor, pointerWithin, useDraggable, useDroppable, useSensor, useSensors,
} from "@dnd-kit/core";
import { SortableContext, horizontalListSortingStrategy, useSortable } from "@dnd-kit/sortable";
import { UnitCard, Plinth } from "./UnitCard.jsx";

const TEAM_SIZE = 5;

const PADS = [
  { id: "upgrade", label: "Upgrade", icon: "⬆️", tip: "Drop a unit here for +1/+1. Spends a credit if you hold one, otherwise 2 gold." },
  { id: "duplicate", label: "Duplicate", icon: "🧬", tip: "Drop a unit here to append a copy, upgrades and all. Once per stage." },
  { id: "sell", label: "Sell", icon: "🗑️", tip: "Drop a unit here to bank one upgrade credit." },
];

export function ShopScreen({ game }) {
  const { run, definition } = game;
  const [held, setHeld] = useState(null);
  const sensors = useSensors(useSensor(PointerSensor, { activationConstraint: { distance: 5 } }));

  const credits = run.upgradeCredits;
  const holdingUnit = held?.startsWith("team:");

  // Where a dragged shop monster would land, with the card middles and the row that decide it.
  // Both are measured once, at the start of the drag: opening the gap moves every card, so
  // measuring live would let the placeholder shift the very middles that positioned it and
  // oscillate under a still cursor.
  const [landing, setLanding] = useState(null);
  const row = useRef(null);
  const middles = useRef([]);
  const rowBox = useRef(null);

  const padStates = {
    upgrade: {
      cost: credits > 0 ? `free · ${credits}✨` : "2🪙",
      unavailable: run.units.length === 0 || (credits === 0 && run.gold < 2),
    },
    duplicate: {
      cost: "6🪙",
      unavailable: run.units.length === 0 || run.units.length >= TEAM_SIZE
        || run.duplicated || run.gold < 6,
    },
    sell: { cost: "+1✨", unavailable: run.units.length <= 1 },
  };

  function onDragStart({ active }) {
    setHeld(String(active.id));
    rowBox.current = row.current.getBoundingClientRect();
    middles.current = [...row.current.querySelectorAll("[data-slot]")].map((card) => {
      const box = card.getBoundingClientRect();
      return box.left + box.width / 2;
    });
  }

  // The board is drawn right to left, so a monster belongs behind every card whose middle the
  // cursor has not yet passed.
  function onDragMove({ active, activatorEvent, delta }) {
    if (!String(active.id).startsWith("shop:")) return;

    // A full team has no gap to open and the server would refuse the buy anyway.
    if (run.units.length >= TEAM_SIZE) return;

    const box = rowBox.current;
    const x = activatorEvent.clientX + delta.x;
    const y = activatorEvent.clientY + delta.y;

    if (x < box.left || x > box.right || y < box.top || y > box.bottom) return setLanding(null);

    setLanding(middles.current.filter((middle) => middle > x).length);
  }

  function onDragEnd({ active, over }) {
    const [source, slot] = String(active.id).split(":");
    const droppedAt = landing;

    setHeld(null);
    setLanding(null);

    if (source === "shop") {
      if (droppedAt !== null) game.buyInto(Number(slot), droppedAt);
      return;
    }

    if (!over) return;
    const target = String(over.id);

    if (!target.startsWith("team:")) {
      game[target](Number(slot));
      return;
    }

    const onto = Number(target.split(":")[1]);
    if (Number(slot) !== onto) game.reorder(Number(slot), onto);
  }

  return (
    <DndContext
      sensors={sensors}
      // Only a droppable actually under the pointer counts, so letting go anywhere else is a
      // cancel rather than a purchase at the nearest slot.
      collisionDetection={pointerWithin}
      onDragStart={onDragStart}
      onDragMove={onDragMove}
      onDragCancel={() => { setHeld(null); setPreview(null); }}
      onDragEnd={onDragEnd}
    >
      <Board run={run} definition={definition} landing={landing} rowRef={row} />

      <Shelf run={run} definition={definition} game={game} />

      <div className="flex flex-wrap gap-3 justify-center mt-6">
        {PADS.map((pad) => (
          <Pad key={pad.id} {...pad} {...padStates[pad.id]} armed={holdingUnit} />
        ))}
      </div>

      <DragOverlay dropAnimation={null}>
        {held ? <Carried id={held} run={run} definition={definition} /> : null}
      </DragOverlay>
    </DndContext>
  );
}

// Slot 0 is the head and is drawn on the right, the end that meets the enemy, so the queue
// reads the same here as it does on the battle screen. The array is reversed for rendering
// rather than the row being flipped with row-reverse, because dnd-kit measures geometry and
// expects the DOM order to be the order on screen.
function Board({ run, definition, landing, rowRef }) {
  const slots = Array.from({ length: TEAM_SIZE }, (_, index) => TEAM_SIZE - 1 - index);
  const taken = slots.filter((slot) => slot < run.units.length);

  // The gap is paid for out of a spare slot, so the row keeps its width as it moves.
  const drawOrder = [...slots];
  if (landing !== null) {
    drawOrder.shift();
    drawOrder.splice(drawOrder.length - landing, 0, "gap");
  }

  return (
    <section className="text-center">
      <h2 className="text-gold text-xs tracking-widest mb-1">YOUR TEAM</h2>
      <p className="text-[10px] text-parchment/50 mb-3">
        The front of the queue, on the right, strikes first. Drag onto the board to place, or
        drop anywhere else to cancel.
      </p>

      <SortableContext items={taken.map((slot) => `team:${slot}`)} strategy={horizontalListSortingStrategy}>
        <div
          ref={rowRef}
          className="inline-flex items-center gap-8 px-8 py-8 border-4 border-black bg-panel"
          style={{ boxShadow: "8px 8px 0 #000" }}
        >
          {drawOrder.map((slot, index) =>
            slot === "gap"
              ? <Plinth key="gap" label="here" highlight />
              : slot < run.units.length
                ? <Fighter key={slot} slot={slot} unit={run.units[slot]} definition={definition} />
                : <OpenSlot key={`open-${index}`} slot={slot} />,
          )}

          <span className="pl-2 text-[10px] text-gold whitespace-nowrap">FRONT ▶</span>
        </div>
      </SortableContext>
    </section>
  );
}

// A drop target for a team card sent to the back. It stays unlit: the gap is what says where
// anything lands, and a second highlight only competes with it.
function OpenSlot({ slot }) {
  const { setNodeRef } = useDroppable({ id: `team:${slot}` });
  return (
    <div ref={setNodeRef}>
      <Plinth label="empty" />
    </div>
  );
}

function Fighter({ slot, unit, definition }) {
  const { attributes, listeners, setNodeRef, transform, transition, isDragging } =
    useSortable({ id: `team:${slot}` });

  return (
    <UnitCard
      ref={setNodeRef}
      unit={unit}
      definition={definition}
      dragging={isDragging}
      data-slot={slot}
      className="cursor-grab active:cursor-grabbing"
      style={{
        transform: transform ? `translate3d(${transform.x}px, ${transform.y}px, 0)` : undefined,
        transition,
      }}
      {...attributes}
      {...listeners}
    />
  );
}

function Shelf({ run, definition, game }) {
  return (
    <section className="text-center mt-8">
      <h2 className="text-gold text-xs tracking-widest mb-1">SHOP</h2>
      <p className="text-[10px] text-parchment/50 mb-3">Drag a monster onto the board to buy it.</p>

      <div
        className="inline-flex items-center gap-8 px-8 py-8 border-4 border-black bg-slate"
        style={{ boxShadow: "8px 8px 0 #000" }}
      >
        {run.shop.map((offer, slot) =>
          offer
            ? <Offer key={slot} slot={slot} unit={offer} definition={definition} />
            : <Plinth key={slot} label="sold" />,
        )}

        <div className="flex flex-col gap-2 pl-4 ml-2 border-l-4 border-black/40">
          <PixelButton disabled={run.gold < 1} onClick={game.reroll}>
            🎲 Roll <small className="opacity-70">1🪙</small>
          </PixelButton>
          <PixelButton tone="gold" disabled={run.units.length === 0} onClick={game.fight}>
            ⚔️ Fight
          </PixelButton>
        </div>
      </div>
    </section>
  );
}

function Offer({ slot, unit, definition }) {
  const { attributes, listeners, setNodeRef, isDragging } = useDraggable({ id: `shop:${slot}` });

  return (
    <UnitCard
      ref={setNodeRef}
      unit={unit}
      definition={definition}
      dragging={isDragging}
      className="cursor-grab active:cursor-grabbing"
      {...attributes}
      {...listeners}
    />
  );
}

// A pad lights up only while a unit is in hand, so the board is uncluttered the rest of the time.
function Pad({ id, label, icon, cost, tip, unavailable, armed }) {
  const { setNodeRef, isOver } = useDroppable({ id });

  return (
    <div
      ref={setNodeRef}
      data-tooltip-id="unit-tip"
      data-tooltip-content={tip}
      className={`w-32 px-3 py-2 border-4 border-dashed grid place-items-center text-center transition-all
        ${unavailable ? "opacity-30" : armed ? "opacity-100" : "opacity-60"}
        ${isOver ? "border-gold bg-gold/15 -translate-y-1" : "border-parchment/30"}`}
    >
      <span className="text-xl leading-none">{icon}</span>
      <span className="text-[11px]">{label}</span>
      <span className="text-[9px] text-parchment/60">{cost}</span>
    </div>
  );
}

// The card under the cursor. dnd-kit renders it in a portal, so the grab point stays exact.
function Carried({ id, run, definition }) {
  const [source, slot] = id.split(":");
  const unit = source === "shop" ? run.shop[Number(slot)] : run.units[Number(slot)];
  return <UnitCard unit={unit} definition={definition} tip={false} className="rotate-3 scale-105" />;
}
