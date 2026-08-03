<img width="1130" height="608" alt="image" src="https://github.com/user-attachments/assets/a458c8af-d713-420e-a72f-02021618e4f5" />


# 2D Platformer Toolkit

A free, modular toolkit with everything a classic (GBA-style) 2D platformer needs — drop the components into your own project and build levels instead of boilerplate.

**Requires Unity 6000.0+** · built-in Input Manager · no external dependencies

## Features

- 🏃 **Tight character controller** — physics-based motor with acceleration curves, variable jump height, gravity shaping, coyote time, jump buffering and optional double jump
- ❤️ **Health & damage** — hit points with invulnerability frames, contact damage with knockback, kill zones
- 🪙 **Collectibles** — coins and health pick-ups with sound/effect feedback, extensible base class
- 👾 **Enemies** — patrolling walkers that turn at walls and ledges, Mario-style stomping
- 🚩 **Level elements** — checkpoints with automatic respawn, moving platforms that carry the player, springs, level exits
- 🎥 **Camera** — smooth follow with look-ahead and world bounds (or just use Cinemachine, like the demo does)
- 🖥️ **HUD** — coin counter and heart-based health display
- 🧩 **Clean architecture** — namespaced assembly (`PlatformerToolkit.Runtime`), XML-documented public API, tooltips on every field, UnityEvents *and* C# events on everything

## Getting started

1. Clone the repository or import the `Assets/2DPlatformerToolkit` folder into your project.
2. Open `Assets/2DPlatformerToolkit/Demo/Scenes/Playground.unity` and press Play — arrows/A-D to run, Space to jump. It is a complete test level with coins, enemies, checkpoints, a moving platform, a spring and a HUD.
3. Read the [Getting Started guide](Assets/2DPlatformerToolkit/Documentation/GettingStarted.md) for component recipes (player, enemies, checkpoints, moving platforms, HUD and more).

All components live under **Add Component → 2D Platformer Toolkit**.

## Package layout

```
Assets/2DPlatformerToolkit/
├── Runtime/          Gameplay code: Characters, Combat, Collectibles,
│                     Enemies, Level, Cameras, UI, Core
├── Demo/             Demo scene, animations and third-party demo art
├── Documentation/    Getting Started guide
└── CHANGELOG.md
```

* * *

### Check our other Unity packages
➡️ You can also find our other solutions on the **[Unity Asset Store](https://assetstore.unity.com/publishers/25633)**

* * *

### Need help?

**University of Games** is a place for indie creators. We ship practical Unity solutions that speed up game development — tools, ready-made packages, and knowledge you can apply the same day. Everything comes from real production experience.

Full documentation for this package and our other products lives on GitBook:

- **Docs home (About):** https://university-of-games.gitbook.io/welcome/
- **This package:** https://university-of-games.gitbook.io/welcome/products/utilities/shader-pack-cartoon-water-and-environment
- **Community & channels:** https://university-of-games.gitbook.io/welcome/community
- **Unity Asset Store (publisher):** https://assetstore.unity.com/publishers/25633
- **Medium articles:** https://medium.com/university-of-games

Questions about the package? Leave a review question on the [Asset Store page](https://assetstore.unity.com/packages/vfx/shaders/shader-pack-cartoon-water-environment-201242) or reach us through the channels listed on the Community page.
