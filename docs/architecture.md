# Architecture

Neon Rush is a small game built the way I'd build a large one. The point of this document is that someone can read it in five minutes and know where any piece of code belongs and what it is allowed to touch.

## Module map

```
┌───────────────────────────────────────────────────────────────┐
│ App          composition roots, game flow, player-loop bridge   │
└──────────────┬────────────────────────────────────────────────┘
               │ references
┌──────────────▼────────────────────────────────────────────────┐
│ Features     Pace · Player · Track · Scoring · Screens ·        │
│              Cameras · Effects · Sound                          │
│              (each its own assembly, none references another)   │
└──────────────┬────────────────────────────────────────────────┘
               │
┌──────────────▼────────────────────────────────────────────────┐
│ Shared       contracts between features: IGameFlow,             │
│              IRunProgress, IRunnerBody, ILaneLayout,            │
│              IInputProfile, IPointerClaims, IBestScore,         │
│              ISoundSettings, messages                           │
└──────────────┬────────────────────────────────────────────────┘
               │
┌──────────────▼────────────────────────────────────────────────┐
│ Core         com.roadandcode.core — knows nothing about this    │
│              game: message bus, state machine, simulation loop, │
│              pooling, save store, seeded random, platform       │
│              service, screen metrics, gesture recognition       │
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
| `Presentation` | Views, and presenters that are tied to one concrete view | No rules; a view only does what it is told |
| `Composition` | The feature's installer | The only place the feature touches the DI container |

| Feature | Owns | Offers to others |
| --- | --- | --- |
| Pace | The run clock: elapsed time, distance, speed, difficulty tier | `IRunProgress` |
| Player | Movement rules, input, and what a hazard hit costs | `IRunnerBody` |
| Track | What is on the track, spawning, recycling, contact checks | `HazardHit`, `PickupCollected` |
| Scoring | Score, combo multiplier, best score | `ScoreChanged`, `RunScored`, `IBestScore` |
| Screens | Menu, HUD, pause and game-over screens, and which of them is up | `IPointerClaims`, `ButtonPressed` |
| Cameras | The shots, the move between them, and the shake on impact | nothing: it only listens |
| Effects | Particle bursts for hits and pickups | nothing: it only listens |
| Sound | Sound effects, the two music loops, the sound setting | `ISoundSettings` |

## Composition

There are two composition roots, one per scope:

- **`AppLifetimeScope`** (Bootstrap scene) holds what lives for the whole session: the platform, the message bus, the save store and the game flow.
- **`GameplayLifetimeScope`** (Gameplay scene, loaded additively as a child scope) holds the platform's input and the features. It calls each feature's installer in a fixed order and nothing else.

These two classes and the installers are the only code that names concrete types or touches the container ([VContainer](https://github.com/hadashiA/VContainer)). Everything else receives what it needs through its constructor, which is why logic classes can be built by hand in tests. There are no singletons and no static state.

Adding a feature is: a new assembly, a new installer, one line in `GameplayLifetimeScope`. Sound, Effects and Cameras were added that way after the game was already playable, and no existing feature was edited to know about them: they listen to messages the game was already sending, plus two new ones (`RunnerMoved`, `ButtonPressed`) that say what happened without saying what it is for. Removing the Sound installer gives a silent game and nothing else notices.

## Game flow

`GameFlow` owns the current phase through a state machine with a declared transition table. A transition that isn't in the table can't happen, whoever asks for it.

```
Boot ──► Menu ──► Intro ──► Run ◄──► Paused
          ▲                  │          │
          │                  ▼          │
          ├────────────── GameOver ──► Run   (a retry skips the intro)
          │                             │
          └─────────────────────────────┘   (abandon run)
