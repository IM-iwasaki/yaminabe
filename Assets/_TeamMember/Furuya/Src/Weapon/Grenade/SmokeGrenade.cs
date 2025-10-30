using UnityEngine;
using Mirror;

public class SmokeGrenade : GrenadeBase {
    private float smokeDuration;

    [Server]
    public void Init(SmokeData data, int teamID, Vector3 direction) {
        // GrenadeBaseのInitを呼び出す（ダメージ0、爆発半径0、味方にダメージなし）
        smokeDuration = data.duration;

        base.Init(
            teamID,
            direction,
            data.throwForce,
            data.projectileSpeed,
            0f, // explosionRadius
            0,  // damage
            false, // canDamageAllies
            data.useEffectType,
            data.explosionDelay
        );
    }

    [Server]
    protected override void Explode() {
        if (exploded) return;
        exploded = true;

        Vector3 pos = transform.position;

        // スモークエフェクトのみ再生（ダメージ処理なし）
        RpcPlayExplosion(pos, effectType, smokeDuration);

#if UNITY_EDITOR
        ExplosionDebugCircle.Create(pos, 1.5f, Color.gray, 0.5f);
#endif
    }
}