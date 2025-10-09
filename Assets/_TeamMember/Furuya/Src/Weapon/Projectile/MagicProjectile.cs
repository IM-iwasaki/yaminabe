using UnityEngine;
using Mirror;
using static UnityEngine.UI.GridLayoutGroup;

public class MagicProjectile : ProjectileBase {
    public float explosionRadius = 3f;

    void Explode() {
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (var hit in hits) {
            var target = hit.GetComponent<CharacterBase>();
            if (target != null && target.gameObject != owner) {
                target.TakeDamage(damage);
            }
        }

        // エフェクト再生
        if (WeaponPoolRegistry.Instance.hitEffect)
            EffectPoolManager.Instance.GetFromPool(
                WeaponPoolRegistry.Instance.hitEffect, transform.position, Quaternion.identity
            );

        Despawn();
    }

    void OnTriggerEnter(Collider other) {
        if (other.gameObject == owner) return;
        Explode();
    }
}
