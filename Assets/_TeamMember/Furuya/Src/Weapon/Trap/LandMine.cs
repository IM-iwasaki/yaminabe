using UnityEngine;
using Mirror;
using System.Collections;

/// <summary>
/// ’n—‹
/// </summary>
public class LandMine : TrapBase {
    private float explosionRadius;
    private int damage;
    private bool canDamageAllies;
    private EffectType explosionEffect;
    private int ID;

    [Server]
    public void Init(
    TrapInitData trapData,
    float explosionRadius,
    int damage,
    bool canDamageAllies,
    int _ID,
    EffectType explosionEffect
    ) {
        base.Init(trapData);
        this.explosionRadius = explosionRadius;
        this.damage = damage;
        this.canDamageAllies = canDamageAllies;
        this.ID = _ID;
        this.explosionEffect = explosionEffect;

        hasTriggered = false;
        isActivated = false;

        StartCoroutine(TimerExplosionRoutine(trapData.duration));
    }

    [ServerCallback]
    private void OnTriggerEnter(Collider other) {
        if (!isActivated || hasTriggered) return;
        if (other.TryGetComponent(out CreatureBase target)) {
            if (!canDamageAllies && target.teamID == ownerTeamID) return;
            Explode();
        }
    }

    /// <summary>
    /// ”š”­‚Ü‚Å‘Ò‹@
    /// </summary>
    /// <param name="delay"></param>
    /// <returns></returns>
    [Server]
    private IEnumerator TimerExplosionRoutine(float delay) {
        yield return new WaitForSeconds(delay);
        if (!hasTriggered) Explode();
    }

    /// <summary>
    /// ”š”­
    /// </summary>
    [Server]
    private void Explode() {
        hasTriggered = true;

        var hits = Physics.OverlapSphere(transform.position, explosionRadius, LayerMask.GetMask("Character"));
        foreach (var c in hits) {
            var target = c.GetComponent<CreatureBase>();
            if (target == null) continue;
            if (!canDamageAllies && target.teamID == ownerTeamID) continue;
            target.TakeDamage(damage, ownerName, ID);
        }

        RpcPlayEffect(transform.position, explosionEffect);
        ProjectilePool.Instance.DespawnToPool(gameObject);
    }
}