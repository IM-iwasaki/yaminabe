using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileHitbox : MonoBehaviour
{
    private Projectile projectile;

    void Awake() {
        projectile = GetComponentInParent<Projectile>();
    }

    void OnTriggerEnter(Collider other) {
        if (!projectile.isServer) return;

        // --- Shield判定 ---
        if (other.TryGetComponent<Shield>(out var shield)) {
            // 同じチーム → 貫通
            if (shield.ownerTeamID == projectile.teamID)
                return;
        }

        if (other.TryGetComponent<CreatureBase>(out _))
            return; // プレイヤー・敵は無視
        if (other.TryGetComponent<DoTArea>(out _))
            return;

        if (other.gameObject.tag == "Magic" || other.gameObject.tag == "Shield")
            return;

        if (other.TryGetComponent<ExplosionProjectile>(out _))
            return;

        projectile.ServerDeactivate();
    }
}
