using UnityEngine;
using Mirror;
using System.Collections;

public class Projectile : NetworkBehaviour {
    private int damage;
    private float speed;

    private Rigidbody rb;
    private GameObject owner;
    private EffectType hitEffectType;
    private bool initialized;
    private float lifetime = 5f;

    public void Init(GameObject shooter, EffectType hitEffect, float _speed, int _damage) {
        owner = shooter;
        hitEffectType = hitEffect;
        speed = _speed;
        damage = _damage;

        if (rb == null) rb = GetComponent<Rigidbody>();

        initialized = true;

        if (isServer) {
            StopAllCoroutines();
            StartCoroutine(AutoDisable()); // 自動で非アクティブ化
        }
    }

    void FixedUpdate() {
        if (!isServer) return;
        transform.position += transform.forward * speed * Time.fixedDeltaTime;
    }

    [ServerCallback]
    void OnTriggerEnter(Collider other) {
        if (!initialized || !isServer) return;
        if (other.gameObject == owner) return;

        if (other.TryGetComponent<NetworkIdentity>(out var identity)) {
            if (identity.TryGetComponent(out CharacterBase target)) {
                target.TakeDamage(damage);
            }
        }

        RpcPlayHitEffect(transform.position, hitEffectType);

        Deactivate();
    }

    IEnumerator AutoDisable() {
        yield return new WaitForSeconds(lifetime);
        Deactivate();
    }

    [Server]
    private void Deactivate() {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        initialized = false;

        if (ProjectilePool.Instance != null)
            ProjectilePool.Instance.DespawnToPool(gameObject);
        else
            NetworkServer.Destroy(gameObject);
    }

    [ClientRpc(includeOwner = true)]
    void RpcPlayHitEffect(Vector3 pos, EffectType effectType) {

        GameObject prefab = EffectPoolRegistry.Instance.GetHitEffect(effectType);
        if (prefab != null) {
            var fx = WeaponEffectPool.Instance.GetFromPool(prefab, pos, Quaternion.identity);
            fx.SetActive(true);
            WeaponEffectPool.Instance.ReturnToPool(fx, 1.5f);
        }
    }
}