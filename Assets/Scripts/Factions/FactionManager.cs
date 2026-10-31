using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZONE.Factions
{
    /// <summary>
    /// Manages all faction data: reputation, relationships, and territory control.
    /// A singleton that persists across the game world.
    /// </summary>
    public class FactionManager : MonoBehaviour
    {
        public static FactionManager Instance { get; private set; }

        [Header("Faction Definitions")]
        [SerializeField] private FactionDefinition[] factionDefinitions;

        private Dictionary<FactionID, FactionState> _factionStates = new();

        // Player reputation: -100 (hated) to +100 (exalted)
        private Dictionary<FactionID, float> _playerReputation = new();

        // Faction relationships: -100 (war) to +100 (allied)
        private Dictionary<(FactionID, FactionID), float> _factionRelationships = new();

        public delegate void ReputationChangedHandler(FactionID faction, float newRep, float oldRep);
        public event ReputationChangedHandler OnReputationChanged;

        public delegate void TerritoryChangedHandler(FactionID faction, string zoneID, FactionID oldOwner);
        public event TerritoryChangedHandler OnTerritoryChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeFactions();
        }

        private void InitializeFactions()
        {
            foreach (var def in factionDefinitions)
            {
                _factionStates[def.factionID] = new FactionState
                {
                    definition = def,
                    controlledZones = new List<string>(),
                    strength = def.startingStrength,
                    isAtWar = new List<FactionID>()
                };

                _playerReputation[def.factionID] = def.startingReputation;
            }

            // Initialize default relationships
            SetRelationship(FactionID.Wardens, FactionID.Scourge, -80f);
            SetRelationship(FactionID.Wardens, FactionID.Drifters, -20f);
            SetRelationship(FactionID.Wardens, FactionID.Traders, 40f);
            SetRelationship(FactionID.Scourge, FactionID.Drifters, -30f);
            SetRelationship(FactionID.Scourge, FactionID.Traders, -60f);
            SetRelationship(FactionID.Drifters, FactionID.Traders, 50f);
        }

        /// <summary>
        /// Get the player's reputation with a faction.
        /// </summary>
        public float GetReputation(FactionID faction)
        {
            return _playerReputation.TryGetValue(faction, out float rep) ? rep : 0f;
        }

        /// <summary>
        /// Get the player's standing tier with a faction.
        /// </summary>
        public ReputationTier GetReputationTier(FactionID faction)
        {
            float rep = GetReputation(faction);
            return rep switch
            {
                >= 75  => ReputationTier.Exalted,
                >= 25  => ReputationTier.Friendly,
                >= -25 => ReputationTier.Neutral,
                >= -75 => ReputationTier.Hostile,
                _      => ReputationTier.Hated
            };
        }

        /// <summary>
        /// Modify the player's reputation with a faction. Clamped to -100/+100.
        /// </summary>
        public void ModifyReputation(FactionID faction, float delta)
        {
            if (!_playerReputation.ContainsKey(faction)) return;

            float oldRep = _playerReputation[faction];
            _playerReputation[faction] = Mathf.Clamp(oldRep + delta, -100f, 100f);

            OnReputationChanged?.Invoke(faction, _playerReputation[faction], oldRep);
        }

        /// <summary>
        /// Check if a faction is hostile to the player.
        /// </summary>
        public bool IsHostileToPlayer(FactionID faction)
        {
            return GetReputation(faction) < -25f;
        }

        /// <summary>
        /// Get the relationship between two factions.
        /// </summary>
        public float GetRelationship(FactionID a, FactionID b)
        {
            if (a == b) return 100f;

            // Try both key orderings
            if (_factionRelationships.TryGetValue((a, b), out float rel)) return rel;
            if (_factionRelationships.TryGetValue((b, a), out rel)) return rel;

            return 0f;
        }

        /// <summary>
        /// Set the relationship between two factions. Bidirectional.
        /// </summary>
        public void SetRelationship(FactionID a, FactionID b, float value)
        {
            value = Mathf.Clamp(value, -100f, 100f);
            _factionRelationships[(a, b)] = value;
            _factionRelationships[(b, a)] = value;

            // If relationship drops below -50, they're at war
            if (value < -50f)
            {
                _factionStates[a].isAtWar.Add(b);
                _factionStates[b].isAtWar.Add(a);
            }
            else
            {
                _factionStates[a].isAtWar.Remove(b);
                _factionStates[b].isAtWar.Remove(a);
            }
        }

        /// <summary>
        /// Check if two factions are at war.
        /// </summary>
        public bool AreAtWar(FactionID a, FactionID b)
        {
            if (!_factionStates.ContainsKey(a)) return false;
            return _factionStates[a].isAtWar.Contains(b);
        }

        /// <summary>
        /// Assign a territory zone to a faction.
        /// </summary>
        public void SetTerritoryOwner(string zoneID, FactionID newOwner)
        {
            // Remove from old owner
            foreach (var kvp in _factionStates)
            {
                if (kvp.Value.controlledZones.Contains(zoneID))
                {
                    FactionID oldOwner = kvp.Key;
                    kvp.Value.controlledZones.Remove(zoneID);
                    _factionStates[newOwner].controlledZones.Add(zoneID);
                    OnTerritoryChanged?.Invoke(newOwner, zoneID, oldOwner);
                    return;
                }
            }

            // No previous owner
            _factionStates[newOwner].controlledZones.Add(zoneID);
            OnTerritoryChanged?.Invoke(newOwner, zoneID, FactionID.None);
        }

        /// <summary>
        /// Get the faction that controls a specific zone.
        /// </summary>
        public FactionID GetTerritoryOwner(string zoneID)
        {
            foreach (var kvp in _factionStates)
            {
                if (kvp.Value.controlledZones.Contains(zoneID))
                    return kvp.Key;
            }
            return FactionID.None;
        }

        /// <summary>
        /// Get all zones controlled by a faction.
        /// </summary>
        public List<string> GetFactionTerritories(FactionID faction)
        {
            return _factionStates.TryGetValue(faction, out var state) ? state.controlledZones : new List<string>();
        }

        /// <summary>
        /// Simulate autonomous faction warfare — called periodically by WorldManager.
        /// </summary>
        public void SimulateFactionWarfare(float deltaTime)
        {
            foreach (var kvp in _factionStates)
            {
                FactionID faction = kvp.Key;
                FactionState state = kvp.Value;

                // For each enemy faction, attempt territory gains/losses
                foreach (FactionID enemy in state.isAtWar)
                {
                    if (enemy <= faction) continue; // Avoid double-processing

                    float myStrength = state.strength;
                    float enemyStrength = _factionStates[enemy].strength;

                    // Weaker faction loses territory slowly
                    if (myStrength < enemyStrength && state.controlledZones.Count > 0)
                    {
                        float lossChance = (enemyStrength - myStrength) / 200f * deltaTime;
                        if (Random.value < lossChance)
                        {
                            string contestedZone = state.controlledZones[Random.Range(0, state.controlledZones.Count)];
                            SetTerritoryOwner(contestedZone, enemy);
                            state.strength -= 5f;
                            _factionStates[enemy].strength += 5f;
                        }
                    }
                }
            }
        }

        public FactionState GetFactionState(FactionID faction)
        {
            return _factionStates.TryGetValue(faction, out var state) ? state : null;
        }
    }

    [Serializable]
    public class FactionDefinition
    {
        public FactionID factionID;
        public string displayName;
        [TextArea] public string description;
        public float startingReputation = 0f;
        public float startingStrength = 100f;
        public Color factionColor = Color.white;
    }

    public class FactionState
    {
        public FactionDefinition definition;
        public List<string> controlledZones;
        public float strength;
        public List<FactionID> isAtWar;
    }

    public enum FactionID
    {
        None,
        Wardens,
        Scourge,
        Drifters,
        Traders
    }

    public enum ReputationTier
    {
        Hated,
        Hostile,
        Neutral,
        Friendly,
        Exalted
    }
}
