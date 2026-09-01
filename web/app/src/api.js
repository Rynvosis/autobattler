// The only place that knows about HTTP. Everything above it sees a run or an error.

export class StaleError extends Error {
  constructor(run) { super("stale version"); this.run = run; }
}

export class MoveError extends Error {
  constructor(code) { super(code); this.code = code; }
}

export class NotFoundError extends Error {}

async function call(method, path, body) {
  const response = await fetch(path, {
    method,
    headers: body ? { "Content-Type": "application/json" } : {},
    body: body ? JSON.stringify(body) : undefined,
  });

  const text = await response.text();
  const data = text ? JSON.parse(text) : null;

  if (response.status === 404) throw new NotFoundError(path);
  if (response.status === 409) throw new StaleError(data);
  if (response.status === 400) throw new MoveError(data?.error ?? "badRequest");
  if (!response.ok) throw new Error(`${method} ${path} → ${response.status}`);

  return data;
}

const mutate = (id, action, version, fields = {}) =>
  call("POST", `/runs/${id}/${action}`, { version, ...fields });

export const api = {
  content: () => call("GET", "/content"),
  createRun: () => call("POST", "/runs"),
  getRun: (id) => call("GET", `/runs/${id}`),
  battle: (id, version) => mutate(id, "battle", version),
  reroll: (id, version) => mutate(id, "shop/roll", version),
  buy: (id, version, shopSlot) => mutate(id, "shop/buy", version, { shopSlot }),
  upgrade: (id, version, teamSlot) => mutate(id, "shop/upgrade", version, { teamSlot }),
  duplicate: (id, version, teamSlot) => mutate(id, "shop/duplicate", version, { teamSlot }),
  reorder: (id, version, order) => mutate(id, "team/reorder", version, { order }),
  sell: (id, version, teamSlot) => mutate(id, "team/sell", version, { teamSlot }),
};
