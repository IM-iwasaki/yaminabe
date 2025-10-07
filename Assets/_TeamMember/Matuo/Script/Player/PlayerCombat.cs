using UnityEngine;
using Mirror;

/// <summary>
/// デスマッチ用戦闘処理
/// </summary>
public class PlayerCombat : NetworkBehaviour {
    [SyncVar] public int teamId = 0;

    /// <summary>
    /// 敵を倒したときの処理
    /// </summary>
    /// <param name="target">倒した相手</param>
    [Server]
    public void OnKill(NetworkIdentity target) {
        var objectManager = FindAnyObjectByType<ObjectManager>();
        objectManager?.NotifyKill(teamId, 1);
    }
}