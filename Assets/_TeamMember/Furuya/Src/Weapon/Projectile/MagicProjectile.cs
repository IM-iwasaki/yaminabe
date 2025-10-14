using UnityEngine;
using Mirror;
using System.Collections;

public class MagicProjectile : NetworkBehaviour {
    public ProjectileType type = ProjectileType.Linear;
    public float speed = 20f;
    public float initialHeightSpeed = 5f;
    public int damage = 10;

    private Rigidbody rb;
    private GameObject owner;
    private EffectType hitEffectType;

    public void Init(GameObject shooter, ProjectileType _type, EffectType hitEffect, float _speed, float _initialHeightSpeed, int _damage, Vector3 direction) {
        owner = shooter;
        type = _type;
        hitEffectType = hitEffect;
        speed = _speed;
        initialHeightSpeed = _initialHeightSpeed;
        damage = _damage;

        rb = GetComponent<Rigidbody>();
        if (rb != null) {
            if (type == ProjectileType.Parabola) {
                rb.useGravity = true;
                rb.velocity = direction * speed + Vector3.up * initialHeightSpeed;
            }
            else {
                rb.useGravity = false;
                rb.velocity = direction * speed;
            }
        }
    }

    void FixedUpdate() {
        if (!isServer) return;
        if (type == ProjectileType.Linear && rb == null) transform.position += transform.forward * speed * Time.fixedDeltaTime;
    }

    void OnTriggerEnter(Collider other) {
        if (!isServer) return;
        if (other.gameObject == owner) return;

        CharacterBase target = other.GetComponent<CharacterBase>();
        if (target != null) target.TakeDamage(damage);

        RpcPlayHitEffect(transform.position, hitEffectType);
        StartCoroutine(DelayedDestroy());
    }

    IEnumerator DelayedDestroy() {
        yield return new WaitForSeconds(0.05f);
        if (this != null && gameObject != null) NetworkServer.Destroy(gameObject);
    }

    [ClientRpc]
    void RpcPlayHitEffect(Vector3 pos, EffectType effectType) {
        GameObject prefab = WeaponPoolRegistry.Instance.GetHitEffect(effectType);
        if (prefab != null) {
            var fx = EffectPoolManager.Instance.GetFromPool(prefab, pos, Quaternion.identity);
            EffectPoolManager.Instance.ReturnToPool(fx, 1.5f);
        }
    }
}
