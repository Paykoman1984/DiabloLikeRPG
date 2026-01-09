using UnityEngine;

public class ManualSpawnTest : MonoBehaviour
{
    public GameObject playerHitboxPrefab;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("=== MANUAL SPAWN TEST ===");
            if (playerHitboxPrefab != null)
            {
                GameObject obj = Instantiate(playerHitboxPrefab, transform.position, Quaternion.identity);
                Debug.Log($"Manual spawn: {obj.name}");
                obj.name = "ManualTest_Hitbox";

                // Check if it has Hitbox component
                Hitbox hb = obj.GetComponent<Hitbox>();
                Debug.Log($"Has Hitbox: {hb != null}");
            }
            else
            {
                Debug.LogError("playerHitboxPrefab not assigned!");
            }
        }
    }
}