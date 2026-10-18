using UnityEngine;

/// <summary>
/// Displays the archer's idle, run, jump, and bow attack animations.
/// </summary>
public class ArcherSpriteAnimator : MonoBehaviour
{
    public float pixelsPerUnit = 32f;
    public float runFrameDuration = 0.11f;
    public float idleFrameDuration = 0.22f;
    public float attackFrameDuration = 0.075f;

    SpriteRenderer spriteRenderer;
    Rigidbody2D body;
    PlayerController player;
    Sprite[] idleFrames;
    Sprite[] runFrames;
    Sprite[] jumpFrames;
    Sprite[] attackFrames;
    Sprite[] currentFrames;
    int frameIndex;
    float frameTimer;
    bool wasGrounded;
    bool jumpAnimationComplete;
    bool isAttacking;
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        body = GetComponent<Rigidbody2D>();
        player = GetComponent<PlayerController>();

        // Sprite sheets are sliced from their top rows for consistent animation order.
        idleFrames = LoadTopRowFrames("Archer/Idle and running", 0, 2);
        runFrames = LoadTopRowFrames("Archer/Idle and running", 1, 8);
        jumpFrames = CombineFrames(
            LoadTopRowFrames("Archer/Jumping", 0, 8),
            LoadTopRowFrames("Archer/Jumping", 1, 2));
        attackFrames = CombineFrames(
            LoadTopRowFrames("Archer/Normal Attack", 0, 8),
            LoadTopRowFrames("Archer/Normal Attack", 1, 3));

        if (idleFrames.Length > 0)
        {
            currentFrames = idleFrames;
            spriteRenderer.sprite = idleFrames[0];
        }
    }

    void Update()
    {
        if (spriteRenderer == null || body == null || player == null || GameManager.Instance == null || GameManager.Instance.State != GameState.Playing)
        {
            return;
        }

        if (isAttacking)
        {
            frameTimer += Time.deltaTime;
            if (frameTimer >= attackFrameDuration)
            {
                frameTimer -= attackFrameDuration;
                if (frameIndex < attackFrames.Length - 1)
                {
                    frameIndex++;
                    spriteRenderer.sprite = attackFrames[frameIndex];
                    if (frameIndex == 6)
                    {
                        player.FireArrow();
                    }
                }
                else
                {
                    isAttacking = false;
                }
            }

            UpdateFacing();
            if (isAttacking)
            {
                return;
            }
        }

        bool grounded = player.IsGrounded();
        if (!grounded && jumpFrames.Length > 0)
        {
            if (wasGrounded)
            {
                currentFrames = jumpFrames;
                frameIndex = 0;
                frameTimer = 0f;
                jumpAnimationComplete = false;
                spriteRenderer.sprite = jumpFrames[0];
            }

            if (!jumpAnimationComplete)
            {
                frameTimer += Time.deltaTime;
                if (frameTimer >= 0.08f)
                {
                    frameTimer -= 0.08f;
                    if (frameIndex < jumpFrames.Length - 1)
                    {
                        frameIndex++;
                        spriteRenderer.sprite = jumpFrames[frameIndex];
                    }
                    else
                    {
                        jumpAnimationComplete = true;
                    }
                }
            }

            wasGrounded = false;
            UpdateFacing();
            return;
        }

        wasGrounded = grounded;
        jumpAnimationComplete = false;
        Sprite[] groundAnimation = Mathf.Abs(body.linearVelocity.x) > 0.1f ? runFrames : idleFrames;
        float duration = groundAnimation == runFrames ? runFrameDuration : idleFrameDuration;
        SetGroundAnimation(groundAnimation, duration);
        UpdateFacing();
    }

    public void PlaySlash()
    {
        if (attackFrames == null || attackFrames.Length == 0)
        {
            return;
        }

        isAttacking = true;
        currentFrames = attackFrames;
        frameIndex = 0;
        frameTimer = 0f;
        spriteRenderer.sprite = attackFrames[0];
    }

    void SetGroundAnimation(Sprite[] animation, float duration)
    {
        if (animation == null || animation.Length == 0)
        {
            return;
        }

        if (animation != currentFrames)
        {
            currentFrames = animation;
            frameIndex = 0;
            frameTimer = 0f;
            spriteRenderer.sprite = currentFrames[0];
        }

        frameTimer += Time.deltaTime;
        if (frameTimer >= duration)
        {
            frameTimer -= duration;
            frameIndex = (frameIndex + 1) % currentFrames.Length;
            spriteRenderer.sprite = currentFrames[frameIndex];
        }
    }

    void UpdateFacing()
    {
        if (Mathf.Abs(body.linearVelocity.x) > 0.1f)
        {
            spriteRenderer.flipX = body.linearVelocity.x < 0f;
        }
    }

    Sprite[] LoadTopRowFrames(string resourcePath, int rowFromTop, int frameCount)
    {
        const int frameSize = 64;
        Texture2D sheet = Resources.Load<Texture2D>(resourcePath);
        if (sheet == null)
        {
            Debug.LogWarning("Archer sprite sheet not found at Resources/" + resourcePath + ".png");
            return new Sprite[0];
        }

        sheet.filterMode = FilterMode.Point;
        int availableRows = sheet.height / frameSize;
        int availableColumns = sheet.width / frameSize;
        if (rowFromTop >= availableRows)
        {
            Debug.LogWarning("Archer animation row is missing in Resources/" + resourcePath + ".png");
            return new Sprite[0];
        }

        int count = Mathf.Min(frameCount, availableColumns);
        Sprite[] frames = new Sprite[count];
        float colliderHalfHeightPixels = 0.6f * pixelsPerUnit;
        for (int i = 0; i < count; i++)
        {
            float pivotY = (8f + colliderHalfHeightPixels) / frameSize;
            frames[i] = Sprite.Create(
                sheet,
                new Rect(i * frameSize, (availableRows - rowFromTop - 1) * frameSize, frameSize, frameSize),
                new Vector2(0.5f, pivotY),
                pixelsPerUnit,
                0,
                SpriteMeshType.FullRect);
        }

        return frames;
    }

    Sprite[] CombineFrames(Sprite[] first, Sprite[] second)
    {
        Sprite[] combined = new Sprite[first.Length + second.Length];
        first.CopyTo(combined, 0);
        second.CopyTo(combined, first.Length);
        return combined;
    }
}