```

Features don't change phase themselves. They call `IGameFlow` (`StartRun`, `Pause`, `Resume`, `FailRun`, `ReturnToMenu`), and each call returns `false` if it isn't valid right now.

**The intro belongs to the flow.** Pressing play on the menu resets the run (`RunStarted`), enters `Intro` and announces `RunIntroStarted(duration)`. The flow's own clock ends the intro; the camera move and the start sound take their length from that message. No feature can stall the start of a run by failing to report that it has finished, and there is one number for how long the opening takes. A retry from the game-over screen goes straight to `Run`.

**The menu gets a clean stage.** Going back to the menu announces `StageCleared` after the run's result has been recorded. Pace, Player and Track put themselves back to their resting state, so the title screen never shows the wreckage of the last run.

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

Some things have to keep moving while the simulation is stopped: the camera on the menu and through the intro, a music fade during a pause, the wait before the game-over screen. Those are container entry points on the player loop with their own delta time, and none of them touches gameplay state.

```
Unity player loop, every frame, in any phase
  ├─ IntroClock          counts the intro down, then lets the run begin
  ├─ ScreenSwitcher      counts down screens that are being held back
  ├─ MusicDirector       fades the two loops towards the phase's levels
  ├─ SafeAreaPresenter   notices the notch changing side
  └─ CameraDriver        (late) advances the director, applies its pose
```

## Talking across modules

- **"Something happened"** is a message on the bus. Any number of listeners, and the sender doesn't know who they are. Messages are structs, so publishing doesn't allocate.
- **"Do this"** is a call on an interface with exactly one owner.

| Message | Sent by | Used by |
| --- | --- | --- |
| `GamePhaseChanged` | GameFlow | SimulationDriver, Player (enables input only during a run), Screens (which screens are up), Cameras, Sound (music levels) |
| `RunStarted(seed)` | GameFlow, before the phase changes | Pace, Player, Track, Scoring: all reset here |
| `RunIntroStarted(duration)` | GameFlow | Cameras (the move to the run shot), Sound (the swoosh), the flow's own intro clock |
| `StageCleared` | GameFlow, on returning to the menu | Pace, Player, Track: back to their resting state |
| `RunEnded(reason)` | GameFlow | Scoring (records the best score) |
| `HazardHit` | Track | Player (knocked down, then asks the flow to fail the run), Cameras (shake), Effects (burst), Screens (flash), Sound |
| `PickupCollected(value)` | Track | Scoring, Effects, Sound |
| `RunnerMoved(move)` | Player, only for moves that took effect | Sound |
| `ButtonPressed` | Screens | Sound |
| `SoundSettingChanged` | Sound | Screens (the toggle shows the setting) |
| `ScoreChanged` | Scoring | Screens (HUD), Sound (pickup pitch follows the combo) |
| `RunScored` | Scoring | Screens (game-over result, best score on the menu), Sound (new-best cue) |

The track reports that a hazard was touched. It does not decide what that means: the player feature does. A shield or an extra life is a change in one class in one feature.

## Platforms

The game targets Windows, Android and the browser from one codebase, and no gameplay or UI class knows which of them it is on.

- **The platform is a service.** `IPlatform` reports a family: Desktop, Mobile or Web. `RuntimePlatformService` is the only class that reads `Application.platform`. In the editor, `AppLifetimeScope` can register a `FixedPlatform` instead, which is how the phone set-up gets checked without making a build.
- **Differences are data.** `PerPlatform<T>` is a serialized value with one slot per family. Frame-rate target, v-sync and screen sleep come from a `PlatformSettingsAsset`; the active control schemes come from an `InputProfileAsset` per family. Changing how the game behaves on Android is an edit to the Android slot.
- **The screen is behind interfaces.** `IScreenMetrics` gives the size gesture thresholds are measured against, and `ISafeArea` gives the notch insets as shares of the screen. Both are faked in tests.

There is no `#if UNITY_ANDROID` and no platform `switch` outside `RuntimePlatformService`.

## Input

Input is four layers, and each one only knows the layer below it.

```
4. Gameplay          PlayerController drains PlayerAction values. It has never heard of a key.
3. Action sources    ButtonActionSource (keyboard, gamepad) · SwipeActionSource (touch, mouse, pen)
                     joined by CompositeActionSource, all behind IPlayerActionSource
2. Platform profile  IInputProfile: the control schemes this platform uses.
                     PlatformInput masks every binding outside them.
1. Devices           NeonRush.inputactions: Run, Flow and Pointer maps; Keyboard, Gamepad and Pointer schemes
```

