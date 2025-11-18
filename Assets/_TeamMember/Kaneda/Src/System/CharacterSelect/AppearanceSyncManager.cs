using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 後入れのクライアントに他プレイヤーの変更後の見た目を対応させる
/// </summary>
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
        if (AppearanceDataHolder.instance.clientPlayers.TryGetValue(netId, out GameObject playerObj)) {
            //  生成
            if(playerObj != null) {
                AppearanceChangeManager.instance.PlayerChange(playerObj, charNo, skinNo, true);
                return;
            }

        }

        //  プレイヤーがスポーンされるまで最大1秒待つ
        StartCoroutine(WaitGetObjct(netId, (obj) => {
            // nullだった場合はデフォルト読み込み
            if (obj == null) {
                Debug.LogWarning($"netId {netId} が見つかりません。デフォルトデータを適用します。");
                return;
            }

            //  辞書に登録
            AppearanceDataHolder.instance.clientPlayers[netId] = obj;

            //  見た目を適応する
            AppearanceChangeManager.instance.PlayerChange(obj, charNo, skinNo, true);
        }));
    }

    /// <summary>
    /// 最大一秒待ちのオブジェクト取得
    /// </summary>
    /// <param name="netId"></param>
    /// <param name="callback"></param>
    /// <returns></returns>
    private IEnumerator WaitGetObjct(uint netId, System.Action<GameObject> callback) {
        GameObject playerObj = null;
        float timer = 1f;
        while (timer > 0) {
            if(NetworkClient.spawned.TryGetValue(netId, out NetworkIdentity network)) {
                playerObj = network.gameObject;
                break;
            }
            timer -= Time.deltaTime;
            yield return null;
        }

        callback?.Invoke(playerObj);
    }

}
