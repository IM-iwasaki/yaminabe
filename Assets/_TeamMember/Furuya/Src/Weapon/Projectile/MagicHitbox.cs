using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicHitbox : MonoBehaviour
{
    private MagicProjectile projectile;

    void Awake() {
        projectile = GetComponentInParent<MagicProjectile>();
    }

    void OnTriggerEnter(Collider other) {
        if (!projectile.isServer) return;

        if (other.TryGetComponent<CreatureBase>(out _))
            return; // プレイヤー・敵は無視
        if (other.TryGetComponent<DoTArea>(out _))
            return;

        if (other.TryGetComponent<MagicProjectile>(out _))
            return;

        projectile.ServerDeactivate();
    }
}
