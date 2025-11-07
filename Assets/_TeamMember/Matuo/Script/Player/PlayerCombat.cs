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
    /// このプレイヤーが死亡したときに呼ばれる
    /// </summary>
    /// <param name="killer">倒した側のプレイヤー（自滅時は null または自分）</param>
    [Server]
    public void OnKill(NetworkIdentity killer) {
        if (ruleManager == null)
            ruleManager = RuleManager.Instance;

        if (ruleManager == null) return;

        var attacker = killer ? killer.GetComponent<PlayerCombat>() : null;
        int attackerTeam = attacker ? attacker.teamId : -1;

        // 自滅
        if (attacker == null || attacker == this) {
            int enemyTeamId = (teamId == 0) ? 1 : 0;
            Debug.LogWarning($"[自滅] Team{teamId} のプレイヤーが自滅 → Team{enemyTeamId} にスコア加算");
            ruleManager.OnTeamKill(enemyTeamId, 1);
            return;
        }

        // チームキル
        if (attackerTeam == teamId) {
            int enemyTeamId = (teamId == 0) ? 1 : 0;
            Debug.LogWarning($"[チームキル] Team{attackerTeam} が味方を倒した → Team{enemyTeamId} にスコア加算");
            ruleManager.OnTeamKill(enemyTeamId, 1);
            return;
        }

        // 通常の敵キル
        Debug.LogWarning($"[敵キル] Team{attackerTeam} が Team{teamId} を倒した → Team{attackerTeam} にスコア加算");
        ruleManager.OnTeamKill(attackerTeam, 1);
    }
}