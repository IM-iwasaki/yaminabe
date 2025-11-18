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
        // すでに登録済みならすぐ実行
        if (AppearanceDataHolder.instance.clientPlayers.TryGetValue(netId, out GameObject playerObj)) {
            //  生成
            if(playerObj != null) {
                AppearanceChangeManager.instance.PlayerChange(playerObj, charNo, skinNo, true);
                return;
            }

        }

        //  プレイヤーがスポーンされるまで待つ
        StartCoroutine(WaitGetObject(netId, (obj) => {
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
    /// プレイヤーが Spawn されるまで待機して取得
    /// </summary>
    /// <param name="netId">対象プレイヤーの netId</param>
    /// <param name="callback">取得後に呼び出す処理</param>
    /// <param name="timeout">最大待機時間（秒）。0以下で無制限待機</param>
    private IEnumerator WaitGetObject(uint netId, System.Action<GameObject> callback, float timeout = 3f) {
        float timer = timeout;
        NetworkIdentity network = null;

        while (true) {
            if (NetworkClient.spawned.TryGetValue(netId, out network)) {
                // Spawn 確認できたら即終了
                break;
            }

            if (timeout > 0f) {
                timer -= Time.deltaTime;
                if (timer <= 0f) {
                    // タイムアウト
                    network = null;
                    break;
                }
            }

            yield return null;
        }

        GameObject playerObj = network != null ? network.gameObject : null;
        callback?.Invoke(playerObj);
    }

}
