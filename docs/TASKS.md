# 📋 ZONE — Development Task List
## Linear Build Roadmap

> Tasks are ordered sequentially. Each phase must be substantially complete before moving to the next, though some parallel work is expected.

---

## Phase 0: Project Foundation (Week 1–2)

- [ ] 0.1 Create Unity project (2022.3 LTS) with WebGL build target
- [ ] 0.2 Configure Unity project settings (quality tiers, rendering pipeline — URP)
- [ ] 0.3 Set up Git LFS for large binary assets (models, textures, audio)
- [ ] 0.4 Create project folder structure (Scripts, Prefabs, Scenes, Art, Audio, Materials)
- [ ] 0.5 Install essential Unity packages (Cinemachine, Input System, TextMeshPro, Addressables)
- [ ] 0.6 Set up Blender-to-Unity asset pipeline (FBX export script, material conventions)
- [ ] 0.7 Configure .env file with all credentials (GitHub, Vercel, Unity, Blender)
- [ ] 0.8 Set up Vercel project and `vercel.json` for WebGL deployment
- [ ] 0.9 Create CI/CD GitHub Action (auto-build + deploy on push to main)
- [ ] 0.10 Create web hub page (project status, playable build link)

---

## Phase 1: Core Player Systems (Week 3–5)

- [ ] 1.1 First-person camera rig with look sensitivity settings
- [ ] 1.2 Player movement controller (walk, sprint, crouch, jump)
- [ ] 1.3 Stamina system (sprint drain, regen, exhaustion state)
- [ ] 1.4 Encumbrance system (weight-based movement speed modification)
- [ ] 1.5 Player health system (HP, damage, regen, death state)
- [ ] 1.6 Injury/limb damage system (wounds affect movement)
- [ ] 1.7 Inventory system framework (weight-based, backpack slots)
- [ ] 1.8 Quick-slot hotbar (weapon/consumable binding)
- [ ] 1.9 Interaction system (raycast-based object interaction)
- [ ] 1.10 Player input mapping (Unity Input System — keyboard/mouse + gamepad)

---

## Phase 2: Weapon & Combat Systems (Week 6–8)

- [ ] 2.1 Weapon system architecture (base class, fire modes, ammo types)
- [ ] 2.2 Raycast-based shooting with bullet drop and penetration
- [ ] 2.3 Weapon recoil and accuracy model
- [ ] 2.4 Weapon condition system (durability, jamming, misfire)
- [ ] 2.5 Melee combat system (light/heavy attacks, blocking)
- [ ] 2.6 Damage types and resistances (ballistic, fire, explosive, monster-specific)
- [ ] 2.7 Hit feedback (hit markers, damage indicators, screen shake)
- [ ] 2.8 Ammo and magazine management (reload, partial reloads, ammo switch)
- [ ] 2.9 Throwable system (grenades, flares, distractions)
- [ ] 2.10 Weapon model setup (viewmodel rig, animations, muzzle flash)

---

## Phase 3: AI & Monster Systems (Week 9–12)

- [ ] 3.1 AI controller framework (state machine, behavior tree integration)
- [ ] 3.2 NavMesh baking system for open-world terrain
- [ ] 3.3 AI perception system (sight, sound, smell detection)
- [ ] 3.4 Burrower AI (burrow, ambush, erupt attack)
- [ ] 3.5 Stalker AI (pack behavior, flanking, light avoidance)
- [ ] 3.6 Lurker AI (echolocation, sound-based tracking, darkness preference)
- [ ] 3.7 Swarmling AI (swarm grouping, overwhelm behavior)
- [ ] 3.8 Goliath AI (territorial, charge, debris throw)
- [ ] 3.9 Wraith AI (phasing, speed, transparency)
- [ ] 3.10 Monster ecology manager (population tracking, territory assignment)
- [ ] 3.11 Monster spawning system (biome-based, time-of-day-based, population-capped)
- [ ] 3.12 Boss encounter framework (Behemoth — multi-phase, raid mechanics)

---

## Phase 4: Faction Systems (Week 13–15)

- [ ] 4.1 Faction data model (reputation, territories, relationships)
- [ ] 4.2 Faction reputation system (player standing, events that modify it)
- [ ] 4.3 Faction NPC controller (patrols, guard posts, combat behavior)
- [ ] 4.4 Wardens faction implementation (heavy armor, suppressive tactics)
- [ ] 4.5 Scourge faction implementation (melee, monster-baiting tactics)
- [ ] 4.6 Drifters faction implementation (adaptive, evasive tactics)
- [ ] 4.7 Traders' Guild implementation (neutral safe zones, economy)
- [ ] 4.8 Faction warfare system (autonomous territory battles)
- [ ] 4.9 Territory control map (zone ownership tracking, shift logic)
- [ ] 4.10 Faction dialogue system (greetings, threats, quest hooks)

---

## Phase 5: World Building (Week 16–22)

### 5A: Terrain & Environment
- [ ] 5.1 Heightmap generation for 25km² world (Blender sculpt → Unity terrain)
- [ ] 5.2 Biome sculpting (Crater, Dead Forest, City, Wetlands, Highlands, Underground)
- [ ] 5.3 Terrain texturing (splat maps, biome-specific materials)
- [ ] 5.4 Foliage system (grass, trees, rocks — GPU instancing)
- [ ] 5.5 Chunk-based world streaming (Addressables scene loading)
- [ ] 5.6 Environment props (abandoned vehicles, wreckage, ruins, barricades)

