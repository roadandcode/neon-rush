# Neon Rush

A fast WebGL arcade runner, built small but structured like a production game.

**Status:** in development. The architecture, core library and game flow are in place and tested. Gameplay is next.

## Overview

Neon Rush is my take on the endless runner, aimed at the browser: it has to download quickly, start quickly and hold its frame rate on an ordinary laptop. I'm using it to show how I structure a Unity project when I expect it to grow: modules with enforced boundaries, logic that runs without the engine, and one place where everything is wired together.

## What's here so far

- **Core library** (`Packages/com.roadandcode.core`): message bus, state machine, simulation loop, object pooling, save store and a seeded random generator. It knows nothing about this game.
- **Game flow**: Boot, Menu, Run, Paused and GameOver, with a declared transition table.
- **Composition root** using VContainer. No singletons, no static state.
- **Tests**: EditMode tests for the core library and the flow, and a PlayMode test that boots the real scene with the real container.

## Tech stack

- Unity 6 (`6000.5.8f1`), URP
- C#, VContainer
- Unity Test Framework

## Architecture

See [docs/architecture.md](docs/architecture.md) for the module map, the dependency rules and the reasoning behind them.

## Build & run

Open the project in Unity `6000.5.8f1` and press Play in `Assets/_Project/Content/Scenes/Bootstrap.unity`.

From a terminal:

```bash
# tests
Unity -batchmode -projectPath . -runTests -testPlatform EditMode -testResults Logs/editmode.xml

# WebGL build into Builds/WebGL
Unity -batchmode -quit -projectPath . -buildTarget WebGL -executeMethod RoadAndCode.Core.Editor.CommandLineBuild.Build
```
