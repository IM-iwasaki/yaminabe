using UnityEngine;
using Mirror;
using System.Collections;
using Unity.VisualScripting;

public class ExplosionProjectile : NetworkBehaviour {
    private int damage;
    private float speed;
    private float radius;

    private Rigidbody rb;
    private GameObject owner;
    private EffectType hitEffectType;
    private bool initialized;
    private float lifetime = 5f;

    protected bool isActivated;

    public void Init(GameObject shooter, EffectType hitEffect, float _speed, int _damage) {
        owner = shooter;
        hitEffectType = hitEffect;
        speed = _speed;
        damage = _damage;

        isActivated = false;

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

        Explode();

        RpcPlayHitEffect(transform.position, hitEffectType);

        Deactivate();
    }

    IEnumerator AutoDisable() {

        yield return new WaitForSeconds(lifetime);
        Explode();
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

    [Server]
    private void Explode() {
        if (isActivated) return;
        isActivated = true;

        var hits = Physics.OverlapSphere(transform.position, radius, LayerMask.GetMask("Character"));
        foreach (var c in hits) {
            var target = c.GetComponent<CharacterBase>();
            if (target == null) continue;
            target.TakeDamage(damage);
        }

        RpcPlayHitEffect(transform.position, hitEffectType);

        Deactivate();
    }
}