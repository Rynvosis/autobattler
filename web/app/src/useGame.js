import { useCallback, useEffect, useState } from "react";
import { api, MoveError, NotFoundError, StaleError } from "./api.js";

const RUN_KEY = "autobattler.runId";

const MESSAGES = {
  insufficientGold: "Not enough gold.",
  teamFull: "Your team is full.",
  emptySlot: "That slot is empty.",
  emptyTeam: "You need at least one unit to fight.",
  lastUnit: "You cannot sell your last unit.",
  notAPermutation: "That is not a valid order.",
  alreadyDuplicated: "You can only duplicate once per stage.",
  runFinished: "This run is over.",
};

const UNKNOWN = { name: "?", icon: "❓", description: "", tier: 0 };

// `order[i]` names the slot each unit is taken from, so a move is a splice on the identity
// permutation: the units between the two slots shuffle along rather than swapping.
function orderMoving(length, from, to) {
  const order = Array.from({ length }, (_, slot) => slot);
  order.splice(to, 0, ...order.splice(from, 1));
  return order;
}

export function useGame() {
  const [run, setRun] = useState(null);
  const [catalog, setCatalog] = useState(new Map());
  const [screen, setScreen] = useState("start");
  const [error, setError] = useState(null);
  const [pending, setPending] = useState(null);
  const [hasSavedRun, setHasSavedRun] = useState(() => Boolean(localStorage.getItem(RUN_KEY)));

  useEffect(() => {
    api.content()
      .then((manifest) => setCatalog(new Map(manifest.units.map((unit) => [unit.kind, unit]))))
      .catch((failure) => setError(`Could not load content: ${failure.message}`));
  }, []);

  const definition = useCallback((kind) => catalog.get(kind) ?? { kind, ...UNKNOWN }, [catalog]);

  const keep = useCallback((next) => {
    setRun(next);
    localStorage.setItem(RUN_KEY, next.runId);
    setHasSavedRun(true);
  }, []);

  const abandonRun = useCallback((reason) => {
    localStorage.removeItem(RUN_KEY);
    setHasSavedRun(false);
    setRun(null);
    setPending(null);
    setError(reason ?? null);
    setScreen("start");
  }, []);

  const report = useCallback((failure) => {
    if (failure instanceof StaleError) {
      keep(failure.run);
      setError("Out of date — resynced.");
    } else if (failure instanceof MoveError) {
      setError(MESSAGES[failure.code] ?? `Rejected: ${failure.code}`);
    } else {
      setError(failure.message);
    }
  }, [keep]);

  // Every mutation replaces the run wholesale. A stale version resyncs from the run the
  // server sends back with the conflict.
  const mutate = useCallback(async (action) => {
    setError(null);
    try {
      keep(await action(run.runId, run.version));
    } catch (failure) {
      report(failure);
    }
  }, [run, keep, report]);

  // The server's buy appends; it takes no team slot. Dropping a monster onto a chosen slot is
  // therefore a buy followed by a reorder, with the board showing the intended placement from
  // the moment it lands. Only the placement is guessed at — gold is the server's to say, so the
  // client never has to know what a unit costs.
  const buyInto = useCallback(async (shopSlot, landing) => {
    const before = run;
    const offer = before.shop[shopSlot];

    setError(null);
    setRun({
      ...before,
      units: [...before.units.slice(0, landing), offer, ...before.units.slice(landing)],
      shop: before.shop.map((slotOffer, slot) => (slot === shopSlot ? null : slotOffer)),
    });

    // The buy can succeed and the reorder still fail, so the run to fall back to is the last
    // one the server actually returned — never the guess, and never the state before it.
    let settled = before;

    try {
      const boughtRun = await api.buy(before.runId, before.version, shopSlot);
      settled = boughtRun;

      const appendedSlot = boughtRun.units.length - 1;

      keep(landing < appendedSlot
        ? await api.reorder(
          boughtRun.runId,
          boughtRun.version,
          orderMoving(boughtRun.units.length, appendedSlot, landing),
        )
        : boughtRun);
    } catch (failure) {
      setRun(settled);
      report(failure);
    }
  }, [run, keep, report]);

  const start = useCallback(async () => {
    setError(null);
    try {
      keep(await api.createRun());
      setScreen("shop");
    } catch (failure) {
      report(failure);
    }
  }, [keep, report]);

  const resume = useCallback(async () => {
    setError(null);
    try {
      const resumed = await api.getRun(localStorage.getItem(RUN_KEY));
      if (resumed.finished) return abandonRun(`That run is over: ${resumed.victories} stages won.`);
      keep(resumed);
      setScreen("shop");
    } catch (failure) {
      if (failure instanceof NotFoundError) abandonRun("That run has expired.");
      else report(failure);
    }
  }, [keep, abandonRun, report]);

  // The battle response carries the run after the fight. It is held back until the replay has
  // played, so the stats on screen do not give the outcome away.
  const fight = useCallback(async () => {
    setError(null);
    try {
      const result = await api.battle(run.runId, run.version);
      setPending({
        record: result.battle,
        run: result.run,
        goldEarned: result.run.gold - run.gold,
      });
      setScreen("battle");
    } catch (failure) {
      report(failure);
    }
  }, [run, report]);

  // Reordering is applied locally before the request goes out. Without that the card settles
  // into its dropped position and then jumps again when the server's run arrives, so every
  // move reads as two. The response confirms the same order, or corrects it on a conflict.
  const reorder = useCallback((from, to) => {
    const order = orderMoving(run.units.length, from, to);
    setRun((current) => ({ ...current, units: order.map((slot) => current.units[slot]) }));
    return mutate((id, version) => api.reorder(id, version, order));
  }, [run, mutate]);

  const settle = useCallback(() => keep(pending.run), [keep, pending]);

  const advance = useCallback(() => {
    if (pending.run.finished) return abandonRun(null);
    setPending(null);
    setScreen("shop");
  }, [pending, abandonRun]);

  return {
    run, screen, error, pending, definition, hasSavedRun,
    start, resume, fight, settle, advance, abandonRun,
    buyInto, reorder,
    sell: (teamSlot) => mutate((id, version) => api.sell(id, version, teamSlot)),
    upgrade: (teamSlot) => mutate((id, version) => api.upgrade(id, version, teamSlot)),
    duplicate: (teamSlot) => mutate((id, version) => api.duplicate(id, version, teamSlot)),
    reroll: () => mutate(api.reroll),
  };
}
