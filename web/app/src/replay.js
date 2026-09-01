import { useCallback, useEffect, useRef, useState } from "react";
import { flushSync } from "react-dom";
import { gsap } from "gsap";

// `base` and `floor` are frames at 60fps. DESIGN sets the shape; these values are tuned from
// the defaults it gives.
const PACING = { base: 42, subtickDecay: 0.94, tickDecay: 0.98, floor: 22, fps: 60 };
const DEATH_HOLD_MS = 260;

// A beat at the end of every tick, so rounds land as rounds instead of running together.
const TICK_GAP_MS = 240;
const CLOSE_GAP_MS = 340;

function durationMs(tick, subtick) {
  const frames = Math.max(
    PACING.floor,
    Math.round(
      PACING.base *
        PACING.tickDecay ** Math.max(tick - 1, 0) *
        PACING.subtickDecay ** Math.max(subtick - 1, 0),
    ),
  );
  return (frames / PACING.fps) * 1000;
}

const sleep = (ms) => new Promise((resolve) => setTimeout(resolve, ms));

// A subtick is one animation batch.
function batches(events) {
  const out = [];
  for (const event of events) {
    const last = out.at(-1);
    if (last && last.tick === event.tick && last.subtick === event.subtick) last.events.push(event);
    else out.push({ tick: event.tick, subtick: event.subtick, events: [event] });
  }
  return out;
}

// Closing a gap in the line is a move, not a jump. Positions are measured, the row is
// rebuilt synchronously so the new ones can be measured too, and every survivor is then slid
// from where it stood to where it now stands. Both passes read every rect before writing any
// transform, because interleaving the two forces a layout per card.
async function closeGaps(cards, compact) {
  const before = new Map();
  for (const [id, element] of cards) before.set(id, element.getBoundingClientRect().left);

  flushSync(compact);

  const shifts = [];
  for (const [id, element] of cards) {
    const from = before.get(id);
    if (from === undefined) continue;

    const shift = from - element.getBoundingClientRect().left;
    if (Math.abs(shift) >= 1) shifts.push([element, shift]);
  }

  for (const [element, shift] of shifts) {
    gsap.fromTo(element, { x: shift }, { x: 0, duration: CLOSE_GAP_MS / 1000, ease: "power2.inOut" });
  }

  if (shifts.length) await sleep(CLOSE_GAP_MS);
}

// The units still standing in a row, dead or alive — a corpse counts until it is compacted out.
function standing(state) {
  return [...state.order.player, ...state.order.opponent].map((id) => state.units.get(id));
}

function openingBoard(record) {
  const units = new Map();
  for (const side of ["player", "opponent"]) {
    for (const unit of record[side]) units.set(unit.id, { ...unit, side });
  }
  return {
    units,
    order: {
      player: record.player.map((unit) => unit.id),
      opponent: record.opponent.map((unit) => unit.id),
    },
  };
}

export function useReplay(record, onDone, onBounty) {
  const [board, setBoard] = useState(() => openingBoard(record));
  const [clock, setClock] = useState("");
  const cards = useRef(new Map());
  const started = useRef(false);

  // `onDone` closes over the whole game and is rebuilt every render, so depending on it would
  // let any mid-battle state change rerun this effect — whose cleanup abandons the loop it
  // started. Held by reference, the battle depends on the log alone.
  const done = useRef(onDone);
  done.current = onDone;

  const register = useCallback((id, element) => {
    if (element) cards.current.set(id, element);
    else cards.current.delete(id);
  }, []);

  useEffect(() => {
    if (started.current) return;
    started.current = true;

    let live = true;
    const state = openingBoard(record);
    const flights = new Set();

    (async () => {
      const groups = batches(record.events);

      for (const [index, batch] of groups.entries()) {
        if (!live) return;
        const ms = durationMs(batch.tick, batch.subtick);
        setClock(`tick ${batch.tick} · subtick ${batch.subtick}`);

        for (const event of batch.events)
          applyEvent(event, state, cards.current, ms, flights, onBounty);
        setBoard({ units: new Map(state.units), order: { ...state.order } });
        await sleep(ms);

        // Bodies clear at the end of the tick, not the subtick. A unit's death and everything
        // its death triggers resolve within one tick, so holding the row until the tick is
        // over keeps the corpse's card in place for its own on-death effects to fly from.
        const next = groups[index + 1];
        if (next && next.tick === batch.tick) continue;

        await sleep(TICK_GAP_MS);
        if (!standing(state).some((unit) => unit.dead)) continue;

        await sleep(DEATH_HOLD_MS);

        // Removing a unit shifts every survivor along, so nothing may be in the air.
        while (flights.size) await Promise.all([...flights]);
        if (!live) return;

        for (const side of ["player", "opponent"]) {
          state.order[side] = state.order[side].filter((id) => !state.units.get(id).dead);
        }
        await closeGaps(cards.current, () => {
          setBoard({ units: new Map(state.units), order: { ...state.order } });
        });
      }

      if (!live) return;
      setClock("");
      done.current();
    })();

    return () => { live = false; };
  }, [record, onBounty]);

  return { board, clock, register };
}

