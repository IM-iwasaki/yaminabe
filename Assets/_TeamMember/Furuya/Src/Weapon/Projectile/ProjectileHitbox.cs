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

        if (other.TryGetComponent<CreatureBase>(out _))
            return; // プレイヤー・敵は無視

        projectile.ServerDeactivate();
    }
}
