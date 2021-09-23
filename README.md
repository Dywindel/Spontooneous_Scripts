# Spontooneous - Scripts
Spontooneous (Recently retitled 'Path.OS') is a small puzzle game created by me, which is currently playable at  https://bonpsi.itch.io/path-os. This repo contains all its scripting functionality.

## Animation
Animation controls
## Artifacts
In-Game collectables. Checks and stores player progress. Each Artifact is stored as a premade Scriptable Object.
## Camera
## Commands
Command Pattern implementation. Allows the user to perform any player-based action and have the action be undoable, whilst remaining encapsulated.
## Controls
Player input is collect here. Sort Input takes player input and turns it into more appropriate data. Sort Input is treated as a singleton, allowing UM (The Update Manager) to collected actions input as needed.
## Dialoque
NPC dialogue controls and UI visuals
## Generic
Trigger functions with multiple applications
## Gizmos
For editor scripting, useful for editor visuals
## Lighting
## Loading
Area loader allows Unity to asyncronously load areas as the player nears them.
## Main
The main components of a game scene: Lighting Manager(LM), Audio Manager(AM), Update Manager(UM), Game Manager(GM).
## Natural
## RL
Reference Library. Contains references for hardcoded values, classes, functions, enums, world clock, objects pools and meshes (For editor functions)
## Rift
Rifts are the main puzzle grid/mechanic in the game. All rift functionality and editor scripts are stored here.
## SM
Social Media scripts. These are small scripts used to generate visual elements of the game that look good on social media.
## Test
## UI
All menus and titles, not including dialogue selection.
## Warping
These scripts allow the user to place warp points into the game. The player can approach, unlock them, and use them to travel between unlocked locations.
