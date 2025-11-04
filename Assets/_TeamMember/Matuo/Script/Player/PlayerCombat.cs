using Mirror;
using UnityEngine;

/// <summary>
/// デスマッチ用戦闘処理
/// </summary>
public class PlayerCombat : NetworkBehaviour {
    [SyncVar] public int teamId = 0;

    private RuleManager ruleManager;

    public override void OnStartServer() {
        // RuleManagerをキャッシュ
        ruleManager = RuleManager.Instance;
    }

    /// <summary>
    /// 敵を倒したときの処理（サーバー専用）
    /// </summary>
    /// <param name="target">倒した相手</param>
    [Server]
    public void OnKill(NetworkIdentity target) {
        if (ruleManager == null)
            ruleManager = RuleManager.Instance;

        if (ruleManager == null) return;

        // 相手情報を取得
        var victim = target ? target.GetComponent<PlayerCombat>() : null;
        int victimTeam = victim ? victim.teamId : -1;

        // チームキル判定
        if (victim != null && victimTeam == teamId) {
            Debug.LogWarning($"[TeamKill] Team {teamId} のプレイヤーが味方を倒しました！相手チームにポイントを加算します。");

            // 相手チーム（ここでは「自分以外のチーム」として推定）
            int enemyTeamId = (teamId == 0) ? 1 : 0;

            // 相手チームにスコア加算
            ruleManager.OnTeamKill(enemyTeamId, 1);
            return;
        }

        // 敵チームを倒した場合
        ruleManager.OnTeamKill(teamId, 1);

        Debug.Log($"[Kill] Team {teamId} が Team {victimTeam} のプレイヤーを撃破");
    }
}