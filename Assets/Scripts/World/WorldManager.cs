using UnityEngine;

namespace ZONE.World
{
    /// <summary>
    /// Central world simulation manager.
    /// Coordinates day/night cycle, weather, faction warfare, dynamic events,
    /// and monster ecology. Runs as a singleton.
    /// </summary>
    public class WorldManager : MonoBehaviour
    {
        public static WorldManager Instance { get; private set; }

        [Header("Time")]
        [SerializeField] private float dayDurationMinutes = 60f; // Real minutes per full day
        [SerializeField] private float startTimeHours = 8f; // Start at 8:00 AM

        [Header("World Simulation")]
        [SerializeField] private float factionWarfareInterval = 60f; // Seconds between war sim ticks
        [SerializeField] private float dynamicEventInterval = 120f; // Seconds between event roll checks
        [SerializeField] private float eventChance = 0.4f; // Chance an event spawns per interval

        [Header("References")]
        [SerializeField] private DayNightCycle dayNightCycle;
        [SerializeField] private WeatherSystem weatherSystem;

        private float _currentTimeHours;
        private float _factionWarTimer;
        private float _eventTimer;
        private int _dayCount = 1;

        // World state flags
        public int DayCount => _dayCount;
        public float CurrentTimeHours => _currentTimeHours;
        public bool IsDaytime => _currentTimeHours >= 6f && _currentTimeHours < 18f;

        public delegate void TimeChangedHandler(float hours, int day);
        public event TimeChangedHandler OnTimeChanged;

        public delegate void DayChangedHandler(int newDay);
        public event DayChangedHandler OnDayChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _currentTimeHours = startTimeHours;
        }

        private void Update()
        {
            UpdateTime();
            UpdateFactionWarfare();
            UpdateDynamicEvents();
        }

        private void UpdateTime()
        {
            float hoursPerSecond = 24f / (dayDurationMinutes * 60f);
            _currentTimeHours += hoursPerSecond * Time.deltaTime;

            if (_currentTimeHours >= 24f)
            {
                _currentTimeHours -= 24f;
                _dayCount++;
                OnDayChanged?.Invoke(_dayCount);
            }

            OnTimeChanged?.Invoke(_currentTimeHours, _dayCount);

            if (dayNightCycle)
                dayNightCycle.UpdateTime(_currentTimeHours);
        }

        private void UpdateFactionWarfare()
        {
            _factionWarTimer += Time.deltaTime;
            if (_factionWarTimer >= factionWarfareInterval)
            {
                _factionWarTimer = 0f;
                Factions.FactionManager.Instance?.SimulateFactionWarfare(factionWarfareInterval);
            }
        }

        private void UpdateDynamicEvents()
        {
            _eventTimer += Time.deltaTime;
            if (_eventTimer >= dynamicEventInterval)
            {
                _eventTimer = 0f;

                if (Random.value < eventChance)
                {
                    SpawnDynamicEvent();
                }
            }
        }

        private void SpawnDynamicEvent()
        {
            DynamicEventType eventType = GetRandomEventType();

            switch (eventType)
            {
                case DynamicEventType.FactionSkirmish:
                    // Spawn a faction battle at a random contested zone
                    Debug.Log($"[WorldManager] Faction skirmish event triggered");
                    break;

                case DynamicEventType.MonsterMigration:
                    // Spawn a group of monsters moving between territories
                    Debug.Log($"[WorldManager] Monster migration event triggered");
                    break;

                case DynamicEventType.SupplyDrop:
                    // Spawn a contested loot cache
                    Debug.Log($"[WorldManager] Supply drop event triggered");
                    break;

                case DynamicEventType.AnomalySurge:
                    // Spawn temporary environmental hazard
                    Debug.Log($"[WorldManager] Anomaly surge event triggered");
                    break;

                case DynamicEventType.RescueEncounter:
                    // Spawn an NPC in distress
                    Debug.Log($"[WorldManager] Rescue encounter event triggered");
                    break;
            }
        }

        private DynamicEventType GetRandomEventType()
        {
            float time = _currentTimeHours;
            bool isNight = time < 6f || time >= 18f;

            // Weight events by time of day
            if (isNight)
            {
                // Night: more monster and anomaly events
                float roll = Random.value;
                if (roll < 0.4f) return DynamicEventType.MonsterMigration;
                if (roll < 0.6f) return DynamicEventType.AnomalySurge;
                if (roll < 0.8f) return DynamicEventType.SupplyDrop;
                if (roll < 0.9f) return DynamicEventType.FactionSkirmish;
                return DynamicEventType.RescueEncounter;
            }
            else
            {
                // Day: more faction and rescue events
                float roll = Random.value;
                if (roll < 0.35f) return DynamicEventType.FactionSkirmish;
                if (roll < 0.55f) return DynamicEventType.SupplyDrop;
                if (roll < 0.75f) return DynamicEventType.RescueEncounter;
                if (roll < 0.9f) return DynamicEventType.MonsterMigration;
                return DynamicEventType.AnomalySurge;
            }
        }
    }

    public enum DynamicEventType
    {
        FactionSkirmish,
        MonsterMigration,
        SupplyDrop,
        AnomalySurge,
        RescueEncounter
    }
}
