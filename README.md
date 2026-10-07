# Neon Rush

A fast arcade runner for the browser, Windows and Android, built small but structured like a production game.

**Status:** in development, and built for the browser while it is. The full loop is playable: a title screen with the runner facing the camera, a camera move round behind it into the run, pause, a crash with sparks and a shake, game over and retry, with keyboard, gamepad, mouse or touch. Three lanes, jump, slide, three kinds of hazard, pickups with a combo multiplier, a difficulty curve, sound effects, music and a saved best score. The game also builds for Windows and Android from the same code; those builds will be made and tested again once the game itself is settled. The audio is synthesised placeholder. A size and performance pass comes later.

## Overview

Neon Rush is my take on the endless runner. It has to download quickly, start quickly and hold its frame rate on an ordinary laptop or phone. I'm using it to show how I structure a Unity project when I expect it to grow: modules with enforced boundaries, logic that runs without the engine, content that is data, one codebase for every platform, and one place where everything is wired together.

## Controls

| Action | Keyboard | Gamepad | Touch or mouse |
| --- | --- | --- | --- |
| Start / run again | Enter or Space | A / Cross | Play button |
| Change lane | A / D or Left / Right | D-pad or left stick | Swipe sideways |
| Jump | W, Up or Space | A / Cross | Swipe up |
| Slide (in the air: dive) | S or Down | B / Circle | Swipe down |
| Pause | Esc or P | Start | Pause button |
| Sound on / off | | | Toggle on the title screen and the pause panel |

Which of these are live depends on the platform: Windows listens to the keyboard and gamepad, Android to touch and a paired gamepad, the browser to all three. The hints on the title screen show only what applies.

## Features

- **Lane runner** with a jump arc, a slide, a mid-air dive, and a short input buffer so a press that is a few frames early still counts.
- **Three hazards from one rule.** A hurdle is a low box, a gate is a box that starts above a sliding runner, a barrier is a tall one. Jumping over and sliding under fall out of the same overlap test.
- **Track made of data.** Hazards, pickups and the patterns they appear in are assets. New content doesn't need new code.
- **Difficulty that stays fair.** Speed rises along a curve and harder patterns unlock by tier, but rows are spaced by travel time, so the reaction window doesn't shrink.
- **Seeded runs.** The same seed lays out the same track.
- **Scoring** from distance and pickups, with a combo multiplier and a saved best score.
- **A cinematic start.** The title screen is a camera shot of the runner. Pressing play swings the camera round behind it, and the run begins as it arrives. Shots are stored as orbits and a framing, so the move goes round the runner and the composition holds at any aspect ratio.
- **Feedback on every event.** A flash, a camera shake, a spark burst and the runner going down on a crash; a sparkle and a rising note on each pickup in a streak; a sound for every move. The game-over screen waits for the crash to play out.
- **Sound and effects that only listen.** Neither is called by gameplay code. They react to the messages the game already sends, so taking the sound feature out leaves a silent game and nothing else changes.
- **One codebase, three platforms.** No `#if UNITY_ANDROID` and no platform checks in gameplay or UI. What differs per platform (frame-rate target, active input devices) is a slot in an asset.
- **Layered input.** Gameplay consumes actions like "jump" and never sees a device. Keys and buttons are one source, swipes are another, and the platform's input profile decides which exist. A press on an on-screen button is never also a swipe.
- **UI in UI Toolkit, model–view–presenter.** Four screens in one document, presenters with no engine types that are tested against fake views, and a layout that keeps clear of notches and scales to any aspect ratio.
- **No per-frame garbage** in the gameplay loop or the HUD: entities and their views are pooled, messages are structs, and the score is drawn without building a string. Tests fail if any of it allocates.

## Tech stack

- Unity 6 (`6000.5.8f1`), URP, two small custom HLSL shaders
- C#, VContainer for dependency injection
- Input System, UI Toolkit, Unity Test Framework

## Architecture

```
App        composition roots, game flow, platform set-up
Features   Pace · Player · Track · Scoring · Screens · Cameras · Effects · Sound      (none references another)
Shared     contracts between features
Core       com.roadandcode.core, game-agnostic
```

Each layer is its own assembly and can only see the layers below it, so a feature reaching into another feature doesn't compile. Game rules and UI presenters are plain C# classes with no `MonoBehaviour`, which is why they are covered by 320+ EditMode tests. There are no singletons and no `Update` methods on gameplay objects: one simulation loop ticks every system in a fixed, documented order.

[docs/architecture.md](docs/architecture.md) has the module map, the frame order, the message table, the platform and input layers, how the screens, camera and sound are put together, and the reasoning behind the decisions.

## Build & run

Open the project in Unity `6000.5.8f1` and press Play in `Assets/_Project/Content/Scenes/Bootstrap.unity`. Always start from Bootstrap: it loads the gameplay scene itself. To try the phone or browser set-up in the editor, tick **Simulate Platform** on the `App` object in that scene.

From a terminal:

```bash
# tests
Unity -batchmode -projectPath . -runTests -testPlatform EditMode -testResults Logs/editmode.xml
Unity -batchmode -projectPath . -runTests -testPlatform PlayMode -testResults Logs/playmode.xml

# builds, into Builds/<target>
Unity -batchmode -quit -projectPath . -buildTarget WebGL -executeMethod RoadAndCode.Core.Editor.CommandLineBuild.Build
Unity -batchmode -quit -projectPath . -buildTarget StandaloneWindows64 -executeMethod RoadAndCode.Core.Editor.CommandLineBuild.Build
Unity -batchmode -quit -projectPath . -buildTarget Android -executeMethod RoadAndCode.Core.Editor.CommandLineBuild.Build
```
