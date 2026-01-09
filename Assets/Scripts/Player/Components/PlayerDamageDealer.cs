using UnityEngine;

public class PlayerDamageDealer : MonoBehaviour
{
    [Header("Attack Settings")]
    public GameObject attackHitboxPrefab;
    public float baseDamage = 20f;
    public LayerMask enemyLayers;
    public float attackOffset = 0.3f; // How far from player center to spawn hitbox

    [Header("Visual")]
    public Transform attackOrigin; // This will move based on attack direction

    void Start()
    {
        // Make sure AttackPoint exists
        if (attackOrigin == null)
        {
            attackOrigin = transform.Find("AttackPoint");

            if (attackOrigin == null)
            {
                // Create it
                GameObject attackPoint = new GameObject("AttackPoint");
                attackPoint.transform.SetParent(transform);
                attackPoint.transform.localPosition = Vector3.zero; // Start at player center
                attackOrigin = attackPoint.transform;
                Debug.Log("Created AttackPoint at player center");
            }
        }

        if (attackHitboxPrefab == null)
        {
            Debug.LogError("NO ATTACK HITBOX PREFAB ASSIGNED!");
        }
    }

    public void PerformAttack(Vector2 direction)
    {
        if (attackHitboxPrefab == null || attackOrigin == null)
        {
            Debug.LogError("Cannot perform attack: Missing prefab or attack origin!");
            return;
        }

        // Handle zero direction
        if (direction == Vector2.zero)
        {
            direction = Vector2.right;
        }

        direction = direction.normalized;

        Debug.Log($"=== PLAYER ATTACK ===");
        Debug.Log($"Direction: {direction}");
        Debug.Log($"Player position: {transform.position}");

        // CRITICAL FIX: Calculate world position directly, ignoring parent transform
        Vector2 spawnPosition = (Vector2)transform.position + (direction * attackOffset);

        Debug.Log($"Calculated spawn position: {spawnPosition}");

        // OPTIONAL: Also update attackOrigin for visuals/debugging
        attackOrigin.position = spawnPosition;

        // Create hitbox at calculated position
        GameObject hitboxObj = Instantiate(attackHitboxPrefab, spawnPosition, Quaternion.identity);

        Debug.Log($"Hitbox spawned at: {spawnPosition}");

        // Configure the hitbox
        Hitbox hitbox = hitboxObj.GetComponent<Hitbox>();
        if (hitbox != null)
        {
            hitbox.damage = baseDamage;
            hitbox.owner = gameObject;
            hitbox.targetLayers = enemyLayers;
            hitbox.Activate(gameObject);

            Debug.Log($"Hitbox activated! Damage: {baseDamage}");
        }
        else
        {
            Debug.LogError("Instantiated hitbox prefab doesn't have Hitbox component!");
        }
    }

    void OnDrawGizmos()
    {
        if (attackOrigin != null)
        {
            // Draw attack origin position
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackOrigin.position, 0.1f);

            // Draw line from player to attack point
            if (transform != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, attackOrigin.position);
            }
        }

        // Visualize attack range in all directions
        if (Application.isPlaying)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f); // Orange translucent

            // Draw circle showing max attack range
            Gizmos.DrawWireSphere(transform.position, attackOffset);

            // Draw cardinal direction markers
            Vector2[] directions = { Vector2.right, Vector2.left, Vector2.up, Vector2.down };
            foreach (Vector2 dir in directions)
            {
                Vector2 pos = (Vector2)transform.position + (dir * attackOffset);
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(pos, 0.05f);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        // Show attack offset when selected
        if (!Application.isPlaying && attackOrigin == null)
        {
            Gizmos.color = new Color(0f, 1f, 1f, 0.5f); // Cyan
            Gizmos.DrawWireSphere(transform.position, attackOffset);

            // Show where attacks would spawn in 4 directions
            Vector2[] testDirs = {
                Vector2.right,
                Vector2.left,
                Vector2.up,
                Vector2.down
            };

            foreach (Vector2 dir in testDirs)
            {
                Vector2 spawnPos = (Vector2)transform.position + (dir * attackOffset);
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(spawnPos, 0.07f);
                Gizmos.DrawLine(transform.position, spawnPos);
            }
        }
    }
}