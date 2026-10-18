# Legacy Dungeon

A 2D side-scrolling Unity platformer. Explore three longer dungeon levels, collect coins, find the USB key, and defeat the final boss.

## How to open

1. In Unity Hub, add this folder: `CS15 FINAL PROJECT`.
2. Open it with **Unity 6000.6.3f1** (Unity 6 — already installed). If Hub asks to upgrade, choose **Upgrade**.
3. Open `Assets/Scenes/Main.unity`.
4. Press **Play**.

The first open can take a few minutes while Unity imports packages.

## Controls

| Key | Action |
| --- | --- |
| A / D or Left / Right | Move |
| Space / W / Up | Jump |
| E | Interact (talk, pick up, switch, door) |
| F | Slash during the final boss fight |
| R | Restart after winning or being defeated |

## Goal

1. Talk to Professor Ada and collect gold coins.
2. Flip the red switch to build a bridge, then take the USB key and open the exit.
3. Cross the hazard course, collect its key, and open the next exit.
4. Slash the final boss with **F** until it is defeated.

## Scripts (`Assets/Scripts`)

- `GameBootstrap` — builds the level at runtime (no prefabs required)
- `GameManager` — score, key, dialogue, win/lose HUD
- `PlayerController` — movement, jump, grounded check, interact
- `IInteractable` — shared interaction contract
- `DialogueNpc`, `PickupItem`, `PowerSwitch`, `LockedDoor` — interactable objects
- `MovingPlatform`, `CameraFollow`, `SpriteFactory`

This is a complete playable scene: colored sprites and colliders are created in code so you can play immediately after opening the project.

## Asset credit

Legacy Dungeon background art by Anokolisa, published by Vaca Roxa. Credit is optional; the included `release.txt` contains the asset terms.
