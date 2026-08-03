# 2D Platformer Toolkit — Getting Started

A modular toolkit with everything a classic (GBA-style) 2D platformer needs:
a tight character controller, health and damage, collectibles, enemies,
checkpoints, moving platforms, camera follow and HUD widgets.

Requires **Unity 6000.0+** and the built-in Input Manager (default axes
`Horizontal` and `Jump`).

---

## Package layout

```
2DPlatformerToolkit/
├── Runtime/            All gameplay code (PlatformerToolkit.Runtime assembly)
│   ├── Cameras/        CameraFollow2D
│   ├── Characters/     CharacterMotor2D, PlayerController, PlayerAnimator,
│   │                   PlayerRespawner, CharacterSquashStretch
│   ├── Collectibles/   Collectible (base), Coin, HealthPickup, KeyPickup
│   ├── Combat/         Health, IDamageable, ContactDamager, KillZone,
│   │                   DamageFlicker, Projectile, ProjectileLauncher
│   ├── Core/           GameSession, GamePauser, HitStop, LayerMaskExtensions
│   ├── Enemies/        PatrolEnemy, FlyingPatrolEnemy, Stompable
│   ├── Level/          Checkpoint, MovingPlatform, FallingPlatform,
│   │                   OneWayPlatform, Ladder, Bouncer, LockedDoor, LevelExit
│   └── UI/             CoinCounterUI, HealthDisplayUI
├── Demo/               Demo scene, animations, sprites, prefabs and third-party art
└── Documentation/      This guide
```

## Default controls

| Action | Input |
| --- | --- |
| Run | `Horizontal` axis (arrows / A-D) |
| Jump / wall jump | `Jump` (Space) |
| Dash | `Fire1` (Left Ctrl / mouse 0) |
| Climb ladders | `Vertical` axis (up/down) near a ladder |
| Crouch | Down while grounded |
| Drop through one-way platform | Down + Jump |
| Pause (with GamePauser) | `Cancel` (Esc) |

## Demo scene

**Playground** — a full test level exercising every module: coins, a health
pick-up, spikes, a stompable patrol enemy, a checkpoint, a moving platform,
a spring, a kill-zone pit, a level exit and a HUD with the coin counter and
heart display. Use it as a living reference for how components are wired.

All components are available from **Add Component → 2D Platformer Toolkit**.

---

## Setting up a player

1. Create a sprite with a **Rigidbody2D** (Dynamic, freeze Z rotation,
   interpolation on) and a **BoxCollider2D** or **CapsuleCollider2D**.
2. Add **Character Motor 2D** — set *Ground Mask* to your ground layers.
3. Add **Player Controller** — coyote time, jump buffer and air jumps live here.
4. Optional: **Player Animator** (drives `Speed` float and `IsJumping` bool),
   **Health**, **Player Respawner** (checkpoint respawn + fall limit).

The motor never reads input itself — AI or cutscene code can drive it through
`MoveInput`, `Jump()`, `Bounce()` and `SetVelocity()`.

### Motor tuning cheat sheet

| Field | Effect |
| --- | --- |
| Run Speed | Top horizontal speed (units/s) |
| Ground/Air Acceleration | How fast you reach top speed |
| Turn Acceleration | Extra grip when reversing direction (skid feel) |
| Jump Height | Peak height of a full jump, in world units |
| Time To Apex | Seconds to reach the peak — this and Jump Height define gravity; the rigidbody's own gravity scale is ignored |
| Jump Cut Multiplier | How much of the jump is kept on early release |
| Fall Gravity Multiplier | > 1 makes the fall snappier than the rise |
| Apex Threshold / Gravity / Control | Hang time and extra steering at the top of the jump |

---

## Recipes

**Coin** — sprite + trigger collider + **Coin**. The total is tracked by
`GameSession` and displayed by **Coin Counter UI** (assign a TMP text).

**Spikes / hazard** — collider + **Contact Damager**. Set damage, knockback and
target layers. The victim needs a **Health** component.

**Pit** — wide trigger collider + **Kill Zone**, or simply rely on the
respawner's *Fall Limit*.

**Patrolling, stompable enemy** — sprite + Rigidbody2D + collider +
**Patrol Enemy** + **Health** (enable *Destroy On Death*) + **Contact Damager**
(enable *Ignore Contacts From Above*) + **Stompable**.

**Checkpoint** — trigger collider + **Checkpoint**. The **Player Respawner**
automatically uses the most recently touched one.

**Moving platform** — sprite + Rigidbody2D (kinematic) + collider +
**Moving Platform**. Define waypoints relative to the start position; riders
are carried automatically.

**Spring** — collider + **Bouncer** with the desired bounce height.

**Level exit** — trigger collider + **Level Exit**; set the next scene name or
hook the `ExitReached` event.

**Flying enemy** — sprite + Rigidbody2D + collider + **Flying Patrol Enemy**
(waypoints relative to start) + **Health** + **Contact Damager** + **Stompable**.

**One-way platform** — collider + **One Way Platform**. Jump through it from
below; press down + jump to drop through it.

**Falling platform** — sprite + Rigidbody2D + collider + **Falling Platform**;
it shakes when stood on, crumbles and respawns.

**Ladder** — tall trigger collider + **Ladder**; the player climbs it with the
vertical axis and jumps off it freely.

**Turret** — any object + **Projectile Launcher** with the `Pellet` prefab
(Demo/Prefabs) assigned; aim with the fire point's right axis, optionally gate
by *Activation Range*.

**Key & door** — key sprite + trigger + **Key Pickup**; door sprite + solid
collider + **Locked Door**. Keys live in the `GameSession`.

**Pause** — one **Game Pauser** anywhere in the scene; hook menu UI into its
`Paused`/`Resumed` events.

**HUD** — Canvas with a TMP text (**Coin Counter UI**) and a row of images
(**Health Display UI**, assign the player's Health plus full/empty sprites).

---

## Events

Every gameplay component exposes designer-friendly **UnityEvents** in the
inspector (`Landed`, `Jumped`, `Damaged`, `Died`, `Collected`, `Stomped`,
`Bounced`, `Activated`, `ExitReached`, `Respawned`, …) — hook up sounds,
particles and screen shake without writing code. For scripting, C# events such
as `Health.Death`, `CharacterMotor2D.GroundedChanged`,
`GameSession.CoinsChanged` and `Checkpoint.CheckpointActivated` are also
available.
