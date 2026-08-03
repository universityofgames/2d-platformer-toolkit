# Changelog

All notable changes to the 2D Platformer Toolkit are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [1.0.0] - 2026-08-03

### Added

- **Characters** — `CharacterMotor2D` (designer-friendly jump model driven by
  height + time-to-apex, acceleration curves, snappy turn acceleration,
  variable jump height, apex hang-time assist, gravity shaping, wall slide,
  wall jump, dash, crouch with collider resize, ladder climbing, drop-through
  for one-way platforms, corner correction, wall-stick prevention and
  rigidbody-cast ground detection), `PlayerController` (coyote time, jump
  buffering, air jumps, wall jumps with input lock, dash/crouch/climb input,
  hit-stun on damage), `PlayerAnimator` (optional parameters, skipped when
  absent), `CharacterSquashStretch` (impact-scaled landing squash and jump
  stretch), `PlayerRespawner` (checkpoint respawn, fall limit, respawn
  invulnerability).
- **Combat** — `Health` (invulnerability window, UnityEvents and C# events),
  `IDamageable` / `DamageInfo` contract, `ContactDamager` (knockback,
  stomp-friendly top exemption, hit-stop), `KillZone`, `DamageFlicker`
  (Yoshi-style invulnerability blinking), `Projectile` and
  `ProjectileLauncher` (turrets and traps, range gating).
- **Collectibles** — extensible `Collectible` base with pickup feedback,
  `Coin`, `HealthPickup`, `KeyPickup`.
- **Enemies** — `PatrolEnemy` (wall and ledge detection), `FlyingPatrolEnemy`
  (waypoint flyer), `Stompable` (bounce, damage, hit-stop).
- **Level** — `Checkpoint`, `MovingPlatform` (waypoints, rider carrying),
  `FallingPlatform` (shake, crumble, respawn), `OneWayPlatform`
  (auto-configured effector, drop-through), `Ladder`, `Bouncer`, `LockedDoor`
  (consumes session keys), `LevelExit`.
- **Cameras** — `CameraFollow2D` (smoothing, look-ahead, world bounds).
- **UI** — `CoinCounterUI` (TextMeshPro), `HealthDisplayUI` (heart icons).
- **Core** — `GameSession` (cross-scene coins and keys), `GamePauser`,
  `HitStop` (global freeze-frames), `LayerMaskExtensions`.
- Runtime assembly definition (`PlatformerToolkit.Runtime`).
- Playground demo scene exercising every module, with first-party pixel-art
  sprites and a projectile prefab.

### Changed

- Rebuilt the original single-script character controller into modular,
  namespaced components with full XML documentation and inspector tooltips.
