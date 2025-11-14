using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppearanceSyncManager : NetworkSystemObject<AppearanceSyncManager>
{

    //  サーバー側が全プレイヤーの見た目を保持する
    private Dictionary<uint, (int characterNo, int skinNo)> states = new Dictionary<uint, (int, int)>();

    // クライアント側で netId → GameObject を保持
    private Dictionary<uint, GameObject> clientPlayers = new Dictionary<uint, GameObject>();

    [Header("キャラ選択用のオブジェクト")]
    [SerializeField] private SelectObjectManager selectObj = null;

    /// <summary>
    /// 見た目をプレイヤーの固有IDごとに保存する関数
    /// </summary>
    /// <param name="netId"></param>
    /// <param name="characterNo"></param>
    /// <param name="skinNo"></param>
    [Server]
    public void RecordAppearance(uint netId, int characterNo, int skinNo) {
        states[netId] = (characterNo, skinNo);
    }

    protected override void OnClientInitialized() {
        TargetSendAllStates();
    }


    [TargetRpc]
    public void TargetSendAllStates() {
        foreach (var kv in states) {
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
        if (!clientPlayers.TryGetValue(netId, out GameObject playerObj)) {
            NetworkIdentity identity = FindIdentity(netId);
            if (identity != null) playerObj = identity.gameObject;
            clientPlayers[netId] = playerObj;
        }

        // null チェック後に適用
        if (playerObj != null && selectObj != null) {
            selectObj.PlayerChange(playerObj, charNo, skinNo, true);
        }
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
