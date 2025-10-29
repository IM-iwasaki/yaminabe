using UnityEngine;
using Mirror;
using System.Collections;


public class LandMine : TrapBase {
    private float explosionRadius;
    private int damage;
    private bool canDamageAllies;
    private EffectType explosionEffect;

    [Server]
    public void Init(
    TrapInitData trapData,
    float explosionRadius,
    int damage,
    bool canDamageAllies,
    EffectType explosionEffect
) {
        base.Init(trapData);
        this.explosionRadius = explosionRadius;
        this.damage = damage;
        this.canDamageAllies = canDamageAllies;
        this.explosionEffect = explosionEffect;

        hasTriggered = false;
        isActivated = false;

        StartCoroutine(TimerExplosionRoutine(trapData.duration));
    }

    [ServerCallback]
    private void OnTriggerEnter(Collider other) {
        if (!isActivated || hasTriggered) return;
        if (other.TryGetComponent(out CharacterBase target)) {
            if (!canDamageAllies && target.TeamID == ownerTeamID) return;
            Explode();
        }
    }

    [Server]
    private IEnumerator TimerExplosionRoutine(float delay) {
        yield return new WaitForSeconds(delay);
        if (!hasTriggered) Explode();
    }

    [Server]
    private void Explode() {
        hasTriggered = true;

        var hits = Physics.OverlapSphere(transform.position, explosionRadius, LayerMask.GetMask("Character"));
        foreach (var c in hits) {
            var target = c.GetComponent<CharacterBase>();
            if (target == null) continue;
            if (!canDamageAllies && target.TeamID == ownerTeamID) continue;
            target.TakeDamage(damage);
        }

        RpcPlayEffect(transform.position, explosionEffect);
        ProjectilePool.Instance.DespawnToPool(gameObject);
    }
}