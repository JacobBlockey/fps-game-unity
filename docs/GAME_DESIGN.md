# 🎮 ZONE — The Fracture
## Game Design Document (GDD) v1.0

---

## 1. CONCEPT OVERVIEW

**Genre:** Open-World First-Person Shooter / Survival Horror
**Engine:** Unity 2022.3 LTS (WebGL build target)
**3D Assets:** Blender 4.2
**Deployment:** Vercel (web-based playable build)
**Inspiration:** S.T.A.L.K.E.R. (atmosphere, world design), Far Cry (faction warfare), Metro (monster encounters)

### Logline
In a fractured landscape scarred by a catastrophic event known as **The Rupture**, you navigate a living world where warring factions vie for territory, terrifying creatures hunt in the dark, and every decision shapes the balance of power. There is no radiation — only the Rupture's aftermath: a world that is broken, alive, and dangerous.

### Core Pillars
1. **Living World** — NPCs have daily routines, factions wage dynamic wars, and the world evolves with or without the player.
2. **Faction Warfare** — Four major factions compete for control of zones, resources, and influence. Alliances shift; betrayals happen.
3. **Monster Ecology** — Creatures are not random encounters — they have lairs, migration patterns, and territorial behavior.
4. **Player Agency** — Every choice affects faction reputation, world state, and available content.
5. **Atmosphere Over Action** — Tension, dread, and discovery drive the experience rather than nonstop combat.

---

## 2. SETTING

### The World: "The Fracture Zone"
A 25km² open-world map divided into distinct biomes:

| Biome | Description | Threat Level |
|------|-------------|-------------|
| **The Crater** | Ground-zero of the Rupture. Twisted terrain, unstable ground, highest monster density. | Extreme |
| **Dead Forest** | Dense woodland with mutated flora. Low visibility. Monster ambush territory. | High |
| **Abandoned City** | Urban ruins. Faction stronghold territory. Loot-rich but contested. | Variable |
| **The Wetlands** | Foggy marshland with sinkholes. Monster nesting grounds. | High |
| **Highlands** | Elevated rocky terrain. Safe zones, faction outposts. Sniper territory. | Low–Medium |
| **The Underground** | Tunnels, bunkers, and subway networks beneath the surface. Darkness. | Extreme |

### The Rupture (Backstory)
Five years ago, a seismic event of unknown origin fractured the earth across a 200km region. The event — called **The Rupture** — didn't release radiation. Instead, it tore open passages to something deep underground. Creatures that had lived in subterranean ecosystems for millennia were unleashed. The region was quarantined. Governments collapsed. Mercenaries, scavengers, and ideologues moved in. The Zone was born.

---

## 3. FACTIONS

### The Wardens
- **Ideology:** Restore order. militaristic, organized.
- **Playstyle:** Heavy armor, suppressive fire, vehicle support.
- **Territory:** Highlands, Abandoned City (northern sectors)
- **Allies:** Traders' Guild (trade relationship)
- **Enemies:** The Scourge, The Drifters

### The Scourge
- **Ideology:** The Rupture is evolution. Monster-worshipping zealots who use creature parts as armor/weaponry.
- **Playstyle:** Aggressive melee, monster baiting, ambush tactics.
- **Territory:** The Crater, Dead Forest
- **Allies:** None (tolerate Drifters occasionally)
- **Enemies:** Everyone, especially The Wardens

### The Drifters
- **Ideology:** Profit above all. Scavengers, traders, and mercenaries.
- **Playstyle:** Adaptive, well-equipped, avoid direct confrontation.
- **Territory:** Abandoned City (southern sectors), Wetlands (trading posts)
- **Allies:** Traders' Guild
- **Enemies:** The Scourge

### The Traders' Guild
- **Ideology:** Neutral commerce. Control the economy.
- **Playstyle:** Non-combatant but hire mercenaries. Control safe zones.
- **Territory:** Highland outposts, select city sectors
- **Allies:** The Wardens, The Drifters
- **Enemies:** The Scourge (disrupts trade)

### Faction Dynamic System
- Factions wage **autonomous territory wars** — capturing/losing zones in real-time.
- Player reputation with each faction (−100 to +100) affects:
  - Access to safe zones and traders
  - Mission availability
  - AI behavior (hostile, neutral, friendly)
  - Endings
- Betraying a faction has **consequences** — bounties, ambushes, loss of services.

---

## 4. MONSTERS

### Design Philosophy
Monsters are not bags of HP. Each species has:
- **Territory:** Lair locations they defend
- **Behavior cycle:** Hunt → Feed → Rest → Migrate
- **Weaknesses:** Specific damage types, environmental triggers
- **Hierarchy:** Alpha, subordinate, and juvenile variants

### Monster Roster

| Monster | Habitat | Behavior | Threat |
|---------|---------|----------|--------|
| **Burrower** | Wetlands, Underground | Ambush predator. Burrows and erupts beneath prey. | Medium |
| **Stalker** | Dead Forest | Pack hunter. Flanks and surrounds. Avoids light. | High |
| **Goliath** | The Crater | Solitary apex. Massive, territorial. Charges and throws debris. | Extreme |
| **Lurker** | Underground | Blind, echolocation-based. Senses movement and sound. | High |
| **Swarmling** | All biomes | Small, numerous. Overwhelm in groups. Weak individually. | Low–Med |
| **Wraith** | Night, all biomes | Fast, semi-translucent. Phases through cover. Rare. | Extreme |
| **Behemoth** | The Crater (boss) | Raid-level encounter. Requires planning and faction support. | Boss |

### Monster Ecology System
- Monster populations **fluctuate** based on:
  - Player hunting pressure (overhunt → population drops → fewer encounters)
  - Faction culling activity (Scourge protects monsters in their territory)
  - Time of day (most monsters are nocturnal or crepuscular)
  - Weather (fog increases Stalker activity, rain masks player sound)

