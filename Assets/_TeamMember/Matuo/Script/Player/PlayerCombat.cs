using Mirror;
using UnityEngine;

/// <summary>
/// デスマッチ用戦闘処理
/// </summary>
public class PlayerCombat : NetworkBehaviour {
    [SyncVar] public int teamId = 0;

    private RuleManager ruleManager;

    public override void OnStartServer() {
        ruleManager = RuleManager.Instance;
    }

    /// <summary>
    /// プレイヤーキル処理
    /// </summary>
    /// <param name="killer">倒した側のプレイヤー（自滅時は null または自分）</param>
    [Server]
    public void OnKill(NetworkIdentity killer) {
        if (ruleManager == null)
            ruleManager = RuleManager.Instance;

        if (ruleManager == null) return;

        var attacker = killer ? killer.GetComponent<PlayerCombat>() : null;
        int attackerTeam = attacker ? attacker.teamId : -1;

        // 自滅またはチームメイトをキル
        if (attacker == null || attacker == this || attackerTeam == teamId) {
            int enemyTeamId = (teamId == 0) ? 1 : 0;
            ruleManager.OnTeamKill(enemyTeamId, 1);
            return;
        }

        // 通常の敵キル
        ruleManager.OnTeamKill(attackerTeam, 1);
    }
}