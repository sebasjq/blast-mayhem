# blast-mayhem

Blast Mayhem is a 2D local multiplayer game developed in Unity. Two players compete in a physics-based arena using bombs, special pickups, movement abilities, and different bomb behaviors.

## Gameplay

Players can move around the arena, jump and double jump, collect bombs and special pickups, and throw bombs at each other.

Bomb throws can be charged by holding the throw key. While charging, an aiming line indicates the throw direction, and the launch force increases depending on how long the button is held.

The game currently includes:

- Local multiplayer for two players.
- Character selection.
- Movement and double jump.
- Chargeable bomb throws.
- Health and damage system.
- Animated health bars and character portraits.
- Main menu, settings and game over interface.
- Different bomb and pickup behaviors.

### Bomb Types

- **Normal** – Standard physics-based bomb.
- **Gravity** – Special bomb with controllable gravity behavior.
- **Spring** – Bomb with spring-based movement and recovery mechanics.
- **Sticky** – Bomb designed to stick after making contact.

### Pickups

- Bomb pickup.
- Health pickup.
- Gravity bomb pickup.
- Spring bomb pickup.
- Sticky bomb pickup.

## Controls

| Action | Player 1 | Player 2 |
|---|---|---|
| Move Left | `A` | `←` |
| Move Right | `D` | `→` |
| Jump / Double Jump | `W` | `↑` |
| Throw / Use Bomb | `X` | `Space` |

Hold the bomb key to charge the throw and release it to launch the bomb. The same key is also used for some special bomb behaviors.

## Characters

Four playable character prefabs are currently available:

- Mask Dude
- Ninja Frog
- Pink Man
- Virtual Guy

## Project Structure

```text
Assets/
├── Animations/
├── Art/
├── Pixel Adventure 1/
├── Prefabs/
│   ├── Bomb&PickUps/
│   └── Players/
├── Scenes/
└── Scripts/
    ├── Bombs/
    ├── Interface/
    └── Player/
