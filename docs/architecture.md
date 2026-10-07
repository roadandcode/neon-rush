# Architecture

Neon Rush is a small game built the way I'd build a large one. The point of this document is that someone can read it in five minutes and know where any piece of code belongs and what it is allowed to touch.

## Module map

```
┌───────────────────────────────────────────────────────────────┐
│ App          composition roots, game flow, player-loop bridge   │
└──────────────┬────────────────────────────────────────────────┘
               │ references
┌──────────────▼────────────────────────────────────────────────┐
│ Features     Pace · Player · Track · Scoring                    │
│              (each its own assembly, none references another)   │
└──────────────┬────────────────────────────────────────────────┘
               │
┌──────────────▼────────────────────────────────────────────────┐
│ Shared       contracts between features: IGameFlow,             │
│              IRunProgress, IRunnerBody, ILaneLayout, messages   │
└──────────────┬────────────────────────────────────────────────┘
               │
┌──────────────▼────────────────────────────────────────────────┐
│ Core         com.roadandcode.core — knows nothing about this    │
│              game: message bus, state machine, simulation loop, │
│              pooling, save store, seeded random                 │
└───────────────────────────────────────────────────────────────┘
```

| Assembly | Lives in | May reference |
| --- | --- | --- |
| `RoadAndCode.Core` | `Packages/com.roadandcode.core/Runtime` | Unity only |
| `RoadAndCode.NeonRush.Shared` | `Assets/_Project/Shared` | Core |
| `RoadAndCode.NeonRush.<Feature>` | `Assets/_Project/Features/<Feature>/Runtime` | Core, Shared |
| `RoadAndCode.NeonRush.App` | `Assets/_Project/App/Runtime` | everything above |

Every assembly has `autoReferenced` off, so a dependency only exists if its `.asmdef` says so. A feature reaching into another feature is a compile error, not a code-review comment. Nothing is compiled into `Assembly-CSharp`.

## Inside a feature

Every feature has the same four folders, and almost everything in them is `internal`. What a feature offers the rest of the game is the contracts in Shared and one public installer.

| Folder | Holds | Rule |
| --- | --- | --- |
| `Data` | ScriptableObject assets and the plain settings classes they wrap | Read-only at runtime |
| `Logic` | The rules, as plain C# classes | No `MonoBehaviour`, so all of it is unit-tested |
| `Presentation` | Views (`MonoBehaviour`) and the presenters that drive them | No rules; a view only does what it is told |
| `Composition` | The feature's installer | The only place the feature touches the DI container |

| Feature | Owns | Offers to others |
| --- | --- | --- |
| Pace | The run clock: elapsed time, distance, speed, difficulty tier | `IRunProgress` |
| Player | Movement rules, input, and what a hazard hit costs | `IRunnerBody` |
| Track | What is on the track, spawning, recycling, contact checks | `HazardHit`, `PickupCollected` |
| Scoring | Score, combo multiplier, best score | `ScoreChanged`, `RunScored` |

## Composition

There are two composition roots, one per scope:

- **`AppLifetimeScope`** (Bootstrap scene) holds what lives for the whole session: the message bus, the save store and the game flow.
- **`GameplayLifetimeScope`** (Gameplay scene, loaded additively as a child scope) holds the features. It calls each feature's installer in a fixed order and nothing else.

