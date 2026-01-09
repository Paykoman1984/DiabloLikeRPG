using UnityEngine;

public class AutoFixEnemyCollider : MonoBehaviour
{
    void Start()
    {
        Debug.Log($"=== AUTO-FIXING {name} ===");

        // Get all colliders on this GameObject
        Collider2D[] colliders = GetComponents<Collider2D>();

        foreach (Collider2D col in colliders)
        {
            if (!col.isTrigger)
            {
                Debug.Log($"Setting {col.GetType().Name} as trigger on {name}");
                col.isTrigger = true;
            }
        }

        // Also check children
        Collider2D[] childColliders = GetComponentsInChildren<Collider2D>();
        foreach (Collider2D col in childColliders)
        {
            if (col.gameObject != gameObject && !col.isTrigger)
            {
                Debug.Log($"Setting child {col.name} as trigger");
                col.isTrigger = true;
            }
        }

        Debug.Log($"=== AUTO-FIX COMPLETE FOR {name} ===");
    }
}