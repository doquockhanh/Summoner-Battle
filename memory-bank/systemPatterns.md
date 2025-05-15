# System Patterns

## System Architecture
- Modular, component-based architecture for game logic and UI.
- Separation of concerns between game state, rendering, and input handling.
- Data-driven design for summoners, creatures, and abilities.

## Key Technical Decisions
- Use of Unity for rapid prototyping and cross-platform support.
- ScriptableObjects for defining game data (summoners, creatures, abilities).
- Event-driven systems for game state changes and UI updates.

## Design Patterns
- Observer pattern for event handling (e.g., turn changes, ability triggers).
- Factory pattern for instantiating game entities.
- State pattern for managing game phases (e.g., player turn, enemy turn, resolution).

## Component Relationships
- Summoner: Controls creatures, manages resources, issues commands.
- Creature: Executes actions, interacts with battlefield, has stats/abilities.
- Ability: Defines effects, triggers, and costs.
- UI: Presents game state, receives player input, provides feedback. 