using UnityEngine;
using Mirror;

public class ProjectileBase : NetworkBehaviour {
    public float speed = 20f;
    public float lifeTime = 3f;
    public int damage = 10;

    private float lifeTimer;
    public GameObject owner;

    public void Init(GameObject owner, float speed, int damage) {
        this.owner = owner;
        this.speed = speed;
        this.damage = damage;
        lifeTimer = 0f;
    }

    void Update() {
        transform.position += transform.forward * speed * Time.deltaTime;

        lifeTimer += Time.deltaTime;
        if (lifeTimer >= lifeTime)
            Despawn();
    }

    void OnTriggerEnter(Collider other) {
        if (other.gameObject == owner) return;

        CharacterBase target = other.GetComponent<CharacterBase>();
        if (target != null) {
            target.TakeDamage(damage);
        }

        // ヒットエフェクト再生
        if (WeaponPoolRegistry.Instance.hitEffect != null) {
            var fx = EffectPoolManager.Instance.GetFromPool(WeaponPoolRegistry.Instance.hitEffect, transform.position, Quaternion.identity);
            EffectPoolManager.Instance.ReturnToPool(fx, 1.5f);
        }

        Despawn();
    }

    public void Despawn() {
        gameObject.SetActive(false);
    }
}