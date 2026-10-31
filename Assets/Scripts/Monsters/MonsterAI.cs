using UnityEngine;
using UnityEngine.AI;
using ZONE.AI;

namespace ZONE.Monsters
{
    /// <summary>
    /// Base monster AI — extends AIController with monster-specific behavior:
    /// territory defense, hunting cycles, and species-specific attack patterns.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class MonsterAI : AIController, ZONE.Weapons.IDamageable
    {
        [Header("Monster Identity")]
        [SerializeField] private MonsterSpecies species = MonsterSpecies.Swarmling;

        [Header("Health")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth;

        [Header("Damage & Resistances")]
        [SerializeField] private float attackDamage = 15f;
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private float attackCooldown = 1.5f;
        [SerializeField] private float ballisticResistance = 0f;  // 0 = no resistance, 0.5 = 50% reduction
        [SerializeField] private float fireResistance = 0f;
        [SerializeField] private float explosiveResistance = 0f;

        [Header("Behavior Cycle")]
        [SerializeField] private MonsterState monsterState = MonsterState.Resting;
        [SerializeField] private float territoryRadius = 30f;
        [SerializeField] private Transform lairPosition;

        [Header("Loot")]
        [SerializeField] private MonsterLootTable lootTable;

        private float _lastAttackTime;
        private bool _isDead;

        public MonsterSpecies Species => species;
        public float Health => currentHealth;
        public float MaxHealth => maxHealth;
        public bool IsDead => _isDead;

        protected override void Awake()
        {
            base.Awake();
            currentHealth = maxHealth;
        }

        protected override void Update()
        {
            base.Update();

            if (_isDead) return;

            UpdateMonsterBehavior();
        }

        private void UpdateMonsterBehavior()
        {
            // Species-specific behavior overrides
            switch (species)
            {
                case MonsterSpecies.Burrower:
                    UpdateBurrowerBehavior();
                    break;
                case MonsterSpecies.Stalker:
                    UpdateStalkerBehavior();
                    break;
                case MonsterSpecies.Lurker:
                    UpdateLurkerBehavior();
                    break;
                case MonsterSpecies.Swarmling:
                    UpdateSwarmlingBehavior();
                    break;
                case MonsterSpecies.Goliath:
                    UpdateGoliathBehavior();
                    break;
                case MonsterSpecies.Wraith:
                    UpdateWraithBehavior();
                    break;
            }
        }

        #region Species Behaviors

        private void UpdateBurrowerBehavior()
        {
            if (CurrentState == AIState.Combat && HasTarget)
            {
                // Burrow underground, then erupt beneath the target
                if (Time.time - _lastAttackTime > attackCooldown)
                {
                    // Move to target position underground (fast, invisible)
                    Agent.speed = 8f;
                    Agent.SetDestination(CurrentTarget.position);

                    if (Vector3.Distance(transform.position, CurrentTarget.position) < attackRange)
                    {
                        EruptAttack();
                        _lastAttackTime = Time.time;
                    }
                }
            }
            else
            {
                // Return to lair when no target
                if (lairPosition && Vector3.Distance(transform.position, lairPosition.position) > territoryRadius)
                {
                    Agent.SetDestination(lairPosition.position);
                }
            }
        }

        private void UpdateStalkerBehavior()
        {
            if (CurrentState == AIState.Combat && HasTarget)
            {
                // Pack behavior: flank and surround
                Agent.speed = 6f;

                // Avoid light sources (check for nearby lights)
                Vector3 direction = (CurrentTarget.position - transform.position).normalized;
                Vector3 flankOffset = Vector3.Cross(direction, Vector3.up) * (Random.value > 0.5f ? 1f : -1f) * 3f;
                Vector3 attackPos = CurrentTarget.position + flankOffset;

                Agent.SetDestination(attackPos);

                if (Vector3.Distance(transform.position, CurrentTarget.position) < attackRange &&
                    Time.time - _lastAttackTime > attackCooldown)
                {
                    MeleeAttack();
                    _lastAttackTime = Time.time;
                }
            }
        }

        private void UpdateLurkerBehavior()
        {
            if (CurrentState == AIState.Combat && HasTarget)
            {
                // Echolocation-based: detect by sound, blind to sight
                // Only attack if target is making noise (moving fast, shooting)
                Agent.speed = 5f;

                // Check if target is moving (simple velocity check)
                Rigidbody targetRb = CurrentTarget.GetComponent<Rigidbody>();
                bool targetIsMoving = targetRb != null && targetRb.velocity.magnitude > 1f;

                if (targetIsMoving || Time.time - _lastAttackTime < attackCooldown)
                {
                    Agent.SetDestination(CurrentTarget.position);

                    if (Vector3.Distance(transform.position, CurrentTarget.position) < attackRange &&
                        Time.time - _lastAttackTime > attackCooldown)
                    {
                        MeleeAttack();
                        _lastAttackTime = Time.time;
                    }
                }
                else
                {
                    // Target is still — lurk and wait
                    Agent.isStopped = true;
                }
            }
            else
            {
                Agent.isStopped = false;
            }
        }

        private void UpdateSwarmlingBehavior()
        {
            if (CurrentState == AIState.Combat && HasTarget)
            {
                // Swarm: rush directly at the target
                Agent.speed = 7f;
                Agent.SetDestination(CurrentTarget.position);

                if (Vector3.Distance(transform.position, CurrentTarget.position) < attackRange &&
                    Time.time - _lastAttackTime > attackCooldown)
                {
                    MeleeAttack();
                    _lastAttackTime = Time.time;
                }
            }
        }

        private void UpdateGoliathBehavior()
        {
            if (CurrentState == AIState.Combat && HasTarget)
            {
                // Territorial apex: charge and throw debris
                Agent.speed = 4f; // Slow but powerful
                Agent.SetDestination(CurrentTarget.position);

                float distance = Vector3.Distance(transform.position, CurrentTarget.position);

                if (distance < attackRange && Time.time - _lastAttackTime > attackCooldown)
                {
                    ChargeAttack();
                    _lastAttackTime = Time.time;
                }
                else if (distance > attackRange && distance < 15f && Time.time - _lastAttackTime > attackCooldown)
                {
                    ThrowDebris();
                    _lastAttackTime = Time.time;
                }
            }
        }

        private void UpdateWraithBehavior()
        {
            if (CurrentState == AIState.Combat && HasTarget)
            {
                // Fast, semi-translucent, phases through cover
                Agent.speed = 12f;
                Agent.SetDestination(CurrentTarget.position);

                // Make semi-transparent when not attacking
                Renderer[] renderers = GetComponentsInChildren<Renderer>();
                float distance = Vector3.Distance(transform.position, CurrentTarget.position);
                float alpha = distance > attackRange ? 0.3f : 0.9f;

                foreach (var r in renderers)
                {
                    if (r.material.HasProperty("_Color"))
                    {
                        Color c = r.material.color;
                        c.a = alpha;
                        r.material.color = c;
                    }
                }

                if (distance < attackRange && Time.time - _lastAttackTime > attackCooldown)
                {
                    MeleeAttack();
                    _lastAttackTime = Time.time;
                }
            }
        }

        #endregion

        #region Attacks

        private void MeleeAttack()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position + Vector3.up, attackRange);
            foreach (var hit in hits)
            {
                var damageable = hit.GetComponent<ZONE.Weapons.IDamageable>();
                if (damageable != null && hit.transform != transform)
                {
                    damageable.TakeDamage(attackDamage, ZONE.Player.DamageType.Monster, transform.position, transform.forward);
                }
            }
        }