| Platform | Schemes | Sources created |
| --- | --- | --- |
| Desktop | Keyboard, Gamepad | Buttons |
| Mobile | Pointer, Gamepad | Buttons (for a paired gamepad), Swipes |
| Web | Keyboard, Gamepad, Pointer | Buttons, Swipes |

- **Nothing outside layer 3 touches the Input System**, apart from `FlowInput`, which maps the Confirm and Pause actions onto `IGameFlow`. There is no `Keyboard.current` and no key polling anywhere.
- **`PlayerInputFactory` builds sources from the profile.** A platform without the Pointer scheme never creates a swipe source, so there is nothing to switch off at runtime.
- **Recognising a swipe is plain C#.** `SwipeRecognizer` (in Core) takes positions and a threshold and returns a direction. The threshold is a share of the screen's short side, so a swipe is the same physical gesture on a phone and a tablet. `SwipeActionSource` only connects the pointer to it.
- **Gestures ask the UI first.** A press that lands on an on-screen control belongs to that control. `SwipeActionSource` asks `IPointerClaims` before starting a gesture, and the Screens feature answers from the UI panel. Pressing the pause button can't also change lane.
- **A gesture belongs to the device that started it.** A touch laptop has a finger and a mouse at once. The pointer position action is pass-through, so it reports every pointer, and the swipe source follows only the one whose press it is tracking.
- **Sources are tested with virtual devices** against the real action asset: a key or a finger goes in at layer 1 and a `PlayerAction` has to come out at layer 4.

## Screens

The UI is UI Toolkit, split model–view–presenter. Presenters are plain C# in the feature's `Logic` folder and only know view interfaces; the UI Toolkit code is in `Presentation`.

```
Screens.uxml  (one UIDocument, one panel)
  ├─ hud         HudView         ◄── HudPresenter         ScoreChanged → digits · pause button → IGameFlow.Pause
  ├─ menu        MenuView        ◄── MenuPresenter        IBestScore, RunScored · play → IGameFlow.StartRun
  ├─ pause       PauseView       ◄── PausePresenter       resume / end run → IGameFlow
  └─ game-over   GameOverView    ◄── GameOverPresenter    RunScored · run again / menu → IGameFlow

ScreenSwitcher        shows and hides screens from the screen table on GamePhaseChanged
ControlHintsPresenter tags the document with the schemes in IInputProfile; the style sheet reveals matching hints
SafeAreaPresenter     pads content by ISafeArea, and again when the notch changes side
```

- **Screens don't show themselves.** `ScreenTable` is the one list of which screens are up in which phase, and `ScreenSwitcher` applies it. A presenter only fills its screen in and turns button presses into `IGameFlow` calls. No UI class holds a game rule.
- **One MonoBehaviour.** `ScreensDocument` owns the views and binds them to the visual tree when Unity builds it. Views are plain classes, so they are bound to the authored UXML in an EditMode test.
- **The score display builds no strings.** `DigitStrip` shows a number as one label per digit, each holding one of ten constant strings. The score changes most frames; this way it does so without garbage, and fixed-width cells stop the number shifting sideways as it counts.
- **Hints follow the input profile, not the platform.** Every hint is written in the UXML once per control scheme and hidden. The presenter tags the document root with the schemes the profile uses and USS does the rest, so a phone says "swipe" and a desktop names keys without a platform check in UI code.
- **Built for a range of screens.** The panel scales from a 1920 × 1080 reference and only ever grows the canvas, so the layout always has at least that much room. Touch targets are 120 units or taller. Content sits inside the safe area while backdrops run to the edge of the glass.
- **Buttons can't take keyboard focus.** A focused UI Toolkit button answers Space and Enter, which are also jump and start. Keyboard and gamepad reach the flow through `FlowInput` instead.
- **The result waits for the crash.** A rule in the screen table can hold its screen back. The game-over screen arrives 0.85 seconds after the phase does, so the hit, the sparks and the runner going down are seen before a panel covers them. A retry during that wait means it never appears.

## Camera

The title screen is a shot, not a backdrop: the camera sits ahead of the runner looking back at it, with the runner on the right of the frame and the menu on the left. Pressing play swings the camera round behind the runner, and the run starts as it arrives.

