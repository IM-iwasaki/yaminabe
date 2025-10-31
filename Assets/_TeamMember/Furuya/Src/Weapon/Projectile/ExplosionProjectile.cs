using UnityEngine;
using Mirror;
using System.Collections;

public class ExplosionProjectile : NetworkBehaviour {
    [SerializeField] float radius = 3f;
    [SerializeField] float delay = 1.5f;
    [SerializeField] EffectType hitEffectType = EffectType.Default;

    private int damage;
    private bool initialized = false;

    public void Init(int dmg, EffectType effectType) {
        damage = dmg;
        hitEffectType = effectType;
        initialized = true;

        if (isServer) {
            StopAllCoroutines();
            StartCoroutine(DelayedExplode());
        }
    }

    IEnumerator DelayedExplode() {
        yield return new WaitForSeconds(delay);
        Explode();
    }

    [Server]
    void Explode() {
        Collider[] hits = Physics.OverlapSphere(transform.position, radius);
        foreach (var c in hits) {
            if (c.TryGetComponent(out CharacterBase target)) {
                target.TakeDamage(damage);
            }
        }

        RpcPlayHitEffect(transform.position, hitEffectType);

        Deactivate();
    }

    [Server]
    void Deactivate() {
        initialized = false;

        if (ProjectilePool.Instance != null) {
            ProjectilePool.Instance.DespawnToPool(gameObject);
        }
        else {
            NetworkServer.Destroy(gameObject);
        }
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
