using Mirror;
using Mirror.BouncyCastle.Asn1.X509;
using Mirror.Examples.Tanks;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using static ShieldData;

public class Shield : NetworkBehaviour {
    [SyncVar] private int ownerTeamID;
    private int ID;

    [Server]
    public void Init(int teamID, float duration) {
        ownerTeamID = teamID;

        StartCoroutine(ReturnAfterSec(duration));
    }

    [Server]
    void OnTriggerEnter(Collider other) {
        // プレイヤー・敵は無視
        if (other.TryGetComponent<CreatureBase>(out _))
            return;

        // --- Projectile ---
        if (other.TryGetComponent<Projectile>(out var proj)) {
            // 味方 → 通過
            if (proj.teamID == ownerTeamID)
                return;
            proj.Deactivate();
            return;
        }

        // --- DoTエリア ---
        if (other.TryGetComponent<DoTArea>(out var dot)) {
            // 味方 → 通過
            if (dot.ownerTeamID == ownerTeamID)
                return;

            dot.Deactivate();
            return;
        }
    }

    [Server]
    private IEnumerator ReturnAfterSec(float delay) {
        yield return new WaitForSeconds(delay);
        ReturnToPool();
    }

    /// <summary>
    /// プールに戻す
    /// </summary>
    [Server]
    private void ReturnToPool() {
        if (ProjectilePool.Instance != null)
            ProjectilePool.Instance.DespawnToPool(gameObject, 0.05f);
        else
            NetworkServer.Destroy(gameObject);
    }
}