        private void EruptAttack()
        {
            // AoE attack when emerging from underground
            Collider[] hits = Physics.OverlapSphere(transform.position, attackRange * 1.5f);
            foreach (var hit in hits)
            {
                var damageable = hit.GetComponent<ZONE.Weapons.IDamageable>();
                if (damageable != null && hit.transform != transform)
                {
                    damageable.TakeDamage(attackDamage * 1.5f, ZONE.Player.DamageType.Monster, transform.position, transform.up);
                }
            }
        }

        private void ChargeAttack()
        {
            // High-damage knockback attack
            Collider[] hits = Physics.OverlapSphere(transform.position + transform.forward * 2f + Vector3.up, attackRange * 2f);
            foreach (var hit in hits)
            {
                var damageable = hit.GetComponent<ZONE.Weapons.IDamageable>();
                if (damageable != null && hit.transform != transform)
                {
                    damageable.TakeDamage(attackDamage * 2f, ZONE.Player.DamageType.Monster, hit.transform.position, transform.forward);
                }
            }
        }

        private void ThrowDebris()
        {
            // Ranged attack: throw a physics object at the target
            // In full implementation, spawn a debris projectile with arc trajectory
            if (CurrentTarget)
            {
                Vector3 direction = (CurrentTarget.position - transform.position).normalized;
                Debug.DrawLine(transform.position + Vector3.up * 2f, CurrentTarget.position, Color.yellow, 1f);
            }
        }

