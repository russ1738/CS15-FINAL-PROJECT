using UnityEngine;

/// <summary>
/// Patrols the arena and attacks nearby players with a scythe swing.
/// </summary>
public class BossController : MonoBehaviour
{
    public int maxHp = 10;
    public float moveSpeed = 2.5f;
    public float leftX = 4f;
    public float rightX = 16f;
    public float attackInterval = 1.8f;
    public float attackRange = 3.1f;
    public float attackWindup = 0.28f;
    public int attackDamage = 1;

    int hp;
    int direction = 1;
    float attackCooldown = 0.5f;
    float attackElapsed;
    bool isAttacking;
    bool hitApplied;
    int attackDirection = 1;
    SpriteRenderer sprite;
    BossSpriteAnimator animator;

    public int Hp => hp;

    public void SetMaxHp(int value)
    {
        maxHp = Mathf.Max(1, value);
        hp = maxHp;
        GameManager.Instance?.NotifyBossHp(hp, maxHp);
    }

    void Awake()
    {
        hp = maxHp;
        sprite = GetComponent<SpriteRenderer>();
        animator = GetComponent<BossSpriteAnimator>();
    }

    void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing || hp <= 0)
        {
            return;
        }

        GameObject playerObject = GameObject.FindWithTag("Player");
        PlayerController player = playerObject != null ? playerObject.GetComponent<PlayerController>() : null;
        if (player == null)
        {
            return;
        }

        if (isAttacking)
        {
            UpdateAttack(player);
            return;
        }

        attackCooldown -= Time.deltaTime;
        Vector2 toPlayer = player.transform.position - transform.position;
        if (attackCooldown <= 0f && Mathf.Abs(toPlayer.x) <= attackRange && Mathf.Abs(toPlayer.y) <= 2.5f)
        {
            BeginAttack(toPlayer.x >= 0f ? 1 : -1);
            return;
        }

        transform.position += new Vector3(direction * moveSpeed * Time.deltaTime, 0f, 0f);
        if (transform.position.x > rightX)
        {
            direction = -1;
        }
        else if (transform.position.x < leftX)
        {
            direction = 1;
        }

        SetFacing(direction);
    }

    void BeginAttack(int facing)
    {
        isAttacking = true;
        hitApplied = false;
        attackElapsed = 0f;
        attackDirection = facing;
        SetFacing(attackDirection);
        animator?.PlayAttack();
    }

    void UpdateAttack(PlayerController player)
    {
        attackElapsed += Time.deltaTime;
        if (!hitApplied && attackElapsed >= attackWindup)
        {
            hitApplied = true;
            Vector2 toPlayer = player.transform.position - transform.position;
            if (toPlayer.x * attackDirection >= -0.25f && Mathf.Abs(toPlayer.x) <= attackRange && Mathf.Abs(toPlayer.y) <= 2.5f)
            {
                player.TakeDamage(attackDamage);
            }
        }

        float attackDuration = animator != null ? animator.AttackDuration : 0.8f;
        if (attackElapsed >= attackDuration)
        {
            isAttacking = false;
            attackCooldown = attackInterval;
            animator?.StopAttack();
        }
    }

    void SetFacing(int facing)
    {
        if (sprite != null)
        {
            sprite.flipX = facing < 0;
        }
    }

    public void TakeDamage(int amount)
    {
        if (hp <= 0 || amount <= 0)
        {
            return;
        }

        hp = Mathf.Max(0, hp - amount);
        GameManager.Instance?.NotifyBossHp(hp, maxHp);

        if (hp <= 0)
        {
            GameManager.Instance?.Win();
            Destroy(gameObject);
        }
    }
}
