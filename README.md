# Super Smash Blocks

Super Smash Blocks is a local 2.5D platform fighter inspired by Super Smash Bros and built in Unity. The project features Batman, Joker, and Red Hood in fast-paced matches with combos, knockback, life pickups, cinematic intro animations, and end-of-match victory sequences.

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Requirements](#requirements)
- [Getting Started](#getting-started)
- [Project Structure](#project-structure)
- [Game Flow and Scenes](#game-flow-and-scenes)
- [Playable Characters](#playable-characters)
- [Controls and Input](#controls-and-input)
- [Core Systems](#core-systems)
- [Build Notes](#build-notes)
- [Development Notes](#development-notes)

## Overview

Super Smash Blocks is designed as a local fighting game with both `SinglePlayer` and local `Multiplayer` modes.

| Field | Details |
| --- | --- |
| Engine | Unity 6.0.4.0f1 |
| Genre | 2.5D Platform Fighter |
| Modes | SinglePlayer, Local Multiplayer |
| Target Platform | PC / Mac |
| Language | C# |

## Features

- Local versus gameplay with two selectable modes: human vs human and human vs AI.
- Three playable DC-inspired characters: Batman, Joker, and Red Hood.
- Combo-based combat with knockback, dodge, respawn, and life-based elimination.
- Dynamic camera that keeps both fighters visible and adjusts FOV in real time.
- Character intro animations at the start of the match and victory animations at the end.
- Heart pickups that can restore lives during battle.
- Support for keyboard, standard gamepads, and PS5 `DualSense` controllers through Unity Input System.
- `NavMesh`-driven enemy AI for `SinglePlayer`, with selectable difficulty levels.

## Requirements

- Unity `6000.4.0f1`
- Unity Input System package
- TextMeshPro

The Unity editor version can be confirmed in [`ProjectSettings/ProjectVersion.txt`](/Users/joack/UPB/7th semester/Virtual Environment/Super Smash Blocks/ProjectSettings/ProjectVersion.txt).

## Getting Started

1. Clone the repository:

```bash
git clone https://github.com/joackagui/Super-Smash-Blocks.git
```

2. Open Unity Hub.
3. Add the project folder.
4. Open it with Unity `6000.4.0f1`.
5. Load the main scene:

```text
Assets/Scenes/MainMenuScene.unity
```

6. Press Play in the editor.

## Project Structure

```text
Assets/
├── Audios/      # Music and sound effects
├── Images/      # UI images, textures, and victory artwork
├── Inputs/      # Input System .inputactions assets
├── Prefabs/     # Characters, props, pickups, and stage elements
├── Scenes/      # Main game scenes
├── Scripts/     # Gameplay, UI, and manager logic
└── Animations/  # Character and object animation clips/controllers
```

## Game Flow and Scenes

The current playable flow is:

`MainMenuScene` -> `CharacterSelectionScene` -> `StageSelectionScene` -> `ControlsScene` -> `FightScene`

### Main scenes

- `MainMenuScene`
  Handles the menu intro and lets the player choose between `SinglePlayer` and `Multiplayer`.
- `CharacterSelectionScene`
  Lets players choose their characters. In `SinglePlayer`, it also shows AI difficulty selection (`Normal` / `Hard`).
- `StageSelectionScene`
  Lets the player choose the battle stage.
- `ControlsScene`
  Shows the control screen before entering the match.
- `FightScene`
  Runs the main battle, including HUD, dynamic camera, intro sequences, combat, AI activation, and the final victory presentation.
- `TestScene`
  Internal testing scene used during development.

There is also a `VictoryScene` asset in the project, but it is not part of the final gameplay flow currently used. The actual victory sequence is resolved directly inside `FightScene`.

## Playable Characters

All fighters inherit from the base `Character` class.

| Character | Prefab | Notes |
| --- | --- | --- |
| Batman | `Assets/Prefabs/Batman.prefab` | Uses the shared base combat system |
| Joker | `Assets/Prefabs/Joker.prefab` | Includes intro and victory animation support |
| Red Hood | `Assets/Prefabs/RedHood.prefab` | Includes intro and victory animation support |

The architecture is ready for future character-specific abilities and deeper differentiation.

## Controls and Input

Input is managed with Unity's Input System through the assets in `Assets/Inputs/`.

### Supported devices

- Keyboard
- Standard gamepads
- PS5 `DualSense` controllers

### Main gameplay actions

| Action | Description |
| --- | --- |
| `Move` / `Movement` | Horizontal movement |
| `Left` / `Right` | Digital movement alternatives |
| `Jump` / `Up` | Jump |
| `Action1` | Primary attack |
| `Action2` | Secondary / special attack |
| `Dodge` | Evasive movement |
| `Return` / `Back` / `Select` | Menu navigation |

Each player uses separate input assets, which makes it easier to route controls independently.

## Core Systems

### Combat

`Character.cs` handles shared fighter behavior:

- Movement and jumping
- Combo attacks
- Damage and knockback
- Dodge and temporary invulnerability
- Death and respawn
- Intro (`Intro`) and victory (`Exit`) animation playback

### Match flow

`GameManager.cs` manages:

- Player registration
- Character spawning from selected prefabs
- Game mode state
- Match intro sequence
- Victory detection and winner presentation

### Camera

`CameraController.cs` keeps both fighters framed on screen and adjusts FOV dynamically. It also supports special camera poses for match intros and victory presentation.

### Single-player AI

`NavMeshEnemyAI.cs` is used in `SinglePlayer` mode together with `NavMeshAgent`.

The AI can:

- Chase the player
- Reposition on the platform
- Look for heart pickups
- Attack depending on range
- Behave differently based on `Normal` or `Hard` difficulty

### UI and audio

- `UIManager.cs` updates HUD elements such as lives and character visuals.
- `WinnerScreenUI.cs` displays the final prompt after the victory sequence and returns to the main menu.
- `MusicManager.cs` handles background music and SFX.

## Build Notes

To build the game:

1. Open `File -> Build Settings`.
2. Add the required scenes in the correct order.
3. Select `PC, Mac & Linux Standalone`.
4. Build for your preferred target architecture.

Recommended starting scene for testing in editor:

```text
Assets/Scenes/MainMenuScene.unity
```

## Development Notes

- The project follows a typical Unity structure with singleton-style global managers such as `GameManager` and `MusicManager`.
- `Nav.cs` is currently unused and left as a placeholder.
- Character classes are still mostly based on shared logic, so a natural next step would be adding unique abilities, balancing, and more differentiated animations.
- The pause menu still has room for controller-focused UX improvements.
- Online multiplayer is not implemented.