---

## 5. PLAYER SYSTEMS

### Player Character
- **Perspective:** First-person
- **Identity:** Customizable survivor (no pre-set name — player-defined)
- **Background:** Arrived in the Zone seeking someone (story hook)

### Movement
- Walk, sprint, crouch, prone, slide, climb, swim
- **Stamina system** — sprinting, climbing, and melee drain stamina
- **Encumbrance** — heavier loads reduce movement speed and stamina regen
- **Injury system** — limb damage affects movement (leg wound = limp)

### Combat
- **Weapons:** Pistols, rifles, shotguns, melee, improvised, monster-part weapons (from Scourge)
- **Weapon condition** — degrades with use, can jam/misfire
- **Ammo scarcity** — ammo is currency; not every fight is worth it
- **Ballistics:** Bullet drop, penetration, recoil patterns
- **Stealth:** Sound-based detection, light/shadow visibility, distraction tools

### Survival
- **Health:** Regenerates slowly with rest; stim-paks for immediate recovery
- **Hunger/Thirst:** Optional survival mode (can be toggled)
- **Sleep:** Fatigue affects accuracy and perception
- **Crafting:** Field-craft items from scavenged materials and monster parts

### Inventory
- **Weight-based** inventory (no slot count — realistic encumbrance)
- **Backpack system** — different backpacks offer different capacity/mobility tradeoffs
- **Quick slots** — hotbar for weapons and consumables

---

## 6. WORLD SYSTEMS

### Day/Night Cycle
- Full 24-hour cycle (~1 hour real time)
- **Day:** Faction activity peaks, traders open, safer travel
- **Night:** Monster activity peaks, faction patrols retreat, dangerous
- **Dawn/Dusk:** Transition windows — both factions and monsters active

### Weather System
- Dynamic weather: Clear, fog, rain, storm, anomaly storms
- Weather affects:
  - Visibility (fog reduces draw distance and detection range)
  - Sound propagation (rain masks footsteps)
  - Monster behavior (fog increases Stalker spawns, storms trigger anomaly events)
  - Ballistics (wind affects bullet trajectory)

### Dynamic Events
Random and scripted events that occur across the world:
- **Faction skirmishes** — stumble into ongoing firefights
- **Monster migrations** — large groups moving between territories
- **Supply drops** — contested loot caches (faction and player magnet)
- **Anomaly surges** — temporary environmental hazards
- **Rescue encounters** — NPCs in distress (faction reputation opportunities)

### Territory Control
- Map divided into **control zones** (sectors)
- Each zone has a **dominant faction** (or is contested)
- Zones shift ownership based on:
  - Faction AI warfare (autonomous)
  - Player mission outcomes
  - Player territory capture missions

---

## 7. PROGRESSION

### Skill System
Three skill trees, unlocked through use and trainers:

| Tree | Focus | Example Skills |
|------|-------|----------------|
| **Survivalist** | Crafting, stealth, endurance | Silent movement, extended stamina, advanced recipes |
| **Gunslinger** | Weapon proficiency, accuracy, reload | Recoil control, fast reload, dual wield, sniper breathing |
| **Tactician** | Faction relations, leadership, strategy | Command NPCs, better trade prices, faction mission access |

### Economy
- **Currency:** "Shards" (Rupture crystals — universal trade currency)
- **Income:** Looting, trading, missions, territory rewards
- **Expenses:** Ammo, repairs, medical supplies, equipment, bribes

### Endings
Multiple endings based on:
- Final faction reputation standings
- Key story decisions
- Territory control state at endgame
- Monster ecosystem state (extinct vs thriving)

---

## 8. STORY STRUCTURE

### Act 1: Arrival
- Player enters the Zone, is captured by Wardens
- Learns the basics, meets key NPCs, chooses initial faction leanings
- First monster encounter (Burrower attack on a camp)

### Act 2: Escalation
- Faction war intensifies; player gains reputation and access
- Monster activity surges (The Crater becomes more active)
- Player discovers underground passages and the Rupture's source

### Act 3: The Fracture
- Massive monster outbreak threatens all factions
- Player must unite or dominate factions for a final push
- Confrontation at The Crater — the Rupture's epicenter

---

## 9. AUDIO DESIGN

- **Environmental:** Wind, distant gunfire, creature calls, ambient hum
- **Music:** Minimalist, atmospheric — tense strings, industrial drones
- **Combat:** Weighty gunfire, monster roars, impact feedback
- **UI:** Diegetic where possible (in-world map, physical journal)

---

## 10. TECHNICAL SPECIFICATIONS

| Spec | Detail |
|------|--------|
| Engine | Unity 2022.3 LTS |
| Build Target | WebGL (Vercel deployment) |
| 3D Modeling | Blender 4.2 |
| Texture Format | KTX2 / Basis Universal (WebGL optimized) |
| World Size | 25km² (streamed chunks) |
| Max Concurrent AI | 64 active agents |
| Target FPS | 60 (desktop browsers) |
| Memory Budget | 2GB (WebGL constraints) |
| Multiplayer | Single-player (future: co-op consideration) |

### Performance Strategy for WebGL
- Asset bundling and streaming (chunk-based world loading)
- LOD system with aggressive distance culling
- Compressed textures (Basis Universal)
- Object pooling for all AI and projectiles
- Baked lighting for environment, real-time for dynamic objects only

---

## 11. ACCESSIBILITY

- Remappable controls
- Colorblind modes (faction colors have distinct patterns, not just hue)
- Subtitles for all dialogue
- Difficulty settings (damage modifiers, survival toggle, monster aggression)
- Arachnophobia toggle (replaces spider-like creatures with alternative models)
