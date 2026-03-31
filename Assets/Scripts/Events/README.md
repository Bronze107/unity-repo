# Unity Event System

## Layout

- `IGameEvent`: marker interface for all events
- `EventBus`: generic event dispatch and subscription
- `EventBusExtensions`: Unity-friendly overloads for owner-first subscriptions
- `EventBinding<T>`: optional disposable binding handle
- `EventScope`: MonoBehaviour helper for lifecycle unbinding
- `GameEvents`: sample event definitions
- `Examples`: minimal global and local bus usage

## Namespaces

- Runtime code: `Game.Events`
- Example MonoBehaviours: `Game.Events.Examples`

## Guidelines

1. Add new events as `struct` types only.
2. Prefer owner-first subscriptions such as `EventBus.Global.Subscribe(this, OnDamage)`.
3. Use `EventBus.Global` only for cross-system communication.
4. Prefer a local `EventBus` for module-internal or battle-instance events.
5. Do not route extremely high-frequency per-frame signals through the global bus.
