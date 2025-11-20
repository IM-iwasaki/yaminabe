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
    [Server]
    public void OnKill(NetworkIdentity killerId, int victimTeam) {
        int killerTeam = -1;

        if (killerId != null && killerId != netIdentity)
            killerTeam = killerId.GetComponent<PlayerCombat>().teamId;

        if (RuleManager.Instance.currentRule != GameRuleType.DeathMatch)
            return;

        // 自滅または味方キル
        if (killerId == null || killerId == netIdentity || killerTeam == victimTeam) {
            int enemyTeam = (victimTeam == 0) ? 1 : 0;
            RuleManager.Instance.OnTeamKillByTeam(enemyTeam);
            return;
        }

        // 敵キル
        RuleManager.Instance.OnTeamKillByTeam(killerTeam);
    }
}