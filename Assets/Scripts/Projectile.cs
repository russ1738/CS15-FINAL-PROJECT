using UnityEngine;

public class Projectile : MonoBehaviour
{
    public bool fromPlayer;
    public int damage = 1;
    public float life = 3.5f;
    public Vector2 velocity;

    void Update()
    {
        transform.position += (Vector3)(velocity * Time.deltaTime);
        life -= Time.deltaTime;
        if (life <= 0f)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (fromPlayer)
        {
            var boss = other.GetComponent<BossController>();
            if (boss != null)
            {
                boss.TakeDamage(damage);
                Destroy(gameObject);
            }
            else if (!other.isTrigger && other.GetComponent<PlayerController>() == null)
            {
                Destroy(gameObject);
            }

            return;
        }

        var player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.TakeDamage(damage);
            Destroy(gameObject);
        }
        else if (!other.isTrigger && other.GetComponent<BossController>() == null)
        {
            Destroy(gameObject);
        }
    }
}
