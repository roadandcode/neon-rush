# Neon Rush

A fast WebGL arcade runner, built small but structured like a production game.

**Status:** in development. The run is playable from the keyboard or a gamepad: three lanes, jump, slide, three kinds of hazard, pickups with a combo multiplier, and a difficulty curve. Menus and the HUD are next.

## Overview

Neon Rush is my take on the endless runner, aimed at the browser: it has to download quickly, start quickly and hold its frame rate on an ordinary laptop. I'm using it to show how I structure a Unity project when I expect it to grow: modules with enforced boundaries, logic that runs without the engine, content that is data, and one place where everything is wired together.

## Controls

| Action | Keyboard | Gamepad |
| --- | --- | --- |
| Start / restart | Enter or Space | A / Cross |
| Change lane | A / D or Left / Right | D-pad or left stick |
| Jump | W, Up or Space | A / Cross |
| Slide (in the air: dive) | S or Down | B / Circle |
| Pause | Esc or P | Start |

## Features

- **Lane runner** with a jump arc, a slide, a mid-air dive, and a short input buffer so a press that is a few frames early still counts.
- **Three hazards from one rule.** A hurdle is a low box, a gate is a box that starts above a sliding runner, a barrier is a tall one. Jumping over and sliding under fall out of the same overlap test.
- **Track made of data.** Hazards, pickups and the patterns they appear in are assets. New content doesn't need new code.
- **Difficulty that stays fair.** Speed rises along a curve and harder patterns unlock by tier, but rows are spaced by travel time, so the reaction window doesn't shrink.
- **Seeded runs.** The same seed lays out the same track.
- **Scoring** from distance and pickups, with a combo multiplier and a saved best score.
- **No per-frame garbage** in the gameplay loop: entities and their views are pooled and messages are structs. Tests fail if the player, track or scoring loop allocates.

## Tech stack

- Unity 6 (`6000.5.8f1`), URP, two small custom HLSL shaders
- C#, VContainer for dependency injection
- Input System, Unity Test Framework

## Architecture

```
App        composition roots, game flow
Features   Pace · Player · Track · Scoring      (none references another)
Shared     contracts between features
Core       com.roadandcode.core, game-agnostic
```

Each layer is its own assembly and can only see the layers below it, so a feature reaching into another feature doesn't compile. Game rules are plain C# classes with no `MonoBehaviour`, which is why they are covered by 140+ EditMode tests. There are no singletons and no `Update` methods on gameplay objects: one simulation loop ticks every system in a fixed, documented order.

[docs/architecture.md](docs/architecture.md) has the module map, the frame order, the message table and the reasoning behind the decisions.

## Build & run

Open the project in Unity `6000.5.8f1` and press Play in `Assets/_Project/Content/Scenes/Bootstrap.unity`. Always start from Bootstrap: it loads the gameplay scene itself.

From a terminal:

```bash
# tests
Unity -batchmode -projectPath . -runTests -testPlatform EditMode -testResults Logs/editmode.xml
Unity -batchmode -projectPath . -runTests -testPlatform PlayMode -testResults Logs/playmode.xml

# WebGL build into Builds/WebGL
Unity -batchmode -quit -projectPath . -buildTarget WebGL -executeMethod RoadAndCode.Core.Editor.CommandLineBuild.Build
```
