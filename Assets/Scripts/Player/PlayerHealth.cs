using UnityEngine;

namespace ZONE.Player
{
    /// <summary>
    /// Player health system — handles damage, healing, regen, and injury/limb system.
    /// </summary>
    public class PlayerHealth : MonoBehaviour
    {
        [Header("Health")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float regenRate = 2f;
        [SerializeField] private float regenDelay = 5f;

        [Header("Limb System")]
        [SerializeField] private float limbDamageThreshold = 25f; // Damage above this affects a limb
        [SerializeField] private float legInjurySpeedMultiplier = 0.5f;
        [SerializeField] private float armInjuryAccuracyMultiplier = 0.6f;

        [Header("Feedback")]
        [SerializeField] private AudioSource damageAudioSource;
        [SerializeField] private AudioClip damageSound;
        [SerializeField] private AudioClip criticalHealthSound;

        public delegate void HealthChangedHandler(float current, float max);
        public event HealthChangedHandler OnHealthChanged;

        public delegate void DeathHandler();
        public event DeathHandler OnDeath;

        private float _currentHealth;
        private float _regenTimer;
        private bool _isDead;

        // Limb health (0–100, 100 = healthy)
        private float _leftLegHealth = 100f;
        private float _rightLegHealth = 100f;
        private float _leftArmHealth = 100f;
        private float _rightArmHealth = 100f;

        public float CurrentHealth => _currentHealth;
        public float MaxHealth => maxHealth;
        public bool IsDead => _isDead;

        // Public limb state for other systems
        public float LegMovementMultiplier
        {
            get
            {
                float avgLeg = (_leftLegHealth + _rightLegHealth) / 2f;
                return avgLeg <= 0f ? legInjurySpeedMultiplier : 1f - (1f - legInjurySpeedMultiplier) * (1f - avgLeg / 100f);
            }
        }

        public float ArmAccuracyMultiplier
        {
            get
            {
                float avgArm = (_leftArmHealth + _rightArmHealth) / 2f;
                return avgArm <= 0f ? armInjuryAccuracyMultiplier : 1f - (1f - armInjuryAccuracyMultiplier) * (1f - avgArm / 100f);
            }
        }

        private void Awake()
        {
            _currentHealth = maxHealth;
        }

        private void Update()
        {
            if (_isDead) return;

            HandleRegen();
        }

        private void HandleRegen()
        {
            _regenTimer += Time.deltaTime;
            if (_regenTimer >= regenDelay && _currentHealth < maxHealth)
            {
                _currentHealth += regenRate * Time.deltaTime;
                _currentHealth = Mathf.Min(_currentHealth, maxHealth);
                OnHealthChanged?.Invoke(_currentHealth, maxHealth);
            }
        }

        /// <summary>
        /// Apply damage to the player. Optionally specify which limb takes the hit.
        /// </summary>
        public void TakeDamage(float amount, DamageType damageType = DamageType.Ballistic, Limb targetLimb = Limb.Torso)
        {
            if (_isDead) return;

            _currentHealth -= amount;
            _regenTimer = 0f;

            // Apply limb damage if the hit is significant
            if (amount >= limbDamageThreshold)
            {
                ApplyLimbDamage(targetLimb, amount);
            }

            // Feedback
            if (damageAudioSource && damageSound)
                damageAudioSource.PlayOneShot(damageSound);

            if (_currentHealth <= 50f && criticalHealthSound && !damageAudioSource.isPlaying)
                damageAudioSource.PlayOneShot(criticalHealthSound);

            OnHealthChanged?.Invoke(_currentHealth, maxHealth);

            if (_currentHealth <= 0f)
            {
                Die();
            }
        }

        public void Heal(float amount)
        {
            if (_isDead) return;

            _currentHealth = Mathf.Min(_currentHealth + amount, maxHealth);
            OnHealthChanged?.Invoke(_currentHealth, maxHealth);
        }

        public void HealLimb(Limb limb, float amount)
        {
            switch (limb)
            {
                case Limb.LeftLeg:  _leftLegHealth = Mathf.Min(100f, _leftLegHealth + amount); break;
                case Limb.RightLeg: _rightLegHealth = Mathf.Min(100f, _rightLegHealth + amount); break;
                case Limb.LeftArm:  _leftArmHealth = Mathf.Min(100f, _leftArmHealth + amount); break;
                case Limb.RightArm: _rightArmHealth = Mathf.Min(100f, _rightArmHealth + amount); break;
            }
        }

        private void ApplyLimbDamage(Limb limb, float amount)
        {
            float limbDamage = amount * 0.3f;
            switch (limb)
            {
                case Limb.LeftLeg:  _leftLegHealth = Mathf.Max(0f, _leftLegHealth - limbDamage); break;
                case Limb.RightLeg: _rightLegHealth = Mathf.Max(0f, _rightLegHealth - limbDamage); break;
                case Limb.LeftArm:  _leftArmHealth = Mathf.Max(0f, _leftArmHealth - limbDamage); break;
                case Limb.RightArm: _rightArmHealth = Mathf.Max(0f, _rightArmHealth - limbDamage); break;
                case Limb.Torso: break; // Torso damage goes straight to health
            }
        }

        private void Die()
        {
            _isDead = true;
            _currentHealth = 0f;
            OnDeath?.Invoke();
        }
    }

    public enum DamageType
    {
        Ballistic,
        Fire,
        Explosive,
        Monster,
        Fall
    }

    public enum Limb
    {
        Head,
        Torso,
        LeftArm,
        RightArm,
        LeftLeg,
        RightLeg
    }
}
