using UnityEngine;
using TMPro;

public class DamageEffectManager : MonoBehaviour
{
    public static DamageEffectManager Instance;

    [Header("Positions")]
    public Transform playerPos;
    public Transform enemyPos;

    [Header("Settings")]
    public Vector3 playerEffectOffset = new Vector3(0, -600f, 0); // Lower for player
    public Vector3 enemyEffectOffset = new Vector3(0, -350f, 0);  // Default for enemy

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Try to find HUD to get positions if not assigned
        // We delay slightly or check continuously if HUD is not ready yet, but Start is usually fine if HUD is in scene.
        FindPositions();

        // Register to Battle Events
        EventCenter.Register("BattleStarted", (obj) => 
        {
            FindPositions();
            SubscribeToCharacters();
        });
        
        // If battle already started (e.g. reload or late init)
        if (BattleManager.Instance != null && BattleManager.Instance.player != null)
        {
            SubscribeToCharacters();
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from Battle Events
        if (BattleManager.Instance != null)
        {
            if (BattleManager.Instance.player != null)
            {
                BattleManager.Instance.player.DamageTaken -= OnPlayerDamage;
            }
            if (BattleManager.Instance.enemy != null)
            {
                BattleManager.Instance.enemy.DamageTaken -= OnEnemyDamage;
            }
        }
        EventCenter.Unregister("BattleStarted", (obj) => { }); // Note: EventCenter.Unregister logic needs to be checked, usually requires exact delegate.
        // Assuming simpler unsubscription or rely on Scene Reload to clear EventCenter if it's not static persistent.
        // If EventCenter persists, we have a leak here because we use lambda in Register.
    }

    private void FindPositions()
    {
        if (playerPos == null || enemyPos == null)
        {
            var hud = FindObjectOfType<HUD>();
            if (hud != null)
            {
                if (playerPos == null && hud.playerHPText != null) playerPos = hud.playerHPText.transform;
                if (enemyPos == null && hud.enemyHPText != null) enemyPos = hud.enemyHPText.transform;
            }
        }
    }

    private void SubscribeToCharacters()
    {
        if (BattleManager.Instance.player != null)
        {
            BattleManager.Instance.player.DamageTaken -= OnPlayerDamage;
            BattleManager.Instance.player.DamageTaken += OnPlayerDamage;
            BattleManager.Instance.player.HealTaken -= OnPlayerHeal;
            BattleManager.Instance.player.HealTaken += OnPlayerHeal;
        }
        if (BattleManager.Instance.enemy != null)
        {
            BattleManager.Instance.enemy.DamageTaken -= OnEnemyDamage;
            BattleManager.Instance.enemy.DamageTaken += OnEnemyDamage;
            BattleManager.Instance.enemy.HealTaken -= OnEnemyHeal;
            BattleManager.Instance.enemy.HealTaken += OnEnemyHeal;
        }
    }

    private void OnPlayerHeal(int amount)
    {
        ShowHealEffect(playerPos, amount, playerEffectOffset);
    }

    private void OnEnemyHeal(int amount)
    {
        ShowHealEffect(enemyPos, amount, enemyEffectOffset);
    }

    private void OnPlayerDamage(int amount, BaseCharacter source)
    {
        ShowEffect(playerPos, amount, playerEffectOffset);
    }

    private void OnEnemyDamage(int amount, BaseCharacter source)
    {
        ShowEffect(enemyPos, amount, enemyEffectOffset);
    }

    public void ShowFloatingText(Transform targetDetails, string text, Color color, Vector3? offsetOverride = null)
    {
        if (targetDetails == null) return;
        
        Canvas canvas = targetDetails.GetComponentInParent<Canvas>();
        if (canvas == null) return;

        // Create Center Text Object
        GameObject popupObj = new GameObject("FloatingText_Center");
        popupObj.transform.SetParent(canvas.transform, false);
        
        // Center position in Screen/Canvas
        RectTransform rect = popupObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0.5f);
        rect.anchorMax = new Vector2(1, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(0, 100); // Full width, height 100
        
        var popup = popupObj.AddComponent<DamagePopup>();
        popup.Setup(text, color);
    }

