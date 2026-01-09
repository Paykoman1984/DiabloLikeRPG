// Assets/Scripts/Debug/ForceDamageTest.cs
using UnityEngine;

public class ForceDamageTest : MonoBehaviour
{
    public GameObject enemy;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) && enemy != null)
        {
            Debug.Log("=== FORCE DAMAGE TEST ===");

            // Try to find Hurtbox on enemy
            Hurtbox[] hurtboxes = enemy.GetComponentsInChildren<Hurtbox>();
            Debug.Log($"Found {hurtboxes.Length} Hurtboxes on enemy");

            foreach (Hurtbox hurtbox in hurtboxes)
            {
                Debug.Log($"Hurtbox: {hurtbox.name}, Owner: {hurtbox.owner?.name}");

                // Create damage info
                DamageInfo damageInfo = new DamageInfo(gameObject, 20f)
                {
                    physicalDamage = 20f,
                    hitPoint = enemy.transform.position,
                    hitDirection = Vector2.up
                };

                // Apply damage directly
                Debug.Log($"Applying 20 damage directly to {hurtbox.owner?.name}");
                hurtbox.TakeDamage(damageInfo);
            }
        }
    }
}