- **Shots are orbits, not positions.** A `CameraShot` is an angle around the runner, a distance, a height, a look-at point, a field of view and a framing. Blending two shots blends those numbers, so the camera travels round the runner. Blending two positions would have dragged it in a straight line over the runner's head.
- **Framing is a share of the screen.** "The runner sits 22% right of centre" is stored as that, and `ShotSolver` works out the yaw and pitch that achieve it for the current aspect ratio, with the horizon kept level. The same shot holds in a wide window and a nearly square one, which a browser game has to cope with.
- **`CameraDirector` has no camera.** It reacts to `GamePhaseChanged`, `RunIntroStarted` and `HazardHit`, and produces a pose when asked. The whole opening move is stepped through in EditMode tests. `CameraDriver` is the few lines that tick it and hand the pose to the rig.
- **A move can be interrupted.** A new move starts from wherever the camera is, so pressing play while it is still swinging back to the menu doesn't jump.
- **The shake is deterministic.** Two sine waves at unrelated rates, fading with the square of the time left. It moves the camera and leaves where it points alone.

## Sound and effects

- **`SoundDirector` maps what happens to cues**; `SoundOutput` is the only class that touches Unity audio. One-shot sounds take turns on eight audio sources, so playing a sound creates nothing.
- **A pickup streak climbs in pitch**, two semitones per step of the combo, so the multiplier can be heard without looking at it.
- **Two music loops play in step the whole time.** The title loop is pads and an arpeggio; the run loop is the same with bass and drums. Starting a run fades one into the other, so the drums come in over music that was already playing. They are scheduled on the audio clock to start on the same sample.
- **The sound setting has one owner.** `ISoundSettings` lives in the Sound feature and is saved through `ISaveStore`. The toggle on the menu and the pause panel shows it and asks for it to change; it follows the setting, not its own presses.
- **Effects are two particle systems that only ever emit.** They simulate in world space, so moving a system to the next burst leaves the last one where it was. Nothing is instantiated during play.
- **The audio is placeholder.** Every clip is synthesised: short envelopes on sine, triangle, saw and noise, and a sixteen-second loop in A minor. They are ordinary `AudioClip` assets in one `SoundBank`, so replacing any of them is dragging in a different clip.

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
| Strategy | `IPatternPicker`, `IDifficultyCurve`, `IRunSeedSource`, `ITrackEntityDefinition.OnTouched`, `IPlatform` with `PerPlatform<T>` | Behaviour chosen by binding or by asset instead of by `switch` |
| Composite | `CompositeActionSource` | Gameplay reads one input source whether the platform has one kind of input or three |
| Factory | `PlayerInputFactory` | Which sources exist is decided once, from the platform's profile |
| Adapter | `UnityScreenMetrics`, `RuntimePlatformService`, `PanelPointerClaims`, `PlayerPrefsSaveStore` | Engine and platform APIs behind interfaces the game owns, so they can be faked |
| Object pool | `ComponentPool<T>`, `TrackField` | Nothing spawned during a run costs an allocation |
| Repository | `ISaveStore` | Callers store records; where they go is a binding |
| Dependency injection | Two lifetime scopes plus feature installers | Testable logic, swappable implementations, one place that knows concrete types |
| Model–view–presenter | Every screen (presenter, view interface, UI Toolkit view); `PlayerMotor` / `PlayerView` / `PlayerPresenter`, and the same for the track, the camera, effects and sound | Views stay dumb, rules stay testable |

## Decisions

**Custom lane-space contacts instead of PhysX.** The game needs box overlap on a handful of objects in three lanes. Doing it directly is deterministic, unit-testable and frame-rate independent in a way trigger callbacks are not.

**Installers live in the features.** The alternative is one composition root that registers every class of every feature, which forces those classes to be public. With installers, a feature's internals stay internal and the root only decides which features exist and in what order.

**VContainer over a hand-written composition root.** Scoped lifetimes and player-loop entry points come for free, and it is small and fast enough for WebGL. The cost is one dependency, kept at the edges.

**Declared transitions in the state machine.** More setup than "go to any state", but a wrong transition fails where it is requested instead of showing up later as a UI in the wrong mode.

**Delta time is clamped at the bridge.** Browsers stop calling the game loop for background tabs. Without the clamp, the first frame back would simulate several seconds in one step.

