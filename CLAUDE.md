# Let's Go to Dungeon! — Project Guide

## Tech Stack
- **Engine:** Unity (C#)
- **UI:** UI Toolkit (UI Builder) — no uGUI Canvas
- **Input:** Unity Input System (new)
- **Camera:** Fixed orthographic-like angle, no rotation
- **Visuals:** Low-poly, stylized 2.5D

## Project Structure
```
Assets/
  Scripts/
    UI/           — HUDController.cs, InteractionUI.cs
    CharacterController.cs
    HealthSystem.cs
    CoinCounter.cs
    InteractionZone.cs
    RestPoint.cs
    Portal.cs
    SceneTransition.cs
    Coin.cs
  UI/
    HUD.uxml / HUD.uss         — health bar + coin counter
    Balloon.uxml / Balloon.uss — world-space interaction balloon
  Prefabs/
  Textures/
  3d/
```

## Scenes
- **Overworld** (index 0) — farm, NPCs, rest points, portal to dungeon
- **Dungeon** (index 1) — combat, enemies, portal back

## Controls
| Key | Action |
|-----|--------|
| WASD | Move |
| Space | Attack (not yet implemented) |
| Enter | Interact / confirm dialogue |

## Core Systems Status

### ✅ Done
- `HealthSystem` — TakeDamage, Heal, OnDeath event
- `CoinCounter` — singleton, DontDestroyOnLoad, persists between scenes
- `CharacterMovement` — WASD, camera-relative, jump disabled (Space reserved for attack)
- `SceneTransition` — fade out/in + async scene load, DontDestroyOnLoad; also `FadeAndDo(Action)` for fade without scene change
- `Portal` — OnTriggerEnter → GoToScene
- `InteractionZone` — trigger-based, shows balloon on enter, Enter key → OnInteract()
- `RestPoint` — extends InteractionZone, heals player to full with fade effect
- `HUDController` + HUD UI — health bar (width driven by HealthPercent), coin label
- `InteractionUI` — world-space balloon positioned via RuntimePanelUtils.ScreenToPanel
- `.gitignore` — Library/, Temp/, build artifacts excluded

### 🔲 MVP Remaining
- Attack system (Space → hitbox → damage)
- Enemy: Zombie (NavMesh, melee, death → +10 coins)
- Player death → return to Overworld
- Dungeon level geometry

## Key Architectural Decisions
- `CoinCounter`, `SceneTransition`, `InteractionUI` all use `DontDestroyOnLoad` — place them only in Overworld scene
- `HUD` object must exist in **every scene** (not DontDestroyOnLoad) — HUDController needs reference to the scene's HealthSystem
- `BalloonUI` is a separate GameObject with its own UIDocument + `BalloonPanelSettings` (Sort Order 5)
- `FadePanelSettings` on SceneTransition — Sort Order 10 (above everything)
- Loot in MVP: kill enemy → +10 coins directly, no inventory
- No ProBuilder — levels built manually in scene editor

## DontDestroyOnLoad Objects (Overworld only)
| Object | Script |
|--------|--------|
| SceneTransition | SceneTransition |
| CoinCounter | CoinCounter |
| BalloonUI | InteractionUI |

## MVP Development Plan

### Phase 1 — MVP
- [x] UI: HUD (health bar + coins)
- [x] Scene transitions with fade
- [x] Portal Overworld ↔ Dungeon
- [x] Interaction system (balloon + Enter)
- [x] Rest point (heal on interact)
- [ ] Attack (Space + hitbox)
- [ ] Enemy Zombie (NavMesh + death → coins)
- [ ] Player death → scene transition

### Phase 2 — Core Systems
- [ ] Dialogue system (FF9-style speech balloons, multi-line)
- [ ] Interactive objects (well: spend coin → get coins)
- [ ] Inventory + items
- [ ] Farming (plant → grow → harvest carrots)
- [ ] NPC trader
- [ ] Day/Night cycle
- [ ] Save system

### Phase 3 — Polish
- [ ] VFX (portal swirl, hit flash, loot sparkle)
- [ ] Audio (combat SFX, ambient loops)
- [ ] Mobile touch controls
- [ ] Multiple dungeon rooms
- [ ] Final boss (optional)
