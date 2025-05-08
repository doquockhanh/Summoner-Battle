# System Patterns

## System Architecture
- Modular game loop (separation of summoning, battle, and progression logic)
- Data-driven unit and ability definitions

## Key Technical Decisions
- Use of Unity for rapid prototyping and cross-platform support
- ScriptableObjects for unit/ability data
- Event-driven UI updates

## Design Patterns
- Singleton for game manager
- Observer for event handling
- State pattern for game phases

## Component Relationships
- GameManager coordinates game flow
- UnitManager handles unit instantiation and state
- UIManager updates interface based on game events 