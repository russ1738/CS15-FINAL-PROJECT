using UnityEngine;

/// <summary>
/// Side-scroller movement plus nearby-object interaction.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 7f;
    public float jumpForce = 12f;
    public float groundCheckDistance = 0.12f;
    public float interactRadius = 0.9f;
    public int maxHealth = 5;
    public int slashDamage = 5;
    public Vector3 spawnPoint;

    public int Health => health;
    public int MaxHealth => maxHealth;

    Rigidbody2D body;
    BoxCollider2D box;
    bool jumpQueued;
    int facingDirection = 1;
    int health;
    float slashCooldown;
    float invulnerableUntil;
    ArcherSpriteAnimator archerAnimator;

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        box = GetComponent<BoxCollider2D>();
        archerAnimator = GetComponent<ArcherSpriteAnimator>();
        spawnPoint = transform.position;
        health = maxHealth;
        if (GameManager.Instance != null)
        {
            slashDamage += GameManager.Instance.BowDamageBonus;
        }
        body.freezeRotation = true;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing)
        {
            return;
        }

        if (IsGrounded() && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)))
        {
            jumpQueued = true;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }

        slashCooldown -= Time.deltaTime;
        if (GameManager.Instance.CurrentLevel == 3 && Input.GetKeyDown(KeyCode.F) && slashCooldown <= 0f)
        {
            Slash();
            slashCooldown = 0.45f;
        }

        if (transform.position.y < -10f)
        {
            Respawn();
        }
    }

    void FixedUpdate()
    {
        if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing)
        {
            body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
            return;
        }

        float x = 1f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            x -= 10f;
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            x += 1f;
        }

        if (x != 0f)
        {
            facingDirection = x > 0f ? 1 : -1;
        }

        body.linearVelocity = new Vector2(x * moveSpeed, body.linearVelocity.y);

        if (jumpQueued)
        {
            body.linearVelocity = new Vector2(body.linearVelocity.x, jumpForce);
            jumpQueued = false;
        }
    }

    public bool IsGrounded()
    {
        Vector2 origin = (Vector2)transform.position + Vector2.down * (box.bounds.extents.y + 0.02f);
        RaycastHit2D[] hits = Physics2D.BoxCastAll(origin, new Vector2(box.bounds.size.x * 0.8f, 0.04f), 0f, Vector2.down, groundCheckDistance);
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null || hit.collider.isTrigger || hit.collider.gameObject == gameObject)
            {
                continue;
            }

            return true;
        }

        return false;
    }

    void TryInteract()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactRadius);
        IInteractable closest = null;
        float best = float.MaxValue;

        foreach (Collider2D hit in hits)
        {
            var interactable = hit.GetComponent<IInteractable>();
            if (interactable == null)
            {
                continue;
            }

            float distance = Vector2.Distance(transform.position, hit.transform.position);
            if (distance < best)
            {
                best = distance;
                closest = interactable;
            }
        }

        closest?.Interact(this);
    }

    void LateUpdate()
    {
        if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing)
        {
            GameManager.Instance?.SetPrompt("");
            return;
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactRadius);
        string nextPrompt = "";
        float best = float.MaxValue;

        foreach (Collider2D hit in hits)
        {
            var interactable = hit.GetComponent<IInteractable>();
            if (interactable == null)
            {
                continue;
            }

            float distance = Vector2.Distance(transform.position, hit.transform.position);
            if (distance < best)
            {
                best = distance;
                nextPrompt = interactable.Prompt;
            }
        }

        GameManager.Instance.SetPrompt(nextPrompt);
    }

    public void Respawn()
    {
        transform.position = spawnPoint;
        body.linearVelocity = Vector2.zero;
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0 || health <= 0 || Time.time < invulnerableUntil)
        {
            return;
        }

        invulnerableUntil = Time.time + 0.9f;
        health = Mathf.Max(0, health - amount);
        if (health == 0)
        {
            GameManager.Instance?.Lose();
            body.linearVelocity = Vector2.zero;
            return;
        }

        body.linearVelocity = new Vector2(-facingDirection * 2f, Mathf.Max(body.linearVelocity.y, 1f));
    }

    public void Heal(int amount)
    {
        if (amount > 0)
        {
            health = Mathf.Min(maxHealth, health + amount);
        }
    }

    public void SetHealth(int value)
    {
        health = Mathf.Clamp(value, 0, maxHealth);
    }

    public void UpgradeArrowDamage(int amount)
    {
        if (amount > 0)
        {
            slashDamage += amount;
        }
    }

    void Slash()
    {
        if (archerAnimator == null)
        {
            archerAnimator = GetComponent<ArcherSpriteAnimator>();
        }
        archerAnimator?.PlaySlash();
    }

    public void FireArrow()
    {
        GameObject arrow = new GameObject("PlayerArrow");
        arrow.transform.position = transform.position + new Vector3(facingDirection * 0.62f, 0.12f, 0f);
        arrow.transform.localScale = new Vector3(facingDirection, 1f, 1f);

        SpriteRenderer arrowRenderer = arrow.AddComponent<SpriteRenderer>();
        arrowRenderer.sprite = SpriteFactory.Arrow();
        arrowRenderer.sortingOrder = GetComponent<SpriteRenderer>().sortingOrder + 1;

        BoxCollider2D arrowCollider = arrow.AddComponent<BoxCollider2D>();
        arrowCollider.isTrigger = true;
        arrowCollider.size = new Vector2(0.9f, 0.55f);

        Rigidbody2D arrowBody = arrow.AddComponent<Rigidbody2D>();
        arrowBody.bodyType = RigidbodyType2D.Kinematic;
        arrowBody.gravityScale = 0f;
        arrowBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        Projectile projectile = arrow.AddComponent<Projectile>();
        projectile.fromPlayer = true;
        projectile.damage = slashDamage;
        projectile.velocity = new Vector2(facingDirection * 12f, 0f);
    }
}
