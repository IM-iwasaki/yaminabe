using UnityEngine;
using Mirror;

public class MagicProjectile : NetworkBehaviour {
    [Header("Projectile Settings")]
    public ProjectileType type = ProjectileType.Linear;
    public float speed = 20f;
    public float initialHeightSpeed = 5f;
    public int damage = 10;

    Rigidbody rb;
    GameObject owner;

    // 初期化
    public void Init(GameObject shooter, ProjectileType _type, float _speed, float _initialHeightSpeed, int _damage) {
        owner = shooter;
        type = _type;
        speed = _speed;
        initialHeightSpeed = _initialHeightSpeed;
        damage = _damage;

        rb = GetComponent<Rigidbody>();

        if (type == ProjectileType.Parabola) {
            if (rb != null) {
                rb.useGravity = true;
                rb.velocity = transform.forward * speed + Vector3.up * initialHeightSpeed;
            }
        }
    }

    void FixedUpdate() {
        if (type == ProjectileType.Linear) {
            transform.Translate(transform.forward * speed * Time.fixedDeltaTime, Space.World);
        }
    }

    void OnTriggerEnter(Collider other) {
        if (other.gameObject == owner) return;

        var target = other.GetComponent<CharacterBase>();
        if (target != null) {
            target.TakeDamage(damage);
        }

        // 爆発エフェクトなどはここで再生
        Destroy(gameObject);
    }
}
