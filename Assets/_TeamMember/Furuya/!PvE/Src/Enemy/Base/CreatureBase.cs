using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Mirror.BouncyCastle.Crypto.Digests.SkeinEngine;

public class CreatureBase : NetworkBehaviour
{

    public CharacterParameter parameter { get; private set; }

    protected void Awake() {
        parameter = GetComponent<CharacterParameter>();
    }

    /// <summary>
    /// 被弾・死亡判定関数
    /// </summary>
    [Server]
    public virtual void TakeDamage(int _damage, string _name, int _ID) {
        //既に死亡状態かロビー内なら帰る
        if (parameter.isDead || !GameManager.Instance.IsGameRunning()) return;

        //ダメージ倍率を適用
        float damage = _damage * ((float) parameter.DamageRatio / 100);
        //ダメージが0以下だったら1に補正する
        if (damage <= 0) damage = 1;
        //HPの減算処理
        parameter.HP -= (int) damage;

        // hitSE 再生
        PlayHitSE(_ID);

        if (parameter.HP <= 0) {
            parameter.HP = 0;

            if (PlayerListManager.Instance != null) {
                // スコア加算
                PlayerListManager.Instance.AddScoreById(_ID, 100);
                PlayerListManager.Instance.AddKillById(_ID);
            }
        }
    }

    [Server]
    public void PlayHitSE(int attackerID) {
        // 被弾者
        NetworkConnectionToClient victimConn =
            GetComponent<NetworkIdentity>().connectionToClient;

        // 攻撃者（名前から取得）
        NetworkConnectionToClient attackerConn =
            GetConnectionByPlayerName(attackerID);

        if (attackerConn != null)
            AudioManager.Instance.CmdPlayUISE("hit", attackerConn);

        if (victimConn != null)
            AudioManager.Instance.CmdPlayUISE("hit", victimConn);
    }

    [Server]
    private NetworkConnectionToClient GetConnectionByPlayerName(int playerID) {
        foreach (var conn in NetworkServer.connections.Values) {
            if (conn.identity == null) continue;

            var chara = conn.identity.GetComponent<CharacterBase>();
            if (chara == null) continue;

            if (chara.parameter.playerId == playerID)
                return conn;
        }
        return null;
    }

}
