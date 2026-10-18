using UnityEngine;

/// <summary>
/// Plays the undead executioner's idle and scythe-swing sprite sheets.
/// </summary>
public class BossSpriteAnimator : MonoBehaviour
{
    public float pixelsPerUnit = 60f;
    public float attackFrameDuration = 0.065f;
    public float idleFrameDuration = 0.18f;

    SpriteRenderer spriteRenderer;
    Sprite[] idleFrames;
    Sprite[] attackFrames;
    int frameIndex;
    float frameTimer;
    bool attacking;

    public float AttackDuration => attackFrames != null ? attackFrames.Length * attackFrameDuration : 0.8f;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = Color.white;
        idleFrames = LoadIdleFrames();
        attackFrames = LoadAttackFrames();

        if (idleFrames.Length > 0)
        {
            spriteRenderer.sprite = idleFrames[0];
        }
    }

    void Update()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        if (attacking)
        {
            frameTimer += Time.deltaTime;
            if (frameTimer >= attackFrameDuration)
            {
                frameTimer -= attackFrameDuration;
                if (frameIndex < attackFrames.Length - 1)
                {
                    frameIndex++;
                    spriteRenderer.sprite = attackFrames[frameIndex];
                }
            }

            return;
        }

        if (idleFrames.Length > 1)
        {
            frameTimer += Time.deltaTime;
            if (frameTimer >= idleFrameDuration)
            {
                frameTimer -= idleFrameDuration;
                frameIndex = (frameIndex + 1) % idleFrames.Length;
                spriteRenderer.sprite = idleFrames[frameIndex];
            }
        }
    }

    public void PlayAttack()
    {
        if (attackFrames == null || attackFrames.Length == 0)
        {
            return;
        }

        attacking = true;
        frameIndex = 0;
        frameTimer = 0f;
        spriteRenderer.sprite = attackFrames[0];
    }

    public void StopAttack()
    {
        attacking = false;
        frameTimer = 0f;
        frameIndex = 0;
        if (idleFrames.Length > 0)
        {
            spriteRenderer.sprite = idleFrames[0];
        }
    }

    Sprite[] LoadIdleFrames()
    {
        Texture2D sheet = LoadSheet("Boss/idle");
        if (sheet == null)
        {
            return new Sprite[0];
        }

        int[] feet = { 18, 20, 20, 23 };
        Sprite[] frames = new Sprite[feet.Length];
        for (int i = 0; i < frames.Length; i++)
        {
            frames[i] = CreateFrame(sheet, i * 100, 0, feet[i]);
        }

        return frames;
    }

    Sprite[] LoadAttackFrames()
    {
        Texture2D sheet = LoadSheet("Boss/attacking");
        if (sheet == null)
        {
            return new Sprite[0];
        }

        int[] feetFromBottom = { 18, 20, 17, 18, 18, 18, 18, 18, 18, 18, 28, 28, 17 };
        Sprite[] frames = new Sprite[feetFromBottom.Length];
        for (int i = 0; i < frames.Length; i++)
        {
            int row = i / 6;
            int column = i % 6;
            int y = (2 - row) * 100;
            frames[i] = CreateFrame(sheet, column * 100, y, feetFromBottom[i]);
        }

        return frames;
    }

    Texture2D LoadSheet(string resourcePath)
    {
        Texture2D sheet = Resources.Load<Texture2D>(resourcePath);
        if (sheet == null)
        {
            Debug.LogWarning("Boss sprite sheet not found at Resources/" + resourcePath + ".png");
            return null;
        }

        sheet.filterMode = FilterMode.Point;
        return sheet;
    }

    Sprite CreateFrame(Texture2D sheet, int x, int y, int feetFromBottom)
    {
        const int frameSize = 100;
        float scaleY = Mathf.Abs(transform.lossyScale.y);
        float worldFrameHeight = frameSize / pixelsPerUnit * scaleY;
        BoxCollider2D hitbox = GetComponent<BoxCollider2D>();
        float colliderHalfHeight = hitbox != null ? hitbox.bounds.extents.y : worldFrameHeight * 0.5f;
        float feetOffset = feetFromBottom / pixelsPerUnit * scaleY;
        float pivotY = (colliderHalfHeight + feetOffset) / worldFrameHeight;

        return Sprite.Create(
            sheet,
            new Rect(x, y, frameSize, frameSize),
            new Vector2(0.5f, pivotY),
            pixelsPerUnit,
            0,
            SpriteMeshType.FullRect);
    }
}
