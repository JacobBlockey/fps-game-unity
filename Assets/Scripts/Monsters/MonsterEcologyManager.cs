using System.Collections.Generic;
using UnityEngine;

namespace ZONE.Monsters
{
    /// <summary>
    /// Tracks monster populations per species, enforces caps,
    /// and adjusts spawn rates based on player hunting pressure,
    /// time of day, weather, and faction culling activity.
    /// </summary>
    public class MonsterEcologyManager : MonoBehaviour
    {
        public static MonsterEcologyManager Instance { get; private set; }

        [Header("Population Caps")]
        [SerializeField] private int maxTotalMonsters = 64;
        [SerializeField] private int maxPerSpecies = 20;

        [Header("Spawn Timing")]
        [SerializeField] private float spawnCheckInterval = 5f;
        [SerializeField] private float nightSpawnMultiplier = 2f;
        [SerializeField] private float fogSpawnBonus = 1.5f;

        [Header("Population Recovery")]
        [SerializeField] private float populationRegenRate = 0.5f; // Per species per hour
        [SerializeField] private float overhuntThreshold = 5f; // Below this count, slowed regen

        private Dictionary<MonsterSpecies, int> _populationCounts = new();
        private Dictionary<MonsterSpecies, float> _populationHealth = new(); // 0–1, drives regen
        private float _spawnTimer;
        private int _totalMonsters;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Initialize species
            var species = System.Enum.GetValues(typeof(MonsterSpecies));
            foreach (MonsterSpecies s in species)
            {
                _populationCounts[s] = 0;
                _populationHealth[s] = 1f; // Start healthy
            }
        }

        private void Update()
        {
            _spawnTimer += Time.deltaTime;
            if (_spawnTimer >= spawnCheckInterval)
            {
                _spawnTimer = 0f;
                ProcessEcology();
            }
        }

        private void ProcessEcology()
        {
            // Regenerate populations for species below max
            foreach (var kvp in _populationCounts)
            {
                MonsterSpecies species = kvp.Key;
                int currentCount = kvp.Value;

                if (currentCount < maxPerSpecies && _totalMonsters < maxTotalMonsters)
                {
                    float health = _populationHealth[species];
                    float regenRate = health < 0.3f ? populationRegenRate * 0.3f : populationRegenRate;

                    // Time-based regen (convert per-hour rate to per-interval)
                    float hoursElapsed = spawnCheckInterval / 3600f;
                    float regen = regenRate * hoursElapsed;

                    if (Random.value < regen)
                    {
                        // Spawn request — in full implementation, find valid spawn point in species territory
                        Debug.Log($"[Ecology] Regenerating {species} population (current: {currentCount})");
                    }
                }
            }
        }

        public void OnMonsterSpawned(MonsterSpecies species, Vector3 position)
        {
            _populationCounts[species]++;
            _totalMonsters++;
        }

        public void OnMonsterDeath(MonsterSpecies species, Vector3 position)
        {
            if (_populationCounts.ContainsKey(species) && _populationCounts[species] > 0)
                _populationCounts[species]--;

            if (_totalMonsters > 0)
                _totalMonsters--;

            // Reduce population health (overhunting effect)
            if (_populationHealth.ContainsKey(species))
            {
                _populationHealth[species] = Mathf.Max(0f, _populationHealth[species] - 0.05f);
            }
        }

        public int GetSpeciesCount(MonsterSpecies species)
        {
            return _populationCounts.TryGetValue(species, out int count) ? count : 0;
        }

        public int TotalMonsters => _totalMonsters;

        /// <summary>
        /// Returns spawn multiplier based on time of day and weather.
        /// </summary>
        public float GetSpawnMultiplier(bool isNight, bool isFoggy)
        {
            float mult = 1f;
            if (isNight) mult *= nightSpawnMultiplier;
            if (isFoggy) mult *= fogSpawnBonus;
            return mult;
        }
    }
}
