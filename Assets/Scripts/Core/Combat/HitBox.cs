// Assets/Scripts/Combat/Hitbox.cs
using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
    [Header("Hitbox Settings")]
    public LayerMask targetLayers;
    public float damage = 10f;
    public DamageType damageType = DamageType.Physical;
    public bool isCritical = false;
    public float criticalMultiplier = 1.5f;
    public GameObject owner;

    [Header("Timing")]
    public float activeDuration = 0.2f;
    public bool destroyOnHit = true;

    [Header("Visual Feedback")]
    public GameObject hitEffectPrefab;
    public AudioClip hitSound;

    private Collider2D hitCollider;
    private HashSet<GameObject> alreadyHit = new HashSet<GameObject>();
    private float activationTime;

    private void Awake()
    {
        hitCollider = GetComponent<Collider2D>();
        if (hitCollider != null)
        {
            hitCollider.enabled = false;
            Debug.Log($"Hitbox.Awake: Collider found - {hitCollider.GetType().Name}, initially disabled");
        }
        else
        {
            Debug.LogError("Hitbox.Awake: No Collider2D found on Hitbox GameObject!");
        }
    }

    public void Activate(GameObject owner)
    {
        Debug.Log($"=== HITBOX ACTIVATE ===");
        Debug.Log($"Hitbox: {name}");
        Debug.Log($"Owner: {owner?.name ?? "null"}");

        this.owner = owner;
        alreadyHit.Clear();

        if (hitCollider != null)
        {
            hitCollider.enabled = true;
            Debug.Log($"Collider enabled: {hitCollider.enabled}");
        }

        activationTime = Time.time;

        // Auto-deactivate after duration
        if (activeDuration > 0)
        {
            Debug.Log($"Will auto-deactivate in {activeDuration} seconds");
            Invoke(nameof(Deactivate), activeDuration);
        }

        // Debug target layers
        string layerNames = "";
        for (int i = 0; i < 32; i++)
        {
            if ((targetLayers.value & (1 << i)) != 0)
            {
                layerNames += $"{i}:{LayerMask.LayerToName(i)}, ";
            }
        }
        Debug.Log($"Target layers: {layerNames}");
    }

    private void Deactivate()
    {
        Debug.Log($"Hitbox.Deactivate: {name}");
        if (hitCollider != null)
        {
            hitCollider.enabled = false;
        }
        Destroy(gameObject, 0.1f);
    }

    // In Hitbox.cs, update OnTriggerEnter2D with EXTREME debugging:
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"=== HITBOX TRIGGER ENTER ===");
        Debug.Log($"Time: {Time.time}");
        Debug.Log($"Hitbox: {name} at {transform.position}");
        Debug.Log($"Other: {other.name} (Layer: {LayerMask.LayerToName(other.gameObject.layer)})");
        Debug.Log($"Other position: {other.transform.position}");
        Debug.Log($"Distance: {Vector3.Distance(transform.position, other.transform.position)}");

        // Check layer
        bool layerMatch = ((1 << other.gameObject.layer) & targetLayers) != 0;
        Debug.Log($"Layer match with targetLayers ({targetLayers.value}): {layerMatch}");

        // List all layers in targetLayers
        string targetLayerNames = "";
        for (int i = 0; i < 32; i++)
        {
            if ((targetLayers.value & (1 << i)) != 0)
            {
                targetLayerNames += $"{i}:{LayerMask.LayerToName(i)}, ";
            }
        }
        Debug.Log($"Hitbox target layers: {targetLayerNames}");

        if (!ShouldHitTarget(other.gameObject))
        {
            Debug.Log("ShouldHitTarget returned FALSE");
            return;
        }

        if (alreadyHit.Contains(other.gameObject))
        {
            Debug.Log("Already hit this target");
            return;
        }

        // Try to find IDamageable EVERYWHERE
        IDamageable damageable = null;

        // Check on the collider's GameObject
        damageable = other.GetComponent<IDamageable>();
        Debug.Log($"IDamageable on {other.name}: {damageable != null}");

        // Check parent
        if (damageable == null)
        {
            damageable = other.GetComponentInParent<IDamageable>();
            Debug.Log($"IDamageable in parent of {other.name}: {damageable != null}");
        }

        // Check children
        if (damageable == null)
        {
            damageable = other.GetComponentInChildren<IDamageable>();
            Debug.Log($"IDamageable in children of {other.name}: {damageable != null}");
        }

        // Check for Hurtbox specifically
        Hurtbox hurtbox = other.GetComponent<Hurtbox>();
        if (hurtbox == null) hurtbox = other.GetComponentInParent<Hurtbox>();
        if (hurtbox == null) hurtbox = other.GetComponentInChildren<Hurtbox>();

        if (hurtbox != null)
        {
            Debug.Log($"Found Hurtbox: {hurtbox.name}, Owner: {hurtbox.owner?.name}");
            damageable = hurtbox;
        }

        if (damageable != null)
        {
            Debug.Log($"SUCCESS: Found IDamageable on {damageable.gameObject.name}");
            Debug.Log($"IsAlive: {damageable.IsAlive}, Health: {damageable.CurrentHealth}/{damageable.MaxHealth}");

            DamageInfo damageInfo = new DamageInfo(owner, damage)
            {
                physicalDamage = (damageType == DamageType.Physical) ? damage : 0,
                isCritical = isCritical,
                criticalMultiplier = criticalMultiplier,
                hitPoint = other.ClosestPoint(transform.position),
                hitDirection = (other.transform.position - transform.position).normalized
            };

            Debug.Log($"Applying {damage} damage to {damageable.gameObject.name}");

            // Apply damage
            damageable.TakeDamage(damageInfo);

            alreadyHit.Add(other.gameObject);

            if (destroyOnHit)
            {
                Deactivate();
            }
        }
        else
        {
            Debug.LogError($"NO IDAMageable found ANYWHERE on {other.name}!");

            // List all components for debugging
            Debug.Log($"Components on {other.name}:");
            Component[] components = other.GetComponents<Component>();
            foreach (Component comp in components)
            {
                Debug.Log($"  - {comp.GetType().Name}");
            }
        }
    }

    private bool ShouldHitTarget(GameObject target)
    {
        Debug.Log($"\n--- ShouldHitTarget Check ---");
        Debug.Log($"Target: {target.name}");
        Debug.Log($"Target layer: {target.layer} ({LayerMask.LayerToName(target.layer)})");

        // Check layer mask
        bool layerMatches = ((1 << target.layer) & targetLayers) != 0;
        Debug.Log($"Layer matches: {layerMatches}");

        if (!layerMatches)
        {
            Debug.Log($"Layer mismatch! Target is on '{LayerMask.LayerToName(target.layer)}'");
            return false;
        }

        // Don't hit yourself
        if (target == owner)
        {
            Debug.Log("Can't hit owner (same GameObject)");
            return false;
        }

        // Check if target is a child of owner
        if (owner != null && target.transform.IsChildOf(owner.transform))
        {
            Debug.Log("Can't hit child of owner");
            return false;
        }

        // Check tags for ally detection - TEMPORARILY DISABLE FOR DEBUGGING
        Debug.Log("Temporarily skipping ally check for debugging");
        return true;

        /*
        // Original ally check (keep commented for now)
        if (owner != null)
        {
            bool ownerIsPlayer = owner.CompareTag("Player");
            bool targetIsPlayer = target.CompareTag("Player");

            Debug.Log($"Owner is player: {ownerIsPlayer}, Target is player: {targetIsPlayer}");

            if (ownerIsPlayer == targetIsPlayer)
            {
                Debug.Log($"Can't hit ally (same tag)");
                return false;
            }
        }

        Debug.Log($"SHOULD HIT THIS TARGET!");
        return true;
        */
    }

    private void PlayHitEffects(Vector3 position)
    {
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, position, Quaternion.identity);
        }

        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, position);
        }
    }

    // Visual debug in Scene view
    void OnDrawGizmos()
    {
        if (hitCollider != null && hitCollider.enabled)
        {
            Gizmos.color = Color.red;
            if (hitCollider is BoxCollider2D boxCollider)
            {
                Gizmos.DrawWireCube(transform.position + (Vector3)boxCollider.offset, boxCollider.size);
            }
            else if (hitCollider is CircleCollider2D circleCollider)
            {
                Gizmos.DrawWireSphere(transform.position + (Vector3)circleCollider.offset, circleCollider.radius);
            }
        }
    }
}