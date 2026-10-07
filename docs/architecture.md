# Architecture

Neon Rush is a small game built the way I'd build a large one. The point of this document is that someone can read it in five minutes and know where any piece of code belongs and what it is allowed to touch.

## Module map

```
┌──────────────────────────────────────────────────────────────┐
│ App            composition root, game flow, player-loop bridge │
└───────────────┬──────────────────────────────────────────────┘
                │ references
┌───────────────▼──────────────────────────────────────────────┐
│ Features       Player · Track · Scoring · Difficulty · UI …    │
│                (each its own assembly, none references another) │
└───────────────┬──────────────────────────────────────────────┘
                │
┌───────────────▼──────────────────────────────────────────────┐
│ Shared         contracts between features: IGameFlow, messages │
└───────────────┬──────────────────────────────────────────────┘
                │
┌───────────────▼──────────────────────────────────────────────┐
│ Core           com.roadandcode.core — knows nothing about this │
│                game: message bus, state machine, simulation    │
│                loop, pooling, save store, seeded random        │
└──────────────────────────────────────────────────────────────┘
```

| Assembly | Lives in | May reference |
| --- | --- | --- |
| `RoadAndCode.Core` | `Packages/com.roadandcode.core/Runtime` | Unity only |
| `RoadAndCode.NeonRush.Shared` | `Assets/_Project/Shared` | Core |
| `RoadAndCode.NeonRush.<Feature>` | `Assets/_Project/Features/<Feature>/Runtime` | Core, Shared |
| `RoadAndCode.NeonRush.App` | `Assets/_Project/App/Runtime` | everything above, VContainer |

Every assembly has `autoReferenced` off, so a dependency only exists if its `.asmdef` says so. A feature reaching into another feature is a compile error, not a code-review comment. Nothing is compiled into `Assembly-CSharp`.

## Composition

`AppLifetimeScope` is the composition root. It is the only class that decides which concrete type sits behind each interface; everything else receives what it needs through its constructor. There are no singletons and no static state.

The container is [VContainer](https://github.com/hadashiA/VContainer). It is referenced from the composition root and from feature installers only. Logic classes never see it, which is why they can be constructed by hand in tests.

## Game flow

`GameFlow` owns the current phase through a state machine with a declared transition table. A transition that isn't in the table can't happen, whoever asks for it.

```
Boot ──► Menu ──► Run ◄──► Paused
          ▲        │          │
          │        ▼          │
          └──── GameOver      │
          ▲        │          │
          │        └──► Run   │
          └───────────────────┘   (abandon run)
```

Features don't change phase themselves. They call `IGameFlow` (`StartRun`, `Pause`, `Resume`, `FailRun`, `ReturnToMenu`), and each call returns `false` if it isn't valid right now.

## How a frame runs

```
Unity player loop
  └─ AppEntryPoint.Tick()               clamps delta time (a background tab can report seconds)
       └─ SimulationLoop.Tick(dt)       no-op unless the phase is Run
            └─ ISimulationSystem.Tick(dt), in registration order
```

Gameplay systems are plain classes implementing `ISimulationSystem`. They are ticked from one place, in an order that is written down in the composition root, instead of each object having its own `Update`. Pausing is switching the loop off. Nothing else has to know the game is paused.

## Talking across modules

- **"Something happened"** is a message on the bus: `GamePhaseChanged`, `RunStarted`, `RunEnded`. Any number of listeners, and the sender doesn't know who they are. Messages are structs, so publishing doesn't allocate.
- **"Do this"** is a call on an interface with exactly one owner: `IGameFlow`.

`RunStarted` is published before the simulation starts ticking, and carries the run's seed. Systems reset there, and anything random in a run derives from that seed, so a run can be replayed.

## Patterns in use

| Pattern | Where | Why |
| --- | --- | --- |
| State | `StateMachine<TKey>`, `GameFlow` | Phase-dependent behaviour without flag checks; illegal transitions rejected in one place |
| Observer | `MessageBus` | UI, audio and scoring react to gameplay without gameplay knowing they exist |
| Dependency injection | `AppLifetimeScope` | Testable logic, swappable implementations |
| Strategy | `IRunSeedSource` | A daily-seed or shared-seed mode is a different binding, not a code change |
| Object pool | `ComponentPool<T>` | Nothing spawned during a run should cost an `Instantiate` |
| Repository | `ISaveStore` | Callers store records; where they go (PlayerPrefs, memory, later a backend) is a binding |

## Decisions

**VContainer over a hand-written composition root.** Scoped lifetimes and player-loop entry points come for free, and it is small and fast enough for WebGL. The cost is one dependency, kept at the edges.

**Declared transitions in the state machine.** More setup than "go to any state", but a wrong transition fails where it is requested instead of showing up later as a UI in the wrong mode.

**Delta time is clamped at the bridge.** Browsers stop calling the game loop for background tabs. Without the clamp, the first frame back would simulate several seconds in one step.

**Core is an embedded package.** It has no reference to this game. When the next project needs it, it moves to its own repo and both consume it by git URL at a version tag.

## Tests

- `RoadAndCode.Core.Tests` — message bus, state machine, simulation loop, seeded random, save store.
- `RoadAndCode.NeonRush.App.Tests` — every legal and illegal flow transition, and the order messages go out in.
- `RoadAndCode.NeonRush.App.PlayTests` — loads the real bootstrap scene with the real container and walks the flow. A missing registration fails here.

Run them from a terminal:

```bash
Unity -batchmode -projectPath . -runTests -testPlatform EditMode -testResults Logs/editmode.xml
Unity -batchmode -projectPath . -runTests -testPlatform PlayMode -testResults Logs/playmode.xml
```