function applyEvent(event, state, cards, ms, flights, onBounty) {
  const target = state.units.get(event.target);
  const source = state.units.get(event.source);

  // An ability's pellet is the cause arriving, so its number waits for the pellet to land.
  // A strike's pellet was thrown on the previous subtick and has already landed by now.
  const travel = throwAbilityPellet(event, cards, ms, flights);
  const on = (show) => (travel ? travel.then(show) : show());

  switch (event.type) {
    case "unitAttack":
      lunge(cards.get(event.source), source.side === "player" ? 1 : -1, ms);
      throwPellet(cards.get(event.source), cards.get(event.target), ms, "pellet-damage", flights);
      break;
    case "unitHurt":
      target.health -= event.value;
      on(() => {
        hurt(cards.get(event.target), ms);
        float(cards.get(event.target), -event.value, "♥", "float-damage", "health", ms);
      });
      break;
    case "unitAttackChange":
      target.attack += event.value;
      on(() => float(cards.get(event.target), event.value, "⚔", "float-attack", "attack", ms));
      break;
    case "unitHealthChange":
      target.health += event.value;
      on(() => float(cards.get(event.target), event.value, "♥",
        event.value < 0 ? "float-damage" : "float-heal", "health", ms));
      break;
    case "unitDeath":
      target.dead = true;
      die(cards.get(event.target), ms);
      break;
    // A ghost's bounty has no purse to fall into: the server pays the player's units only.
    case "unitBounty":
      if (target.side === "player") {
        collect(cards.get(event.target), event.value, ms, flights, onBounty);
      }
      break;
  }
}

// The purse must not rise before the coin arrives, so the count waits on the flight.
function collect(card, value, ms, flights, onBounty) {
  const purse = document.getElementById("gold-pip");
  const landed = throwPellet(card, purse, ms, "pellet-coin", flights);

  if (!landed) return onBounty(value);

  landed.then(() => {
    onBounty(value);
    float(purse, value, "🪙", "float-attack", "attack", ms);
  });
}

function throwAbilityPellet(event, cards, ms, flights) {
  if (event.cause?.kind !== "ability") return null;
  flash(cards.get(event.cause.unit), ms);
  if (event.cause.unit === event.target) return null;
  return throwPellet(cards.get(event.cause.unit), cards.get(event.target), ms, "pellet-buff", flights);
}

function lunge(element, direction, ms) {
  if (!element) return;
  gsap.fromTo(element, { x: 0 }, { x: direction * 20, duration: ms / 2200, yoyo: true, repeat: 1, ease: "power2.out" });
}

function hurt(element, ms) {
  if (!element) return;
  gsap.fromTo(element, { x: -9 }, { x: 0, duration: ms / 900, ease: "elastic.out(1, 0.3)" });
}

function flash(element, ms) {
  if (!element) return;
  gsap.fromTo(element, { filter: "brightness(2.4)" }, { filter: "brightness(1)", duration: ms / 700 });
}

function die(element, ms) {
  if (!element) return;
  gsap.to(element, { opacity: 0, scale: 0.6, rotate: 12, duration: (ms + DEATH_HOLD_MS) / 1000, ease: "back.in(2)" });
}

// A pellet is thrown on the attack and lands on the hurt a subtick later, so the arc reads as
// the blow travelling rather than damage simply appearing.
function throwPellet(from, to, ms, tone, flights) {
  if (!from || !to) return null;
  const fromBox = from.getBoundingClientRect();
  const toBox = to.getBoundingClientRect();
  const start = { x: fromBox.left + fromBox.width / 2, y: fromBox.top + fromBox.height / 2 };
  const end = { x: toBox.left + toBox.width / 2, y: toBox.top + toBox.height / 2 };
  const peak = -Math.max(56, Math.abs(end.x - start.x) * 0.3);

  const dot = document.createElement("div");
  dot.className = `pellet ${tone}`;
  document.body.appendChild(dot);

  // The flight is registered so the row can hold its layout until every pellet has landed.
  // Its endpoints are screen coordinates taken now, so compacting a dead unit out of the row
  // mid-flight would leave the pellet aimed at where its target used to stand.
  const landed = new Promise((resolve) => {
    gsap.fromTo(dot,
      { x: start.x, y: start.y, scale: 0.4, opacity: 0.3 },
      {
        keyframes: [
          { x: (start.x + end.x) / 2, y: (start.y + end.y) / 2 + peak, scale: 1, opacity: 1, duration: ms / 2000, ease: "power1.out" },
          { x: end.x, y: end.y, scale: 0.6, duration: ms / 2000, ease: "power1.in" },
        ],
        onComplete: () => {
          dot.remove();
          resolve();
        },
      });
  });

  flights.add(landed);
  landed.then(() => flights.delete(landed));
  return landed;
}

// Each unit keeps two running columns, attack on the left and health on the right. Deltas
// stack in the column that owns them, so five goblins gaining +2/+2 on one subtick read as a
// countable list per stat rather than numbers piled on a single point.
function columnFor(element, side) {
  const existing = element.querySelector(`.float-col-${side}`);
  if (existing) return existing;

  const created = document.createElement("div");
  created.className = `float-col float-col-${side}`;
  element.appendChild(created);
  return created;
}

function float(element, value, glyph, tone, side, ms) {
  if (!element) return;

  const span = document.createElement("span");
  span.className = `float-value ${tone}`;
  span.textContent = `${value > 0 ? "+" : "−"}${Math.abs(value)}${glyph}`;
  columnFor(element, side).appendChild(span);

  const seconds = Math.max(ms * 2, 800) / 1000;

  gsap.fromTo(span,
    { opacity: 0, scale: 0.6, x: side === "attack" ? -10 : 10 },
    {
      opacity: 1, scale: 1, x: 0,
      duration: seconds * 0.3,
      ease: "back.out(2.5)",
      onComplete: () => gsap.to(span, {
        opacity: 0,
        duration: seconds * 0.45,
        delay: seconds * 0.35,
        onComplete: () => span.remove(),
      }),
    });
}
