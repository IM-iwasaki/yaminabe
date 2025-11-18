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
        if (NetworkClient.active && !NetworkServer.active) {
            //  nullだった場合スキップ
            if (AppearanceSyncManager.instance == null) return;
            //  クライアントからサーバーへ伝達
            AppearanceSyncManager.instance.CmdRequestAllStates();
        }
    }
}
