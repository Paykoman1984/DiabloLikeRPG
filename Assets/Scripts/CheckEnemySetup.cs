using UnityEngine;

public class CheckEnemySetup : MonoBehaviour
{
    void Start()
    {
        Debug.Log("\n============================================================");
        Debug.Log("🔍 CHECKING ENEMY SETUP");

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemy in enemies)
        {
            Debug.Log($"\n--- Checking: {enemy.name} ---");

            // Layer and tag
            Debug.Log($"Layer: {enemy.layer} ({LayerMask.LayerToName(enemy.layer)})");
            Debug.Log($"Tag: {enemy.tag}");

            // Check colliders
            Collider2D[] colliders = enemy.GetComponents<Collider2D>();
            Debug.Log($"Found {colliders.Length} collider(s):");

            bool hasTrigger = false;
            foreach (Collider2D col in colliders)
            {
                Debug.Log($"- {col.GetType().Name}: Trigger={col.isTrigger}, Enabled={col.enabled}");

                if (col.isTrigger)
                {
                    hasTrigger = true;
                }

                // AUTO-FIX: Make it a trigger if it's not
                if (!col.isTrigger)
                {
                    Debug.Log($"  ⚠ FIXING: Setting {col.GetType().Name} as trigger!");
                    col.isTrigger = true;
                    hasTrigger = true;
                }
            }

            if (!hasTrigger)
            {
                Debug.Log($"❌ WARNING: No trigger collider! Adding one...");
                CircleCollider2D newTrigger = enemy.AddComponent<CircleCollider2D>();
                newTrigger.isTrigger = true;
                newTrigger.radius = 0.5f;
                Debug.Log($"✅ Added trigger collider");
            }

            // Check for IDamageable
            IDamageable damageable = enemy.GetComponent<IDamageable>();
            if (damageable != null)
            {
                Debug.Log($"✅ Has IDamageable interface");

                // Try to get the component type
                MonoBehaviour script = damageable as MonoBehaviour;
                if (script != null)
                {
                    Debug.Log($"  Component: {script.GetType().Name}");
                }
            }
            else
            {
                Debug.Log($"❌ No IDamageable component found!");

                // Check all components to see what's available
                Debug.Log($"  Available components on {enemy.name}:");
                Component[] allComponents = enemy.GetComponents<Component>();
                foreach (Component comp in allComponents)
                {
                    Debug.Log($"  - {comp.GetType().Name}");
                }
            }
        }

        // Check layer setup
        Debug.Log("\n--- Layer Setup ---");
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        int enemyHurtboxLayer = LayerMask.NameToLayer("EnemyHurtbox");

        Debug.Log($"'Enemy' layer index: {(enemyLayer == -1 ? "NOT FOUND" : enemyLayer.ToString())}");
        Debug.Log($"'EnemyHurtbox' layer index: {(enemyHurtboxLayer == -1 ? "NOT FOUND" : enemyHurtboxLayer.ToString())}");

        // Check player setup too
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Debug.Log($"\n--- Checking Player ---");
            Debug.Log($"Player: {player.name}");
            Debug.Log($"Player layer: {player.layer} ({LayerMask.LayerToName(player.layer)})");

            // Check PlayerDamageDealer
            PlayerDamageDealer damageDealer = player.GetComponent<PlayerDamageDealer>();
            if (damageDealer != null)
            {
                Debug.Log($"PlayerDamageDealer enemyLayers: {damageDealer.enemyLayers.value}");

                // Check which layers are targeted
                Debug.Log("Target layers breakdown:");
                for (int i = 0; i < 32; i++)
                {
                    if ((damageDealer.enemyLayers.value & (1 << i)) != 0)
                    {
                        Debug.Log($"  - Layer {i}: {LayerMask.LayerToName(i)}");
                    }
                }
            }
        }

        Debug.Log("============================================================\n");
    }
}