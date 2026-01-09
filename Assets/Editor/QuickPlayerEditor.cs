// Assets/Editor/QuickPlayerCreator.cs
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class QuickPlayerCreator
{
    [MenuItem("GameObject/2D PoE/Quick Create Player")]
    static void CreateQuickPlayer()
    {
        // Create player object
        GameObject player = new GameObject("Player");

        // Add visual
        SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
        sr.color = Color.blue;

        // Create a simple sprite
        Texture2D tex = new Texture2D(32, 32);
        for (int x = 0; x < 32; x++)
            for (int y = 0; y < 32; y++)
                tex.SetPixel(x, y,
                    Vector2.Distance(new Vector2(x, y), new Vector2(16, 16)) < 12 ?
                    Color.blue : Color.clear);
        tex.Apply();
        sr.sprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));

        // Add physics
        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.linearDamping = 5;
        rb.freezeRotation = true;

        player.AddComponent<CapsuleCollider2D>().size = new Vector2(0.8f, 0.8f);

        // Add scripts
        player.AddComponent<PlayerController>();
        player.AddComponent<PlayerInputHandler>();

        // Position
        player.transform.position = Vector3.zero;

        // Select it
        Selection.activeGameObject = player;

        Debug.Log("Player created! Now set up Input Action References in Inspector.");
        Debug.Log("1. Create Input Actions asset if not exists");
        Debug.Log("2. Drag PlayerControls to Move/Dash action refs");
    }
}
#endif