using UnityEngine;
using Mirror;

public class MagicProjectile : NetworkBehaviour {
    [Header("Projectile Settings")]
    public ProjectileType type = ProjectileType.Linear;
    public float speed = 20f;
    public float initialHeightSpeed = 5f;
    public int damage = 10;

    private Rigidbody rb;
    private GameObject owner;
    private WeaponData weaponData;

    public void Init(GameObject shooter, WeaponData data, ProjectileType _type, float _speed, float _initialHeightSpeed, int _damage) {
        owner = shooter;
        weaponData = data;
        type = _type;
        speed = _speed;
        initialHeightSpeed = _initialHeightSpeed;
        damage = _damage;

        rb = GetComponent<Rigidbody>();
        if (type == ProjectileType.Parabola && rb != null) {
            rb.useGravity = true;
            rb.velocity = transform.forward * speed + Vector3.up * initialHeightSpeed;
        }
        else if (rb != null) {
            rb.useGravity = false;
            rb.velocity = transform.forward * speed;
        }
    }

    void FixedUpdate() {
        if (!isServer) return; // サーバーのみで移動制御
        if (type == ProjectileType.Linear && rb == null) {
            transform.position += transform.forward * speed * Time.fixedDeltaTime;
        }
    }

    void OnTriggerEnter(Collider other) {
        if (!isServer) return;
        if (other.gameObject == owner) return;

        CharacterBase target = other.GetComponent<CharacterBase>();
        if (target != null) {
            target.TakeDamage(damage);
        }

        // 💥 先にエフェクト呼び出し（破壊前）
        RpcPlayHitEffect(transform.position);

        // 少し待ってから破壊（RPCが確実に届くように）
        StartCoroutine(DelayedDestroy());
    }

    System.Collections.IEnumerator DelayedDestroy() {
        yield return new WaitForSeconds(0.05f);
        if (this != null && gameObject != null)
            NetworkServer.Destroy(gameObject);
    }

    [ClientRpc]
    void RpcPlayHitEffect(Vector3 pos) {
        if (weaponData == null || weaponData.hitEffectPrefab == null)
            return;

        GameObject fx = EffectPoolManager.Instance.GetFromPool(
            weaponData.hitEffectPrefab,
            pos,
            Quaternion.identity
        );

        if (fx != null)
            EffectPoolManager.Instance.ReturnToPool(fx, 1.5f);
    }
}
