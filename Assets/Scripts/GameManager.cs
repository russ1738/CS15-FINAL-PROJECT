using UnityEngine;

public enum GameState
{
    Menu,
    Playing,
    Talking,
    Shopping,
    Won,
    Lost
}

/// <summary>
/// Tracks score, inventory, dialogue, and on-screen HUD.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState State { get; private set; } = GameState.Menu;
    public int Coins { get; private set; }
    public bool HasUsbKey { get; private set; }
    public bool BridgeBuilt { get; private set; }
    public int BossHp { get; private set; }
    public int BossMaxHp { get; private set; }
    public int CurrentLevel { get; private set; } = 1;
    public int BowDamageBonus { get; private set; }

    GameBootstrap bootstrap;

    string prompt = "";
    string dialogueTitle = "";
    string dialogueBody = "";
    string[] dialogueLines;
    int dialogueIndex;
    string shopMessage = "Choose an upgrade.";

    static bool uiStylesReady;
    static GUIStyle panelStyle;
    static GUIStyle cardStyle;
    static GUIStyle buttonStyle;
    static GUIStyle largeButtonStyle;
    static GUIStyle titleStyle;
    static GUIStyle headerStyle;
    static GUIStyle bodyStyle;
    static GUIStyle smallStyle;
    static GUIStyle badgeStyle;
    static GUIStyle meterStyle;
    static Texture2D panelTexture;
    static Texture2D cardTexture;
    static Texture2D buttonTexture;
    static Texture2D buttonHoverTexture;
    static Texture2D buttonActiveTexture;
    static Texture2D meterTexture;

    void Awake()
    {
        Instance = this;
        bootstrap = FindObjectOfType<GameBootstrap>();
    }

    public void SetPrompt(string text)
    {
        prompt = text ?? "";
    }

    public void StartGame()
    {
        State = GameState.Playing;
    }

    public void AddCoin()
    {
        Coins++;
    }

    public bool SpendCoins(int amount)
    {
        if (amount <= 0 || Coins < amount)
        {
            return false;
        }

        Coins -= amount;
        return true;
    }

    public void UpgradeBow(PlayerController player, int amount)
    {
        if (player == null || amount <= 0)
        {
            return;
        }

        BowDamageBonus += amount;
        player.UpgradeArrowDamage(amount);
    }

    public void OpenShop()
    {
        State = GameState.Shopping;
        shopMessage = "Choose an upgrade.";
        prompt = "";
    }

    public void CloseShop()
    {
        if (State == GameState.Shopping)
        {
            State = GameState.Playing;
        }
    }

    public void GiveUsbKey()
    {
        HasUsbKey = true;
    }

    public void MarkBridgeBuilt()
    {
        BridgeBuilt = true;
    }

    public void NotifyBossHp(int hp, int maxHp)
    {
        BossHp = Mathf.Max(0, hp);
        BossMaxHp = Mathf.Max(1, maxHp);
    }

    public void AdvanceLevel()
    {
        if (CurrentLevel >= 3)
        {
            Win();
            return;
        }

        CurrentLevel++;
        HasUsbKey = false;
        BridgeBuilt = false;
        BossHp = 0;
        BossMaxHp = 0;
        State = GameState.Playing;
        prompt = "";
        if (bootstrap != null)
        {
            PlayerController player = FindObjectOfType<PlayerController>();
            int remainingHealth = player != null ? player.Health : 5;
            bootstrap.AdvanceToLevel(CurrentLevel, remainingHealth);
        }
    }

    public void StartDialogue(string speaker, string[] lines)
    {
        if (lines == null || lines.Length == 0)
        {
            return;
        }

        State = GameState.Talking;
        dialogueTitle = speaker;
        dialogueLines = lines;
        dialogueIndex = 0;
        dialogueBody = lines[0];
    }

    public void Win()
    {
        State = GameState.Won;
        prompt = "";
    }

    public void Lose()
    {
        State = GameState.Lost;
        prompt = "";
    }

    void Update()
    {
        if (State == GameState.Shopping && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseShop();
        }

        if (State == GameState.Talking && (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)))
        {
            AdvanceDialogue();
        }

        if ((State == GameState.Won || State == GameState.Lost) && Input.GetKeyDown(KeyCode.R))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }
    }

    void AdvanceDialogue()
    {
        dialogueIndex++;
        if (dialogueIndex >= dialogueLines.Length)
        {
            State = GameState.Playing;
            dialogueBody = "";
            dialogueTitle = "";
            return;
        }

        dialogueBody = dialogueLines[dialogueIndex];
    }

    void OnGUI()
    {
        EnsureUiStyles();

        if (State == GameState.Menu)
        {
            DrawMainMenu();
            return;
        }

        PlayerController player = null;
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.GetComponent<PlayerController>();
        }

        DrawHud(player);

        if (!string.IsNullOrEmpty(prompt) && State == GameState.Playing)
        {
            Rect promptRect = new Rect(Screen.width / 2f - 210f, Screen.height - 76f, 420f, 48f);
            GUI.Box(promptRect, GUIContent.none, cardStyle);
            GUI.Label(promptRect, prompt, headerStyle);
        }

        if (State == GameState.Talking)
        {
            DrawDialogue();
        }

        if (State == GameState.Won)
        {
            DrawResult(true);
        }

        if (State == GameState.Lost)
        {
            DrawResult(false);
        }

        if (BossMaxHp > 0 && State == GameState.Playing)
        {
            DrawBossHealth();
        }

        if (State == GameState.Shopping)
        {
            DrawShop(player);
        }
    }

    void DrawShop(PlayerController player)
    {
        float width = Mathf.Min(560f, Screen.width - 32f);
        float height = Mathf.Min(410f, Screen.height - 32f);
        Rect panel = new Rect((Screen.width - width) / 2f, (Screen.height - height) / 2f, width, height);
        GUI.Box(panel, GUIContent.none, panelStyle);
        GUI.Label(new Rect(panel.x + 24f, panel.y + 20f, panel.width - 48f, 36f), "DUNGEON SUPPLIES", titleStyle);
        GUI.Label(new Rect(panel.x + 26f, panel.y + 62f, panel.width - 52f, 26f), "Coins   " + Coins, badgeStyle);

        float cardWidth = panel.width - 48f;
        Rect healthCard = new Rect(panel.x + 24f, panel.y + 100f, cardWidth, 96f);
        GUI.Box(healthCard, GUIContent.none, cardStyle);
        GUI.Label(new Rect(healthCard.x + 16f, healthCard.y + 12f, cardWidth - 190f, 26f), "HEALTH POTION", headerStyle);
        GUI.Label(new Rect(healthCard.x + 16f, healthCard.y + 43f, cardWidth - 190f, 34f), "Restores 2 health", bodyStyle);
        if (GUI.Button(new Rect(healthCard.xMax - 164f, healthCard.y + 26f, 146f, 44f), "BUY   3 COINS", buttonStyle))
        {
            if (player == null)
            {
                shopMessage = "Come back when the archer is here.";
            }
            else if (player.Health >= player.MaxHealth)
            {
                shopMessage = "Your health is already full.";
            }
            else if (!SpendCoins(3))
            {
                shopMessage = "You need 3 coins for that.";
            }
            else
            {
                player.Heal(2);
                shopMessage = "Health restored.";
            }
        }

        Rect bowCard = new Rect(panel.x + 24f, panel.y + 208f, cardWidth, 96f);
        GUI.Box(bowCard, GUIContent.none, cardStyle);
        GUI.Label(new Rect(bowCard.x + 16f, bowCard.y + 12f, cardWidth - 190f, 26f), "BOW UPGRADE", headerStyle);
        GUI.Label(new Rect(bowCard.x + 16f, bowCard.y + 43f, cardWidth - 190f, 34f), "+2 arrow damage", bodyStyle);
        if (GUI.Button(new Rect(bowCard.xMax - 164f, bowCard.y + 26f, 146f, 44f), "BUY   6 COINS", buttonStyle))
        {
            if (player == null)
            {
                shopMessage = "Come back when the archer is here.";
            }
            else if (!SpendCoins(6))
            {
                shopMessage = "You need 6 coins for that.";
            }
            else
            {
                UpgradeBow(player, 2);
                shopMessage = "Bow upgraded. Arrows deal more damage.";
            }
        }

        GUI.Label(new Rect(panel.x + 24f, panel.y + 316f, panel.width - 48f, 36f), shopMessage, bodyStyle);
        GUI.Label(new Rect(panel.x + 24f, panel.y + height - 34f, panel.width - 48f, 22f), "ESC  CLOSE", smallStyle);
    }

    void DrawMainMenu()
    {
        float panelWidth = Mathf.Min(560f, Screen.width - 32f);
        float panelHeight = Mathf.Min(390f, Screen.height - 32f);
        Rect panel = new Rect((Screen.width - panelWidth) / 2f, (Screen.height - panelHeight) / 2f, panelWidth, panelHeight);
        GUI.Box(panel, GUIContent.none, panelStyle);
        GUI.Label(new Rect(panel.x + 24f, panel.y + 46f, panel.width - 48f, 54f), "LEGACY DUNGEON", titleStyle);
        GUI.Label(new Rect(panel.x + 34f, panel.y + 108f, panel.width - 68f, 42f), "DESCEND  /  SURVIVE  /  ESCAPE", badgeStyle);
        GUI.Label(new Rect(panel.x + 34f, panel.y + 151f, panel.width - 68f, 34f), "Explore three levels and defeat the final boss.", bodyStyle);

        float buttonWidth = Mathf.Min(280f, panel.width - 48f);
        float buttonX = panel.x + (panel.width - buttonWidth) / 2f;
        if (GUI.Button(new Rect(buttonX, panel.y + 210f, buttonWidth, 52f), "ENTER THE DUNGEON", largeButtonStyle))
        {
            StartGame();
        }

        if (GUI.Button(new Rect(buttonX, panel.y + 275f, buttonWidth, 42f), "QUIT", buttonStyle))
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }

    void DrawHud(PlayerController player)
    {
        Rect panel = new Rect(18f, 18f, 320f, 148f);
        GUI.Box(panel, GUIContent.none, panelStyle);
        GUI.Label(new Rect(panel.x + 16f, panel.y + 12f, 194f, 28f), "LEGACY DUNGEON", headerStyle);
        GUI.Box(new Rect(panel.xMax - 105f, panel.y + 12f, 90f, 27f), "LEVEL " + CurrentLevel + " / 3", badgeStyle);
        GUI.Box(new Rect(panel.x + 14f, panel.y + 45f, panel.width - 28f, 1f), GUIContent.none, meterStyle);

        GUI.Label(new Rect(panel.x + 16f, panel.y + 51f, 132f, 24f), "COINS   " + Coins, bodyStyle);
        GUI.Label(new Rect(panel.x + 158f, panel.y + 51f, 146f, 24f), "USB KEY   " + (HasUsbKey ? "YES" : "NO"), bodyStyle);

        int health = player != null ? player.Health : 0;
        int maxHealth = player != null ? player.MaxHealth : 1;
        GUI.Label(new Rect(panel.x + 16f, panel.y + 78f, panel.width - 32f, 21f),
            "VITALITY   " + health + " / " + maxHealth, smallStyle);
        DrawMeter(new Rect(panel.x + 16f, panel.y + 101f, panel.width - 32f, 11f), health, maxHealth, new Color(0.68f, 0.2f, 0.18f));

        string controls = CurrentLevel == 3
            ? "A / D MOVE     SPACE JUMP     F SHOOT     E USE"
            : "A / D MOVE     SPACE JUMP     E USE";
        GUI.Label(new Rect(panel.x + 12f, panel.y + 119f, panel.width - 24f, 22f), controls, smallStyle);
    }

    void DrawBossHealth()
    {
        Rect panel = new Rect(Screen.width - 278f, 18f, 260f, 76f);
        GUI.Box(panel, GUIContent.none, panelStyle);
        GUI.Label(new Rect(panel.x + 14f, panel.y + 10f, panel.width - 28f, 24f), "THE EXECUTIONER", headerStyle);
        GUI.Label(new Rect(panel.x + 14f, panel.y + 37f, panel.width - 28f, 18f), BossHp + " / " + BossMaxHp, smallStyle);
        DrawMeter(new Rect(panel.x + 14f, panel.y + 57f, panel.width - 28f, 9f), BossHp, BossMaxHp, new Color(0.72f, 0.18f, 0.16f));
    }

    void DrawDialogue()
    {
        float width = Mathf.Min(700f, Screen.width - 32f);
        Rect panel = new Rect((Screen.width - width) / 2f, Screen.height - 184f, width, 150f);
        GUI.Box(panel, GUIContent.none, panelStyle);
        GUI.Label(new Rect(panel.x + 22f, panel.y + 16f, panel.width - 44f, 30f), dialogueTitle, headerStyle);
        GUI.Label(new Rect(panel.x + 28f, panel.y + 52f, panel.width - 56f, 52f), dialogueBody, bodyStyle);
        GUI.Label(new Rect(panel.x + 22f, panel.y + 112f, panel.width - 44f, 24f), "E / SPACE / CLICK  TO CONTINUE", smallStyle);
    }

    void DrawResult(bool won)
    {
        float width = Mathf.Min(500f, Screen.width - 32f);
        Rect panel = new Rect((Screen.width - width) / 2f, (Screen.height - 250f) / 2f, width, 250f);
        GUI.Box(panel, GUIContent.none, panelStyle);
        GUI.Label(new Rect(panel.x + 24f, panel.y + 38f, panel.width - 48f, 48f),
            won ? "DUNGEON CLEARED" : "YOU WERE DEFEATED", titleStyle);
        GUI.Label(new Rect(panel.x + 28f, panel.y + 102f, panel.width - 56f, 38f),
            won ? "Coins collected   " + Coins : "The dungeon awaits another attempt.", bodyStyle);
        GUI.Label(new Rect(panel.x + 28f, panel.y + 154f, panel.width - 56f, 34f), "PRESS R TO PLAY AGAIN", badgeStyle);
    }

    void DrawMeter(Rect rect, int value, int maxValue, Color fillColor)
    {
        GUI.Box(rect, GUIContent.none, meterStyle);
        float ratio = maxValue > 0 ? Mathf.Clamp01((float)value / maxValue) : 0f;
        if (ratio <= 0f)
        {
            return;
        }

        Color previousColor = GUI.color;
        GUI.color = fillColor;
        GUI.DrawTexture(new Rect(rect.x + 2f, rect.y + 2f, Mathf.Max(0f, (rect.width - 4f) * ratio), rect.height - 4f), Texture2D.whiteTexture);
        GUI.color = previousColor;
    }

    static void EnsureUiStyles()
    {
        if (uiStylesReady)
        {
            return;
        }

        Color ink = new Color(0.055f, 0.055f, 0.085f, 0.97f);
        Color stone = new Color(0.24f, 0.27f, 0.35f, 1f);
        Color gold = new Color(0.72f, 0.53f, 0.24f, 1f);
        panelTexture = CreateFrameTexture(ink, stone, gold);
        cardTexture = CreateFrameTexture(new Color(0.09f, 0.095f, 0.14f, 0.98f), stone, new Color(0.38f, 0.33f, 0.24f, 1f));
        buttonTexture = CreateFrameTexture(new Color(0.31f, 0.23f, 0.12f, 1f), gold, new Color(0.92f, 0.74f, 0.4f, 1f));
        buttonHoverTexture = CreateFrameTexture(new Color(0.42f, 0.31f, 0.16f, 1f), new Color(0.92f, 0.74f, 0.4f), new Color(1f, 0.88f, 0.58f));
        buttonActiveTexture = CreateFrameTexture(new Color(0.2f, 0.16f, 0.12f, 1f), new Color(0.84f, 0.62f, 0.3f), gold);
        meterTexture = CreateFrameTexture(new Color(0.025f, 0.025f, 0.04f, 1f), new Color(0.3f, 0.32f, 0.39f, 1f), new Color(0.4f, 0.42f, 0.48f, 1f));

        panelStyle = new GUIStyle(GUI.skin.box)
        {
            normal = { background = panelTexture },
            border = new RectOffset(5, 5, 5, 5),
            padding = new RectOffset(12, 12, 10, 10)
        };
        cardStyle = new GUIStyle(panelStyle)
        {
            normal = { background = cardTexture },
            border = new RectOffset(4, 4, 4, 4),
            padding = new RectOffset(10, 10, 8, 8)
        };
        buttonStyle = new GUIStyle(GUI.skin.button)
        {
            normal = { background = buttonTexture, textColor = new Color(1f, 0.91f, 0.7f) },
            hover = { background = buttonHoverTexture, textColor = Color.white },
            active = { background = buttonActiveTexture, textColor = Color.white },
            focused = { background = buttonHoverTexture, textColor = Color.white },
            border = new RectOffset(5, 5, 5, 5),
            fontSize = 14,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        largeButtonStyle = new GUIStyle(buttonStyle) { fontSize = 18 };
        titleStyle = MakeLabelStyle(26, FontStyle.Bold, new Color(0.95f, 0.78f, 0.44f), TextAnchor.MiddleCenter);
        headerStyle = MakeLabelStyle(16, FontStyle.Bold, new Color(0.92f, 0.78f, 0.52f), TextAnchor.MiddleLeft);
        bodyStyle = MakeLabelStyle(14, FontStyle.Normal, new Color(0.84f, 0.86f, 0.9f), TextAnchor.MiddleCenter);
        smallStyle = MakeLabelStyle(11, FontStyle.Bold, new Color(0.67f, 0.7f, 0.77f), TextAnchor.MiddleCenter);
        badgeStyle = MakeLabelStyle(13, FontStyle.Bold, new Color(0.94f, 0.79f, 0.49f), TextAnchor.MiddleCenter);
        meterStyle = new GUIStyle(GUI.skin.box)
        {
            normal = { background = meterTexture },
            border = new RectOffset(2, 2, 2, 2),
            padding = new RectOffset(0, 0, 0, 0)
        };
        uiStylesReady = true;
    }

    static GUIStyle MakeLabelStyle(int size, FontStyle weight, Color color, TextAnchor alignment)
    {
        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            fontSize = size,
            fontStyle = weight,
            alignment = alignment,
            wordWrap = true
        };
        style.normal.textColor = color;
        return style;
    }

    static Texture2D CreateFrameTexture(Color fill, Color edge, Color accent)
    {
        const int size = 12;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.HideAndDontSave
        };
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool outer = x == 0 || y == 0 || x == size - 1 || y == size - 1;
                bool inner = x == 1 || y == 1 || x == size - 2 || y == size - 2;
                bool corner = (x <= 1 || x >= size - 2) && (y <= 1 || y >= size - 2);
                texture.SetPixel(x, y, outer ? edge : corner ? accent : inner ? edge : fill);
            }
        }
        texture.Apply();
        return texture;
    }
}
