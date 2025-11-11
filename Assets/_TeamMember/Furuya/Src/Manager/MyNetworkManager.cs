using Mirror;
using UnityEngine;

/// <summary>
/// ネットワークマネージャーの拡張
/// </summary>
public class MyNetworkManager : NetworkManager {
    public override void OnStartServer() {
        base.OnStartServer();
        // サーバーが起動したタイミングで SystemManager に Network 系の Spawn を任せる
        if (SystemManager.Instance != null) {
            SystemManager.Instance.SpawnNetworkSystems();
        }
        else {
            Debug.LogWarning("SystemManager が見つかりません。SystemManager は最初のシーンに配置しておいてください。");
        }
    }
}