**Start-up can't fail quietly.** The container drops exceptions from async entry points unless it is given a handler, and a boot that throws then looks like a blank screen with a clean console. Both scopes register one that logs.

**Platform families, not platforms.** `IPlatform` answers Desktop, Mobile or Web, because those are the lines along which this game differs: what input exists and who owns the frame rate. A new target that behaves like an existing family needs one line in the mapping.

**Control schemes are masked, not ignored.** A platform's profile switches whole device families off in the action asset. The alternative, leaving every binding live and filtering later, means a phone with a stray keyboard event has to be reasoned about in gameplay code.

**Pointer position is a pass-through action.** As a value action the Input System picks one "winning" control when several devices are bound, and for a position that means the pointer furthest from the screen's origin. On a machine with a mouse and a touchscreen, a finger's movement was dropped whenever the mouse happened to rest further out. I found this playing the web build, and there is now a test with both devices attached.

**The flow times the intro, not the camera.** The alternative was for the camera to tell the flow when its move had finished. That makes the start of a run depend on a presentation feature being installed and behaving, and puts the length of the intro in two places.

**Sound and effects only listen.** Neither is called by gameplay code. The cost was two new messages; the benefit is that the features that make noise and sparks can be removed, replaced or rewritten without touching a rule.

**The runner is drawn the size it collides at, and a test says so.** For a while it was not: the built-in mesh I had used was twice the size of the primitive capsule, so the runner looked 1.8 m wide with a 0.9 m hitbox and its visor was buried inside it. I only saw it when the title camera looked at the runner from the front. The PlayMode smoke test now compares the drawn bounds with the hitbox.

**One UI document.** All four screens share a panel, so the UI is drawn in one pass and there is one place that scales it. The cost is that screens can't be loaded separately, which a game with four screens doesn't need.

**The UI is not part of the simulation.** Presenters react to messages and call the flow. They are not ticked with the gameplay systems, so pausing the simulation can't freeze a button.

**Core is an embedded package.** It has no reference to this game. When the next project needs it, it moves to its own repo and both consume it by git URL at a version tag.

## Tests

- **Core** — message bus, state machine, simulation loop, seeded random, save stores, platform mapping, safe-area insets, swipe recognition.
- **Pace, Player, Track, Scoring** — every rule in each feature's `Logic` folder, built by hand with fakes for the contracts it depends on.
- **Player input** — virtual keyboard, gamepad, touchscreen and mouse driven through the real action asset, including which sources each kind of profile gets, that a drag starting on an on-screen control is not a swipe, and that a mouse and a touchscreen attached together don't interfere.
- **Cameras** — the shot solver (the target lands where the framing says at five aspect ratios, with a level horizon) and the director (cut on boot, move over exactly the intro, interruption, shake).
- **Sound, Effects** — the cue for each message, the pickup pitch ladder, music levels and fades per phase, the saved setting; bursts at the runner's position.
- **Screens** — every presenter against fake views, and the switcher against the real screen table. The authored UXML is loaded and checked against the views: every element they look up exists, every screen starts hidden, no button can take keyboard focus, only buttons and backdrops take pointer input, and every control scheme has hints.
- **App (EditMode)** — every legal and illegal flow transition; binding masks per platform profile; and the authored assets checked against each other, so a change to jump height that makes a pattern impossible fails a test.
- **App (PlayMode)** — boots the real scenes with the real containers and plays a run; checks the runner is drawn the size of its hitbox; and checks the real UI document and camera: the right screens in each phase, the title shot in front of the runner and the run shot behind it, the result held back after a crash, and the pointer claimed only where controls are. A missing registration or an unassigned scene reference fails here.
- **Allocation tests** — the message bus, the state machine, a warmed-up player, track and scoring loop, and the HUD's score display are each run under `Is.Not.AllocatingGCMemory()`. "No garbage per frame" is a test result, not a claim. The first run of these tests caught a formatted error message being built on every successful state change.

```bash
Unity -batchmode -projectPath . -runTests -testPlatform EditMode -testResults Logs/editmode.xml
Unity -batchmode -projectPath . -runTests -testPlatform PlayMode -testResults Logs/playmode.xml
```