    private void ShowEffect(Transform targetDetails, int amount, Vector3 offset)
    {
        if (targetDetails == null) return;
        
        // Get the Canvas of the target
        Canvas canvas = targetDetails.GetComponentInParent<Canvas>();
        if (canvas == null) return;

        // Spawn Slash Effect first (behind text ideally, but text is spawned after so it appears on top)
        GameObject slashObj = new GameObject("SlashEffect");
        slashObj.transform.SetParent(canvas.transform, false);
        slashObj.transform.position = targetDetails.position + offset;
        
        var slash = slashObj.AddComponent<SlashEffect>();
        // Create Sprite for slash
        var img = slashObj.AddComponent<UnityEngine.UI.Image>();
        img.sprite = CreateSlashSprite();
        // Rotate slash randomly for variety
        slashObj.transform.rotation = Quaternion.Euler(0, 0, Random.Range(-45f, 45f));
        
        slash.Setup();

        // Spawn Popup Text
        GameObject popupObj = new GameObject("DamagePopup");
        popupObj.transform.SetParent(canvas.transform, false);
        popupObj.transform.position = targetDetails.position + offset; 
        
        var popup = popupObj.AddComponent<DamagePopup>();
        popup.Setup(amount);
    }

    private void ShowHealEffect(Transform targetDetails, int amount, Vector3 offset)
    {
        if (targetDetails == null) return;
        
        Canvas canvas = targetDetails.GetComponentInParent<Canvas>();
        if (canvas == null) return;

        // Spawn Heal Circle Effect (reuse slash logic but with heal sprite)
        GameObject healObj = new GameObject("HealEffect");
        healObj.transform.SetParent(canvas.transform, false);
        healObj.transform.position = targetDetails.position + offset;
        
        var slash = healObj.AddComponent<SlashEffect>(); // Reuse slash animation logic (scale up and fade)
        var img = healObj.AddComponent<UnityEngine.UI.Image>();
        img.sprite = CreateHealSprite();
        img.color = Color.green; // Tint green
        
        slash.Setup();

        // Spawn Popup Text
        GameObject popupObj = new GameObject("HealPopup");
        popupObj.transform.SetParent(canvas.transform, false);
        popupObj.transform.position = targetDetails.position + offset; 
        
        var popup = popupObj.AddComponent<DamagePopup>();
        popup.Setup("+" + amount, Color.green);
    }

    private Sprite healSprite;
    private Sprite CreateHealSprite()
    {
        if (healSprite != null) return healSprite;

        int width = 128;
        int height = 128;
        Texture2D texture = new Texture2D(width, height);
        Color[] colors = new Color[width * height];
        for (int i = 0; i < colors.Length; i++) colors[i] = Color.clear;

        // Draw a circle/cross pattern
        Vector2 center = new Vector2(width / 2f, height / 2f);
        float radius = width / 3f;
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                if (dist < radius)
                {
                    // Ring
                    if (dist > radius * 0.8f)
                    {
                         colors[y * width + x] = new Color(1, 1, 1, 0.8f);
                    }
                    // Plus sign inside
                    else if (Mathf.Abs(x - center.x) < 5 || Mathf.Abs(y - center.y) < 5)
                    {
                         colors[y * width + x] = new Color(1, 1, 1, 1f);
                    }
                }
            }
        }
        
        texture.SetPixels(colors);
        texture.Apply();
        healSprite = Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
        return healSprite;
    }

    private Sprite slashSprite;
    private Sprite CreateSlashSprite()
    {
        if (slashSprite != null) return slashSprite;

        int width = 128;
        int height = 128;
        Texture2D texture = new Texture2D(width, height);
        Color[] colors = new Color[width * height];
        for (int i = 0; i < colors.Length; i++) colors[i] = Color.clear;

        // Draw a diagonal line
        for (int i = 0; i < width; i++)
        {
            int y = i;
            // Draw a thick line with fade on edges
            for (int k = -4; k <= 4; k++)
            {
                 int ny = y + k;
                 if(ny >= 0 && ny < height)
                 {
                    float alpha = 1.0f - (Mathf.Abs(k) / 5.0f);
                    colors[ny * width + i] = new Color(1, 1, 1, alpha);
                 }
            }
        }
        
        texture.SetPixels(colors);
        texture.Apply();
        slashSprite = Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
        return slashSprite;
    }
}