### 5B: Blender Asset Creation
- [ ] 5.7 Architectural models (buildings, bunkers, outposts, tunnels)
- [ ] 5.8 Prop models (weapons, crates, supplies, furniture, vehicles)
- [ ] 5.9 Monster models (all 7 species + variants)
- [ ] 5.10 Character models (4 faction armor sets + NPCs)
- [ ] 5.11 Texture baking (PBR materials, optimized for WebGL)
- [ ] 5.12 Animation rigging (player viewmodel, NPCs, monsters)

### 5C: World Systems
- [ ] 5.13 Day/night cycle system (time progression, skybox, lighting shifts)
- [ ] 5.14 Weather system (fog, rain, storm, wind, particle effects)
- [ ] 5.15 Dynamic event spawner (faction skirmishes, migrations, supply drops)
- [ ] 5.16 Safe zone system (faction-controlled sanctuaries, trader access)
- [ ] 5.17 Loot container system (chests, stashes, corpses, randomized contents)
- [ ] 5.18 Map system (in-world journal, discovered locations, faction territories)

---

## Phase 6: Gameplay Systems (Week 23–27)

- [ ] 6.1 Quest system framework (objectives, triggers, rewards, faction hooks)
- [ ] 6.2 Main story questline (Act 1 — Arrival)
- [ ] 6.3 Main story questline (Act 2 — Escalation)
- [ ] 6.4 Main story questline (Act 3 — The Fracture)
- [ ] 6.5 Side quest pool (faction missions, bounties, exploration quests)
- [ ] 6.6 Crafting system (recipes, material requirements, crafting UI)
- [ ] 6.7 Trading system (vendor inventories, pricing, reputation discounts)
- [ ] 6.8 Economy balancing (shard economy, loot tables, pricing curves)
- [ ] 6.9 Skill tree system (XP gain, skill points, unlocks)
- [ ] 6.10 Player progression saving (save/load system — PlayerPrefs or IndexedDB for WebGL)
- [ ] 6.11 Multiple ending logic (faction reputation checks, world state checks)
- [ ] 6.12 Difficulty settings (damage modifiers, survival toggles, AI aggression)

---

## Phase 7: Audio & Polish (Week 28–31)

- [ ] 7.1 Environmental audio (wind, ambient loops, biome soundscapes)
- [ ] 7.2 Weapon audio (gunfire, reloads, impacts, melee)
- [ ] 7.3 Monster audio (calls, roars, footsteps, unique sound cues)
- [ ] 7.4 UI audio (inventory, menus, notifications)
- [ ] 7.5 Dynamic music system (tension layers, combat stingers, ambient drones)
- [ ] 7.6 Audio mixing and compression (WebGL audio budget)
- [ ] 7.7 Particle effects (muzzle flash, blood, explosions, weather)
- [ ] 7.8 Post-processing (color grading, bloom, fog, motion blur — WebGL-safe)
- [ ] 7.9 Screen feedback (damage vignette, stamina blur, low-health desaturation)
- [ ] 7.10 Animation polish (player movement blend, weapon transitions, NPC idle variety)

---

## Phase 8: WebGL Build & Vercel Deployment (Week 32–34)

- [ ] 8.1 Unity WebGL build configuration (memory settings, compression, loader template)
- [ ] 8.2 Asset bundle optimization (chunk bundling, lazy loading)
- [ ] 8.3 Texture compression pass (Basis Universal/KTX2 for all textures)
- [ ] 8.4 LOD audit (aggressive distance culling for WebGL)
- [ ] 8.5 Build size optimization (target < 100MB initial load)
- [ ] 8.6 WebGL build smoke test (playable in browser, no console errors)
- [ ] 8.7 Vercel deployment config (vercel.json — static hosting, headers, caching)
- [ ] 8.8 GitHub Actions CI/CD pipeline (auto-build Unity WebGL on push, deploy to Vercel)
- [ ] 8.9 Web hub page update (embedded playable build, build version display)
- [ ] 8.10 Load testing (memory profiling, FPS monitoring in browser)

---

## Phase 9: Beta & QA (Week 35–37)

- [ ] 9.1 Internal playtest — full playthrough
- [ ] 9.2 Bug tracking and triage
- [ ] 9.3 Balance pass (weapons, monsters, economy, difficulty)
- [ ] 9.4 Performance profiling and optimization (focus on WebGL bottlenecks)
- [ ] 9.5 Accessibility audit (controls, colorblind, subtitles, arachnophobia toggle)
- [ ] 9.6 Closed beta deployment to Vercel (private access)
- [ ] 9.7 Beta feedback collection and iteration
- [ ] 9.8 Final polish pass
- [ ] 9.9 Production deployment to Vercel (public)
- [ ] 9.10 Post-launch monitoring and hotfix pipeline

---

## Long-Term / Post-Launch

- [ ] L.1 Co-op multiplayer (2–4 players)
- [ ] L.2 Modding support (asset loading, config overrides)
- [ ] L.3 Additional biomes and monster species
- [ ] L.4 New game+ mode
- [ ] L.5 Mobile browser optimization
- [ ] L.6 Steam release (desktop build)
