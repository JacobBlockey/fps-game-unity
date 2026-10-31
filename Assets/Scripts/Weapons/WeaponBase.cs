using UnityEngine;

namespace ZONE.Weapons
{
    /// <summary>
    /// Base weapon class — handles firing, ammo, recoil, and condition.
    /// Subclass for specific weapon types (pistol, rifle, shotgun, etc.).
    /// </summary>
    public abstract class WeaponBase : MonoBehaviour
    {
        [Header("Identity")]
        [SerializeField] protected string weaponName = "Unnamed Weapon";
        [SerializeField] protected WeaponType weaponType = WeaponType.Rifle;

        [Header("Damage")]
        [SerializeField] protected float baseDamage = 25f;
        [SerializeField] protected float fireRate = 600f; // Rounds per minute
        [SerializeField] protected float effectiveRange = 200f;
        [SerializeField] protected float bulletDrop = 9.81f;
        [SerializeField] protected float penetration = 0.5f; // 0 = stops on first hit, 1 = infinite

        [Header("Ammo")]
        [SerializeField] protected int magazineSize = 30;
        [SerializeField] protected float reloadTime = 2.5f;
        [SerializeField] protected AmmoType ammoType = AmmoType.Standard;

        [Header("Accuracy & Recoil")]
        [SerializeField] protected float baseSpread = 0.5f; // Degrees of cone
        [SerializeField] protected float maxSpread = 3f; // Max spread after sustained fire
        [SerializeField] protected float spreadRecovery = 0.5f; // Degrees per second
        [SerializeField] protected Vector2 recoilPattern = new Vector2(0.5f, 0.3f); // Vertical, horizontal

        [Header("Condition")]
        [SerializeField] protected float maxCondition = 100f;
        [SerializeField] protected float conditionLossPerShot = 0.05f;
        [SerializeField] protected float jamChanceAtZeroCondition = 0.15f;

        [Header("Audio")]
        [SerializeField] protected AudioSource audioSource;
        [SerializeField] protected AudioClip fireSound;
        [SerializeField] protected AudioClip dryFireSound;
        [SerializeField] protected AudioClip reloadSound;
        [SerializeField] protected AudioClip jamSound;

        // State
        protected int currentAmmo;
        protected float currentCondition;
        protected float currentSpread;
        protected float lastFireTime;
        protected bool isReloading;
        protected float reloadTimer;
        protected bool isJammed;

        // Accuracy multiplier from player (injury, movement, etc.)
        public float AccuracyMultiplier { get; set; } = 1f;
        public string WeaponName => weaponName;
        public WeaponType WeaponType => weaponType;
        public int CurrentAmmo => currentAmmo;
        public int MagazineSize => magazineSize;
        public float Condition => currentCondition;
        public float MaxCondition => maxCondition;
        public bool IsReloading => isReloading;
        public bool IsJammed => isJammed;

        protected virtual void Awake()
        {
            currentAmmo = magazineSize;
            currentCondition = maxCondition;
            currentSpread = baseSpread;
        }

        protected virtual void Update()
        {
            if (isReloading)
            {
                reloadTimer -= Time.deltaTime;
                if (reloadTimer <= 0f)
                    FinishReload();
            }

            // Recover spread
            if (currentSpread > baseSpread)
                currentSpread = Mathf.Max(baseSpread, currentSpread - spreadRecovery * Time.deltaTime);
        }

        /// <summary>
        /// Attempt to fire the weapon. Returns true if a shot was fired.
        /// </summary>
        public virtual bool Fire(Transform fireOrigin, Vector3 direction)
        {
            if (isReloading || isJammed) return false;

            float timeBetweenShots = 60f / fireRate;
            if (Time.time - lastFireTime < timeBetweenShots) return false;

            // Check ammo
            if (currentAmmo <= 0)
            {
                if (audioSource && dryFireSound) audioSource.PlayOneShot(dryFireSound);
                return false;
            }

            // Check jam
            if (currentCondition <= 0f && Random.value < jamChanceAtZeroCondition)
            {
                Jam();
                return false;
            }

            lastFireTime = Time.time;
            currentAmmo--;

            // Apply spread
            Vector3 fireDirection = ApplySpread(direction);
            PerformShot(fireOrigin, fireDirection);

            // Recoil
            ApplyRecoil();

            // Condition
            currentCondition = Mathf.Max(0f, currentCondition - conditionLossPerShot);

            // Increase spread (bloom)
            currentSpread = Mathf.Min(maxSpread, currentSpread + 0.2f);

            // Audio
            if (audioSource && fireSound) audioSource.PlayOneShot(fireSound);

            return true;
        }

        /// <summary>
        /// Perform the actual shot — override in subclasses for different ballistics.
        /// </summary>
        protected virtual void PerformShot(Transform origin, Vector3 direction)
        {
            RaycastHit hit;
            Vector3 startPos = origin.position;
            Vector3 endPos = startPos + direction * effectiveRange;

            // Simulate bullet drop for far shots
            float distance = effectiveRange;
            if (distance > 50f)
            {
                float flightTime = distance / 800f; // Approximate bullet speed
                float drop = 0.5f * bulletDrop * flightTime * flightTime;
                endPos.y -= drop;
            }

            Vector3 rayDirection = (endPos - startPos).normalized;

            if (Physics.Raycast(startPos, rayDirection, out hit, effectiveRange))
            {
                // Apply damage
                var damageable = hit.collider.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(baseDamage, DamageType.Ballistic, hit.point, hit.normal);
                }

                // Spawn impact effect (handled by a VFX manager in a real implementation)
                Debug.DrawLine(startPos, hit.point, Color.red, 0.1f);
            }
        }

        public virtual void Reload()
        {
            if (isReloading || currentAmmo >= magazineSize) return;
            isReloading = true;
            reloadTimer = reloadTime;
            if (audioSource && reloadSound) audioSource.PlayOneShot(reloadSound);
        }

        protected virtual void FinishReload()
        {
            isReloading = false;
            currentAmmo = magazineSize;
        }

        public virtual void Unjam()
        {
            isJammed = false;
            // Unjamming costs condition
            currentCondition = Mathf.Max(0f, currentCondition - 2f);
        }

        protected void Jam()
        {
            isJammed = true;
            if (audioSource && jamSound) audioSource.PlayOneShot(jamSound);
        }

        protected Vector3 ApplySpread(Vector3 direction)
        {
            float effectiveSpread = currentSpread / AccuracyMultiplier;
            Quaternion spreadRotation = Quaternion.Euler(
                Random.Range(-effectiveSpread, effectiveSpread),
                Random.Range(-effectiveSpread, effectiveSpread),
                0f
            );
            return spreadRotation * direction;
        }

        protected virtual void ApplyRecoil()
        {
            // This should be hooked up to the camera/weapon controller
            // For now, we expose the recoil values
            // CameraController.AddRecoil(recoilPattern.x, Random.Range(-recoilPattern.y, recoilPattern.y));
        }

        public void Repair(float amount)
        {
            currentCondition = Mathf.Min(maxCondition, currentCondition + amount);
        }
    }

    public enum WeaponType
    {
        Pistol,
        Rifle,
        Shotgun,
        Sniper,
        Melee,
        Improvised,
        MonsterPart
    }

    public enum AmmoType
    {
        Standard,
        ArmorPiercing,
        HollowPoint,
        Incendiary,
        MonsterHunter,
        Scrouge
    }

    public interface IDamageable
    {
        void TakeDamage(float amount, DamageType damageType, Vector3 hitPoint, Vector3 hitNormal);
    }
}