These two classes and the installers are the only code that names concrete types or touches the container ([VContainer](https://github.com/hadashiA/VContainer)). Everything else receives what it needs through its constructor, which is why logic classes can be built by hand in tests. There are no singletons and no static state.

Adding a feature is: a new assembly, a new installer, one line in `GameplayLifetimeScope`.

## Game flow

`GameFlow` owns the current phase through a state machine with a declared transition table. A transition that isn't in the table can't happen, whoever asks for it.

```
Boot ──► Menu ──► Run ◄──► Paused
          ▲        │          │
          │        ▼          │
          ├──── GameOver ──► Run
          │                   │
          └───────────────────┘   (abandon run)
```

Features don't change phase themselves. They call `IGameFlow` (`StartRun`, `Pause`, `Resume`, `FailRun`, `ReturnToMenu`), and each call returns `false` if it isn't valid right now.

## How a frame runs

```
Unity player loop
  └─ SimulationDriver.Tick()           clamps delta time; no-op unless the phase is Run
       └─ SimulationLoop.Tick(dt)      systems in the order their installers registered them:
            1. RunProgress             advance the clock: speed, distance, tier
            2. PlayerController        apply queued input, move the runner
            3. PlayerPresenter         pose the runner view
            4. TrackSimulation         move the track, recycle, spawn, check contacts
            5. TrackPresenter          place pooled views, scroll the ground
            6. ScoreKeeper             distance + pickups, announce changes
```

Gameplay objects have no `Update` methods. Everything that advances with game time is an `ISimulationSystem`, ticked from one place in an order that is written down. Pausing is the driver not ticking; no system knows what a pause is.

## Talking across modules

- **"Something happened"** is a message on the bus. Any number of listeners, and the sender doesn't know who they are. Messages are structs, so publishing doesn't allocate.
- **"Do this"** is a call on an interface with exactly one owner.

| Message | Sent by | Used by |
| --- | --- | --- |
| `GamePhaseChanged` | GameFlow | SimulationDriver, Player (enables input only during a run) |
| `RunStarted(seed)` | GameFlow, before the phase changes | Pace, Player, Track, Scoring: all reset here |
| `RunEnded(reason)` | GameFlow | Scoring (records the best score) |
| `HazardHit` | Track | Player (knocked down, then asks the flow to fail the run) |
| `PickupCollected(value)` | Track | Scoring |
| `ScoreChanged`, `RunScored` | Scoring | UI |

The track reports that a hazard was touched. It does not decide what that means: the player feature does. A shield or an extra life is a change in one class in one feature.

## The track

- **The runner never moves forward.** The track comes to it. Coordinates stay small however long a run lasts, and "distance" is one number owned by Pace.
- **Lanes, not physics.** Everything on the track is an axis-aligned box in lane space, and so is the runner. A hurdle is a low box, a gate is a box that starts above head height while sliding, a barrier is a tall one. "Jump over" and "slide under" fall out of the same overlap test, with no special cases and no physics engine.
- **Content is data.** A hazard or pickup is a `TrackEntityDefinition` asset; a pattern is a `TrackPatternAsset` of rows and lanes. A new obstacle or a new pattern is a new asset. A new *kind* of entity is a new definition class; the simulation doesn't change.
- **Spacing is in seconds.** Rows are spaced by travel time, so the player's reaction window stays constant as the run speeds up.
- **Seeded.** The track has its own random stream, reseeded from `RunStarted`. The same seed lays out the same track, which is what a daily run or a replay needs.
- **Pooled.** Entity logic objects and their views are both recycled. After warm-up a run allocates nothing per frame.

## Patterns in use

| Pattern | Where | Why |
| --- | --- | --- |
| State | `StateMachine<TKey>`; `GameFlow`, `PlayerMotor` | Mode-dependent behaviour without flag checks; illegal transitions rejected in one place |
| Observer | `MessageBus`; `TrackField.Added/Removed` | Reactions without the sender knowing the listeners; logic that doesn't know views exist |
| Command | `PlayerAction` values through `IPlayerActionSource` | Input as data: it can be queued between ticks, buffered for feel, and later recorded or replayed |
| Strategy | `IPatternPicker`, `IDifficultyCurve`, `IRunSeedSource`, `ITrackEntityDefinition.OnTouched` | Behaviour chosen by binding or by asset instead of by `switch` |
| Object pool | `ComponentPool<T>`, `TrackField` | Nothing spawned during a run costs an allocation |
| Repository | `ISaveStore` | Callers store records; where they go is a binding |
| Dependency injection | Two lifetime scopes plus feature installers | Testable logic, swappable implementations, one place that knows concrete types |
| Model–view–presenter | `PlayerMotor` / `PlayerView` / `PlayerPresenter`, and the same for the track | Views stay dumb, rules stay testable |

## Decisions

**Custom lane-space contacts instead of PhysX.** The game needs box overlap on a handful of objects in three lanes. Doing it directly is deterministic, unit-testable and frame-rate independent in a way trigger callbacks are not.

**Installers live in the features.** The alternative is one composition root that registers every class of every feature, which forces those classes to be public. With installers, a feature's internals stay internal and the root only decides which features exist and in what order.

**VContainer over a hand-written composition root.** Scoped lifetimes and player-loop entry points come for free, and it is small and fast enough for WebGL. The cost is one dependency, kept at the edges.

**Declared transitions in the state machine.** More setup than "go to any state", but a wrong transition fails where it is requested instead of showing up later as a UI in the wrong mode.

**Delta time is clamped at the bridge.** Browsers stop calling the game loop for background tabs. Without the clamp, the first frame back would simulate several seconds in one step.

**Start-up can't fail quietly.** The container drops exceptions from async entry points unless it is given a handler, and a boot that throws then looks like a blank screen with a clean console. Both scopes register one that logs.

**Core is an embedded package.** It has no reference to this game. When the next project needs it, it moves to its own repo and both consume it by git URL at a version tag.

## Tests

- **Core** — message bus, state machine, simulation loop, seeded random, save stores.
- **Pace, Player, Track, Scoring** — every rule in each feature's `Logic` folder, built by hand with fakes for the contracts it depends on.
- **App (EditMode)** — every legal and illegal flow transition; and the authored assets checked against each other, so a change to jump height that makes a pattern impossible fails a test.
- **App (PlayMode)** — boots the real scenes with the real containers and plays a run. A missing registration or an unassigned scene reference fails here.
- **Allocation tests** — the message bus, the state machine, and a warmed-up player, track and scoring loop are each run under `Is.Not.AllocatingGCMemory()`. "No garbage per frame" is a test result, not a claim. The first run of these tests caught a formatted error message being built on every successful state change.

```bash
Unity -batchmode -projectPath . -runTests -testPlatform EditMode -testResults Logs/editmode.xml
Unity -batchmode -projectPath . -runTests -testPlatform PlayMode -testResults Logs/playmode.xml
```
