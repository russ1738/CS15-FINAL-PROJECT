using UnityEngine;

/// <summary>
/// Builds a 1x1 colored sprite so the game does not need imported art.
/// </summary>
public static class SpriteFactory
{
    static Sprite goldCoin;
    static Sprite arrow;
    static Sprite spikes;

    public static Sprite Solid(Color color)
    {
        var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        texture.SetPixel(0, 0, color);
        texture.filterMode = FilterMode.Point;
        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
    }

    public static SpriteRenderer AddSprite(GameObject go, Color color, Vector2 size)
    {
        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = Solid(color);
        renderer.drawMode = SpriteDrawMode.Simple;
        go.transform.localScale = new Vector3(size.x, size.y, 1f);
        return renderer;
    }

    public static Sprite GoldCoin()
    {
        if (goldCoin != null)
        {
            return goldCoin;
        }

        const int size = 32;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        Color rim = new Color(0.72f, 0.38f, 0.04f, 1f);
        Color gold = new Color(1f, 0.72f, 0.12f, 1f);
        Color shine = new Color(1f, 0.93f, 0.48f, 1f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - 15.5f;
                float dy = y - 15.5f;
                float distance = Mathf.Sqrt(dx * dx + dy * dy);
                Color pixel = distance > 14.5f
                    ? Color.clear
                    : distance > 11.5f ? rim : gold;

                if (distance <= 10f && x >= 11 && x <= 14 && y >= 19 && y <= 24)
                {
                    pixel = shine;
                }

                texture.SetPixel(x, y, pixel);
            }
        }

        texture.Apply();
        goldCoin = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
        return goldCoin;
    }

    public static Sprite Arrow()
    {
        if (arrow != null)
        {
            return arrow;
        }

        const int width = 24;
        const int height = 8;
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        Color shaft = new Color(0.88f, 0.72f, 0.42f, 1f);
        Color head = new Color(0.7f, 0.78f, 0.82f, 1f);
        Color feather = new Color(0.35f, 0.78f, 0.45f, 1f);
        for (int x = 0; x < width; x++)
        {
            texture.SetPixel(x, 3, shaft);
            texture.SetPixel(x, 4, shaft);
        }

        for (int i = 0; i < 4; i++)
        {
            texture.SetPixel(width - 1 - i, 3 + i, head);
            texture.SetPixel(width - 1 - i, 4 - i, head);
            texture.SetPixel(i, 2 - i / 2, feather);
            texture.SetPixel(i, 5 + i / 2, feather);
        }

        texture.Apply();
        arrow = Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), width);
        return arrow;
    }

    public static Sprite Spikes()
    {
        if (spikes != null)
        {
            return spikes;
        }

        Texture2D texture = Resources.Load<Texture2D>("DungeonTileSet");
        if (texture == null)
        {
            Debug.LogWarning("Dungeon trap sheet not found at Resources/DungeonTileSet.png");
            return null;
        }

        texture.filterMode = FilterMode.Point;
        // This is the long spike strip at the bottom of the supplied tile sheet.
        spikes = Sprite.Create(texture, new Rect(16f, 46f, 96f, 12f), new Vector2(0.5f, 0.5f), 96f, 0, SpriteMeshType.FullRect);
        return spikes;
    }
}
