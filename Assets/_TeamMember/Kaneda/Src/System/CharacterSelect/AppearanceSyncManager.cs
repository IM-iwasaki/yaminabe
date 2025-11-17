using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppearanceSyncManager : NetworkBehaviour
{
    //  インスタンス化
    public static AppearanceSyncManager instance;

    //  ここでインスタンス化
    private void Awake() {
        instance = this;
    }

    /// <summary>
    /// 見た目をプレイヤーの固有IDごとに保存する関数
    /// </summary>
    /// <param name="netId"></param>
    /// <param name="characterNo"></param>
    /// <param name="skinNo"></param>
    [Server]
    public void RecordAppearance(uint netId, int characterNo, int skinNo) {
        AppearanceDataHolder.instance.states[netId] = (characterNo, skinNo);
    }

    /// <summary>
    /// クライアントからサーバーへ伝達
    /// </summary>
    [Command(requiresAuthority = false)]
    public void CmdRequestAllStates() {
        RpcSendAllStates();
    }

    /// <summary>
    /// id、番号を割り当てる
    /// </summary>
    [ClientRpc]
    public void RpcSendAllStates() {
        foreach (var kv in AppearanceDataHolder.instance.states) {
            uint id = kv.Key;
            int c = kv.Value.characterNo;
            int s = kv.Value.skinNo;

            // 後参加クライアント側で PlayerChange を適用
            RpcApplyAppearanceToOne(id, c, s);
        }
    }

    /// <summary>
    /// クライアント側で見た目を適用
    /// </summary>
    [ClientRpc]
    void RpcApplyAppearanceToOne(uint netId, int charNo, int skinNo) {
        // すでに辞書に登録済みなら取得、なければ検索して登録
        if (!AppearanceDataHolder.instance.clientPlayers.TryGetValue(netId, out GameObject playerObj)) {
            NetworkIdentity identity = FindIdentity(netId);
            if (identity != null) playerObj = identity.gameObject;
            AppearanceDataHolder.instance.clientPlayers[netId] = playerObj;
        }

        // null チェック後に適用
        if (playerObj == null) {
            Debug.LogWarning($"netId {netId} が見つかりません。デフォルトデータを適用します。");
            return;
        }
        AppearanceChangeManager.instance.PlayerChange(playerObj, charNo, skinNo, true);
    }

    /// <summary>
    /// クライアント側で netId から NetworkIdentity を検索
    /// </summary>
    private NetworkIdentity FindIdentity(uint netId) {
        foreach (var ni in NetworkClient.spawned.Values) {
            if (ni.netId == netId) return ni;
        }
        Debug.LogWarning($"NetId {netId} が見つかりません");
        return null;
    }

}
