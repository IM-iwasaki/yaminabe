using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// OnClientInitializedを呼び出す用
/// </summary>
public class SendAppearanceSyncManager : NetworkSystemObject<SendAppearanceSyncManager>
{

    /// <summary>
    /// クライアントが入った際に呼び出す
    /// </summary>
    protected override void OnClientInitialized() {
        if (!NetworkClient.active && NetworkServer.active) return;

        //  即時呼びを防ぐ
        StartCoroutine(RequestWhenReady());
    }

    /// <summary>
    /// 準備が整うまで待つ
    /// </summary>
    /// <returns></returns>
    private IEnumerator RequestWhenReady() {
        // AppearanceSyncManager のインスタンス生成待ち
        while (AppearanceSyncManager.instance == null)
            yield return null;

        // クライアントのローカルプレイヤー生成待ち
        while (NetworkClient.localPlayer == null)
            yield return null;

        // サーバーへ全外見データ要求
        AppearanceSyncManager.instance.CmdRequestAllStates();
    }
}
