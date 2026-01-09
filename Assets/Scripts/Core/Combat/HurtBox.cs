// Assets/Scripts/Combat/Hurtbox.cs
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class Hurtbox : MonoBehaviour, IDamageable
{
    [Header("Hurtbox Settings")]
    public GameObject owner;
    public EntityStats stats;

    [Header("Damage Effects")]
    public GameObject damageEffectPrefab;
    public GameObject deathEffectPrefab;
    public AudioClip hurtSound;
    public AudioClip deathSound;

    [Header("Visual Settings")]
    public Color damageFlashColor = Color.red;
    public float flashDuration = 0.1f;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isFlashing = false;

    public bool IsAlive => stats.IsAlive;
    public float CurrentHealth => stats.health;
    public float MaxHealth => stats.maxHealth;

    private void Awake()
    {
        if (owner == null) owner = gameObject;
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        if (stats == null)
        {
            stats = new EntityStats();
        }
    }

    public void TakeDamage(DamageInfo damageInfo)
    {
        if (!IsAlive) return;

        // Calculate final damage with PoE-style mechanics
        float finalDamage = CalculateFinalDamage(damageInfo);

        // Apply damage
        stats.TakeDamage(finalDamage);

        // Trigger events
        CombatEvents.TriggerDamageReceived(damageInfo, gameObject, finalDamage);
        CombatEvents.TriggerHealthChanged(gameObject, stats.health, stats.maxHealth);

        // Visual/audio feedback
        PlayDamageEffects(damageInfo.hitPoint);

        // Flash effect
        if (spriteRenderer != null && !isFlashing)
        {
            StartCoroutine(FlashDamage());
        }

        // Check for death
        if (!IsAlive)
        {
            Die(damageInfo);
        }
    }

    private float CalculateFinalDamage(DamageInfo damageInfo)
    {
        float totalDamage = 0;

        // Physical damage with armor calculation
        if (damageInfo.physicalDamage > 0)
        {
            float physical = damageInfo.physicalDamage;
            if (!damageInfo.ignoreArmor)
            {
                // PoE-style armor formula
                physical = ApplyArmorReduction(physical);
            }
            totalDamage += physical;
        }

        // Elemental damage with resistances
        if (damageInfo.fireDamage > 0)
        {
            totalDamage += stats.ApplyResistance(damageInfo.fireDamage, DamageType.Fire);
        }

        if (damageInfo.coldDamage > 0)
        {
            totalDamage += stats.ApplyResistance(damageInfo.coldDamage, DamageType.Cold);
        }

        if (damageInfo.lightningDamage > 0)
        {
            totalDamage += stats.ApplyResistance(damageInfo.lightningDamage, DamageType.Lightning);
        }

        if (damageInfo.chaosDamage > 0)
        {
            totalDamage += stats.ApplyResistance(damageInfo.chaosDamage, DamageType.Chaos);
        }

        // Apply critical multiplier
        if (damageInfo.isCritical)
        {
            totalDamage *= damageInfo.criticalMultiplier;
        }

        return Mathf.Max(1, totalDamage); // Minimum 1 damage
    }

    private float ApplyArmorReduction(float damage)
    {
        float armor = stats.armor;
        if (armor <= 0) return damage;

        // PoE formula: Damage Reduction = Armor / (Armor + 10 * Damage)
        float reduction = armor / (armor + 10 * damage);
        reduction = Mathf.Min(reduction, 0.9f); // Cap at 90%

        return damage * (1 - reduction);
    }

    public void Heal(float amount)
    {
        stats.Heal(amount);
        CombatEvents.TriggerHealthChanged(gameObject, stats.health, stats.maxHealth);
    }

    private void Die(DamageInfo killingBlow)
    {
        // Trigger death event
        CombatEvents.TriggerEntityDied(gameObject);

        // Play death effects
        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }

        if (deathSound != null)
        {
            AudioSource.PlayClipAtPoint(deathSound, transform.position);
        }

        // Disable the hurtbox
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        // Optional: Disable movement/behavior scripts
        var enemyController = GetComponent<EnemyController>();
        if (enemyController != null)
        {
            enemyController.enabled = false;
        }

        // Optional: Play death animation before destroying
        StartCoroutine(DestroyAfterDelay(2f));
    }

    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Disable or destroy the owner
        if (owner != null)
        {
            Destroy(owner);
        }
    }

    private IEnumerator FlashDamage()
    {
        isFlashing = true;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = damageFlashColor;
            yield return new WaitForSeconds(flashDuration);
            spriteRenderer.color = originalColor;
        }

        isFlashing = false;
    }

    private void PlayDamageEffects(Vector3 position)
    {
        if (damageEffectPrefab != null)
        {
            Instantiate(damageEffectPrefab, position, Quaternion.identity);
        }

        if (hurtSound != null)
        {
            AudioSource.PlayClipAtPoint(hurtSound, position);
        }
    }
}