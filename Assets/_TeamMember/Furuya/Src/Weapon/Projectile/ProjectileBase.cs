using UnityEngine;
using Mirror;

public class ProjectileBase : NetworkBehaviour {
    public float speed = 20f;
    public float lifeTime = 3f;
    public int damage = 10;

    private float lifeTimer;
    public GameObject owner;

    private Rigidbody rb;

    public void Init(GameObject owner, float speed, int damage) {
        this.owner = owner;
        this.speed = speed;
        this.damage = damage;
        lifeTimer = 0f;

        rb = GetComponent<Rigidbody>();
        if (rb != null) {
            rb.velocity = transform.forward * speed;
        }
    }

    void OnEnable() {
        lifeTimer = 0f;
    }

    void Update() {
        // Rigidbody がある場合は物理で動くので Update で動かさない
        if (rb == null) {
            transform.position += transform.forward * speed * Time.deltaTime;
        }

        lifeTimer += Time.deltaTime;
        if (lifeTimer >= lifeTime)
            Despawn();
    }

    [ServerCallback]
    void OnTriggerEnter(Collider other) {
        if (other.gameObject == owner) return;

        CharacterBase target = other.GetComponent<CharacterBase>();
        if (target != null) {
            target.TakeDamage(damage);
        }

        // ヒットエフェクト再生（クライアント側）
        RpcPlayHitEffect(transform.position);

        Despawn();
    }

    [ClientRpc]
    void RpcPlayHitEffect(Vector3 pos) {
        if (WeaponPoolRegistry.Instance.hitEffect != null) {
            var fx = EffectPoolManager.Instance.GetFromPool(
                WeaponPoolRegistry.Instance.hitEffect, pos, Quaternion.identity
            );
            EffectPoolManager.Instance.ReturnToPool(fx, 1.5f);
        }
    }

    public void Despawn() {
        gameObject.SetActive(false);
        rb?.Sleep();
    }
}
