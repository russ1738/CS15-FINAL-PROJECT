using UnityEngine;

/// <summary>
/// Builds the entire playable level when the scene starts.
/// </summary>
public class GameBootstrap : MonoBehaviour
{
    void Awake()
    {
        Physics2D.gravity = new Vector2(0f, -30f);

        var manager = new GameObject("GameManager");
        manager.AddComponent<GameManager>();

        Camera cam = Camera.main;
        if (cam == null)
        {
            var camObject = new GameObject("Main Camera");
            cam = camObject.AddComponent<Camera>();
            camObject.tag = "MainCamera";
            camObject.AddComponent<AudioListener>();
        }

        cam.orthographic = true;
        cam.orthographicSize = 6f;
        cam.backgroundColor = new Color(0.42f, 0.78f, 1f);
        cam.transform.position = new Vector3(0f, 2f, -10f);
        cam.clearFlags = CameraClearFlags.SolidColor;

        var follow = cam.gameObject.GetComponent<CameraFollow>();
        if (follow == null)
        {
            follow = cam.gameObject.AddComponent<CameraFollow>();
        }

        BuildLevel(1, -1);
    }

    public void AdvanceToLevel(int level, int startingHealth)
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        BuildLevel(level, startingHealth);
    }

    void BuildLevel(int level, int startingHealth)
    {
        Camera cam = Camera.main;
        BuildBackdrop();

        if (level == 1)
        {
            BuildGround();
            GameObject bridge = BuildBridge();
            BuildSwitch(bridge);
            BuildNpc();
            BuildPickups();
            BuildMovingPlatform();
            BuildDoor();
        }
        else if (level == 2)
        {
            BuildLevelTwo();
        }
        else
        {
            BuildLevelThree();
        }

        GameObject player = BuildPlayer(level == 1 ? -11f : -10f, startingHealth);
        CameraFollow follow = cam.GetComponent<CameraFollow>();
        follow.target = player.transform;
        follow.minX = -14f;
        follow.maxX = level == 1 ? 48f : level == 2 ? 52f : 49f;
    }

    void BuildLevelTwo()
    {
        CreateBlock("Level2Start", new Vector3(-5f, -1.2f, 0f), new Vector2(14f, 1.6f), new Color(0.28f, 0.32f, 0.42f), true);
        CreateBlock("Level2Middle", new Vector3(12f, -1.2f, 0f), new Vector2(13f, 1.6f), new Color(0.28f, 0.32f, 0.42f), true);
        CreateBlock("Level2Exit", new Vector3(35.5f, -1.2f, 0f), new Vector2(25f, 1.6f), new Color(0.28f, 0.32f, 0.42f), true);
        CreateBlock("Level2LedgeA", new Vector3(2f, 1.4f, 0f), new Vector2(4f, 0.45f), new Color(0.4f, 0.55f, 0.7f), true);
        CreateBlock("Level2LedgeB", new Vector3(18f, 2.2f, 0f), new Vector2(4f, 0.45f), new Color(0.4f, 0.55f, 0.7f), true);

        CreateHazard("SpikeTrap2", new Vector3(7f, -0.25f, 0f), new Vector2(2f, 0.35f));
        CreateHazard("SpikeTrap3", new Vector3(30.5f, -0.25f, 0f), new Vector2(2f, 0.35f));
        CreatePickup("Level2Coin1", new Vector3(2f, 1.95f, 0f), PickupKind.Coin);
        CreatePickup("Level2Coin2", new Vector3(18f, 2.75f, 0f), PickupKind.Coin);
        CreatePickup("Level2UsbKey", new Vector3(27f, 0.2f, 0f), PickupKind.UsbKey);
        BuildMovingPlatformAt(new Vector3(12f, 0.4f, 0f), new Vector3(12f, 0.4f, 0f), new Vector3(12f, 2.1f, 0f));
        BuildDoorAt(new Vector3(46f, 0.7f, 0f));
        BuildShopAt(new Vector3(-8f, 0.35f, 0f));
    }

    void BuildLevelThree()
    {
        CreateBlock("BossArenaFloor", new Vector3(16f, -1.2f, 0f), new Vector2(62f, 1.6f), new Color(0.28f, 0.32f, 0.42f), true);
        CreateHazard("SpikeTrap4", new Vector3(8.5f, -0.25f, 0f), new Vector2(2f, 0.35f));
        CreateBlock("BossArenaLedgeA", new Vector3(2f, 1.5f, 0f), new Vector2(4f, 0.45f), new Color(0.4f, 0.55f, 0.7f), true);
        CreateBlock("BossArenaLedgeB", new Vector3(20f, 2.5f, 0f), new Vector2(4f, 0.45f), new Color(0.4f, 0.55f, 0.7f), true);
        CreatePickup("Level3Coin1", new Vector3(2f, 2.05f, 0f), PickupKind.Coin);
        CreatePickup("Level3Coin2", new Vector3(20f, 3.05f, 0f), PickupKind.Coin);

        GameObject boss = CreateBlock("FinalBoss", new Vector3(20f, 0.35f, 0f), new Vector2(1.8f, 2f), new Color(0.8f, 0.15f, 0.24f), true);
        boss.AddComponent<BossSpriteAnimator>();
        BossController controller = boss.AddComponent<BossController>();
        controller.leftX = 12f;
        controller.rightX = 34f;
        controller.SetMaxHp(50);
        BuildShopAt(new Vector3(-7f, 0.35f, 0f));
    }

    void CreateHazard(string name, Vector3 position, Vector2 size)
    {
        GameObject hazard = new GameObject(name);
        hazard.transform.position = position;
        hazard.transform.localScale = new Vector3(size.x, size.y * 8f, 1f);
        SpriteRenderer renderer = hazard.AddComponent<SpriteRenderer>();
        renderer.sprite = SpriteFactory.Spikes();
        renderer.sortingOrder = 2;
        BoxCollider2D collider = hazard.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(1f, 1f / 8f);
        collider.isTrigger = true;
        hazard.transform.SetParent(transform, true);
        hazard.AddComponent<Hazard>();
    }

    void BuildMovingPlatformAt(Vector3 position, Vector3 pointA, Vector3 pointB)
    {
        GameObject platform = CreateBlock("Lift", position, new Vector2(2.2f, 0.35f), new Color(0.55f, 0.6f, 0.8f), true);
        MovingPlatform move = platform.AddComponent<MovingPlatform>();
        move.pointA = pointA;
        move.pointB = pointB;
        move.speed = 1.6f;
    }

    void BuildDoorAt(Vector3 position)
    {
        GameObject door = CreateBlock("ServerDoor", position, new Vector2(1.2f, 2.2f), new Color(0.95f, 0.55f, 0.2f), true);
        door.AddComponent<LockedDoor>();
        door.GetComponent<BoxCollider2D>().isTrigger = true;
    }

    void BuildBackdrop()
    {
        CreateBackgroundLayer("DungeonWall", "Backgrounds/Dungeon_Wall", 5f, -100);
        CreateBlock("DungeonLeftWall", new Vector3(-20f, 4f, 1f), new Vector2(2f, 14f), new Color(0.18f, 0.2f, 0.28f), true);
        CreateBlock("DungeonFarWall", new Vector3(54f, 4f, 1f), new Vector2(2f, 14f), new Color(0.18f, 0.2f, 0.28f), true);
    }

    void CreateBackgroundLayer(string objectName, string resourcePath, float z, int sortingOrder)
    {
        Texture2D texture = Resources.Load<Texture2D>(resourcePath);
        if (texture == null)
        {
            Debug.LogWarning("Background image not found at Resources/" + resourcePath + ".png");
            return;
        }

        texture.filterMode = FilterMode.Point;
        GameObject background = new GameObject(objectName);
        background.transform.position = new Vector3(15f, 4f, z);
        background.transform.localScale = new Vector3(1.65f, 1.4f, 1f);
        SpriteRenderer renderer = background.AddComponent<SpriteRenderer>();
        renderer.sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 8f);
        renderer.sortingOrder = sortingOrder;
        background.transform.SetParent(transform, true);
    }

    void BuildGround()
    {
        CreateBlock("StartFloor", new Vector3(-4f, -1.2f, 0f), new Vector2(20f, 1.6f), new Color(0.28f, 0.32f, 0.42f), true);
        CreateBlock("Step", new Vector3(3.5f, 0.4f, 0f), new Vector2(3f, 0.5f), new Color(0.35f, 0.38f, 0.5f), true);
        CreateBlock("MidFloor", new Vector3(18f, -1.2f, 0f), new Vector2(10f, 1.6f), new Color(0.28f, 0.32f, 0.42f), true);
        CreateBlock("KeyLedge", new Vector3(24.5f, 2.6f, 0f), new Vector2(4f, 0.5f), new Color(0.4f, 0.55f, 0.7f), true);
        CreateBlock("ExitFloor", new Vector3(35.5f, -1.2f, 0f), new Vector2(19f, 1.6f), new Color(0.28f, 0.32f, 0.42f), true);
        CreateBlock("HighLedge", new Vector3(14.5f, 2.2f, 0f), new Vector2(2.4f, 0.4f), new Color(0.4f, 0.55f, 0.7f), true);
        CreateHazard("SpikeTrap1", new Vector3(32.5f, -0.25f, 0f), new Vector2(2f, 0.35f));
    }

    GameObject BuildBridge()
    {
        GameObject bridge = CreateBlock("Bridge", new Vector3(9.5f, -0.6f, 0f), new Vector2(7.6f, 0.45f), new Color(0.55f, 0.75f, 0.45f), true);
        bridge.SetActive(false);
        return bridge;
    }

    void BuildSwitch(GameObject bridge)
    {
        GameObject sw = CreateBlock("PowerSwitch", new Vector3(4.6f, 1.15f, 0f), new Vector2(0.5f, 0.9f), new Color(0.85f, 0.25f, 0.25f), true);
        var power = sw.AddComponent<PowerSwitch>();
        power.bridge = bridge;
    }

    void BuildNpc()
    {
        GameObject npc = CreateBlock("Professor", new Vector3(-8.5f, 0.35f, 0f), new Vector2(0.8f, 1.5f), new Color(0.75f, 0.45f, 0.85f), true);
        var dialogue = npc.AddComponent<DialogueNpc>();
        dialogue.speakerName = "Professor Ada";
        dialogue.lines = new[]
        {
            "Welcome to Legacy Dungeon, adventurer.",
            "The passage ahead is sealed.",
            "Flip the red switch, collect the key, and open the exit."
        };
        npc.GetComponent<BoxCollider2D>().isTrigger = true;

        BuildShopAt(new Vector3(-5f, 0.35f, 0f));
    }

    void BuildShopAt(Vector3 position)
    {
        GameObject shop = CreateBlock("CoinShopkeeper", position, new Vector2(0.9f, 1.45f), new Color(0.82f, 0.62f, 0.18f), true);
        shop.GetComponent<BoxCollider2D>().isTrigger = true;
        shop.AddComponent<Shopkeeper>();
    }

    void BuildPickups()
    {
        CreatePickup("Coin1", new Vector3(-2f, 0.4f, 0f), PickupKind.Coin);
        CreatePickup("Coin2", new Vector3(0.8f, 0.4f, 0f), PickupKind.Coin);
        CreatePickup("Coin3", new Vector3(14.5f, 2.85f, 0f), PickupKind.Coin);
        CreatePickup("Coin4", new Vector3(18.5f, 0.4f, 0f), PickupKind.Coin);
        CreatePickup("Coin5", new Vector3(21.5f, 0.4f, 0f), PickupKind.Coin);
        CreatePickup("UsbKey", new Vector3(24.5f, 3.35f, 0f), PickupKind.UsbKey);
    }

    void BuildMovingPlatform()
    {
        GameObject platform = CreateBlock("Lift", new Vector3(21.5f, 0.8f, 0f), new Vector2(2.2f, 0.35f), new Color(0.55f, 0.6f, 0.8f), true);
        var move = platform.AddComponent<MovingPlatform>();
        move.pointA = new Vector3(21.5f, 0.5f, 0f);
        move.pointB = new Vector3(21.5f, 2.4f, 0f);
        move.speed = 1.6f;
    }

    void BuildDoor()
    {
        GameObject door = CreateBlock("ServerDoor", new Vector3(43.5f, 0.7f, 0f), new Vector2(1.2f, 2.2f), new Color(0.95f, 0.55f, 0.2f), true);
        door.AddComponent<LockedDoor>();
        door.GetComponent<BoxCollider2D>().isTrigger = true;
    }

    GameObject BuildPlayer(float startX, int startingHealth)
    {
        GameObject player = CreateBlock("Player", new Vector3(startX, 0.4f, 0f), new Vector2(0.7f, 1.2f), new Color(0.35f, 0.75f, 1f), true);
        player.transform.localScale = Vector3.one;
        player.GetComponent<BoxCollider2D>().size = new Vector2(0.7f, 1.2f);
        player.tag = "Player";

        var body = player.AddComponent<Rigidbody2D>();
        body.freezeRotation = true;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.gravityScale = 1f;
        body.constraints = RigidbodyConstraints2D.FreezeRotation;

        PlayerController controller = player.AddComponent<PlayerController>();
        if (startingHealth >= 0)
        {
            controller.SetHealth(startingHealth);
        }
        player.AddComponent<ArcherSpriteAnimator>();
        return player;
    }

    void CreatePickup(string name, Vector3 position, PickupKind kind)
    {
        Color color = kind == PickupKind.UsbKey
            ? new Color(0.2f, 0.95f, 0.95f)
            : new Color(0.98f, 0.85f, 0.2f);
        Vector2 size = kind == PickupKind.UsbKey ? new Vector2(0.55f, 0.35f) : new Vector2(0.4f, 0.4f);

        GameObject item = CreateBlock(name, position, size, color, true);
        if (kind == PickupKind.Coin)
        {
            item.GetComponent<SpriteRenderer>().sprite = SpriteFactory.GoldCoin();
        }
        item.GetComponent<BoxCollider2D>().isTrigger = true;
        var pickup = item.AddComponent<PickupItem>();
        pickup.kind = kind;
    }

    GameObject CreateBlock(string name, Vector3 position, Vector2 size, Color color, bool collide)
    {
        var go = new GameObject(name);
        go.transform.position = position;
        SpriteFactory.AddSprite(go, color, size);

        if (collide)
        {
            var box = go.AddComponent<BoxCollider2D>();
            box.size = Vector2.one;
        }

        go.transform.SetParent(transform, true);

        return go;
    }
}