        #endregion

        protected override bool IsValidTarget(Transform target)
        {
            if (target == transform) return false;

            // Monsters target the player and faction NPCs (but not Scourge if allied)
            // This can be refined based on monster faction relationships
            if (target.CompareTag("Player")) return true;
            if (target.CompareTag("NPC")) return true;

            return false;
        }

        public void TakeDamage(float amount, ZONE.Player.DamageType damageType, Vector3 hitPoint, Vector3 hitNormal)
        {
            if (_isDead) return;

            float finalDamage = ApplyResistances(amount, damageType);
            currentHealth -= finalDamage;

            // Aggro on attacker direction
            if (!HasTarget)
            {
                _lastKnownTargetPosition = hitPoint;
                ForceState(AIState.Alert);
            }

            if (currentHealth <= 0f)
            {
                Die();
            }
        }

        private float ApplyResistances(float damage, ZONE.Player.DamageType damageType)
        {
            return damageType switch
            {
                ZONE.Player.DamageType.Ballistic  => damage * (1f - ballisticResistance),
                ZONE.Player.DamageType.Fire       => damage * (1f - fireResistance),
                ZONE.Player.DamageType.Explosive  => damage * (1f - explosiveResistance),
                _ => damage
            };
        }

        private void Die()
        {
            _isDead = true;
            currentHealth = 0f;
            Agent.isStopped = true;

            // Drop loot
            if (lootTable)
                lootTable.DropLoot(transform.position);

            // Notify ecology manager
            MonsterEcologyManager.Instance?.OnMonsterDeath(species, transform.position);

            Destroy(gameObject, 5f); // Cleanup after death animation
        }
    }

    public enum MonsterSpecies
    {
        Burrower,
        Stalker,
        Lurker,
        Swarmling,
        Goliath,
        Wraith,
        Behemoth
    }

    public enum MonsterState
    {
        Resting,
        Hunting,
        Feeding,
        Migrating,
        Defending
    }

    [CreateAssetMenu(fileName = "MonsterLootTable", menuName = "ZONE/Monster Loot Table")]
    public class MonsterLootTable : ScriptableObject
    {
        [SerializeField] private LootEntry[] lootEntries;

        public void DropLoot(Vector3 position)
        {
            foreach (var entry in lootEntries)
            {
                if (Random.value <= entry.dropChance)
                {
                    int count = Random.Range(entry.minCount, entry.maxCount + 1);
                    // In full implementation, spawn item prefab at position
                    Debug.Log($"[MonsterLoot] Dropped {count}x {entry.itemName} at {position}");
                }
            }
        }
    }

    [System.Serializable]
    public struct LootEntry
    {
        public string itemName;
        public float dropChance;
        public int minCount;
        public int maxCount;
    }
}
