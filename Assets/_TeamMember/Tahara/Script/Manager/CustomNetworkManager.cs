using Mirror;
using UnityEngine;

public class CustomNetworkManager : NetworkManager
{
    [SerializeField]
    private ServerManager serverManager = null;

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

    /// <summary>
    /// サーバーに接続したタイミングで処理される
    /// 主にサーバー接続可能人数を判定
    /// </summary>
    /// <param name="_conn"></param>
    public override void OnServerConnect(NetworkConnectionToClient _conn) {
        //もし参加人数が既定の数超えていたら
        if(NetworkServer.connections.Count >= maxConnections) {
            Debug.Log("参加人数をオーバーしています");
            _conn.Disconnect();
            return;
        }
        base.OnServerConnect(_conn);
    }

    /// <summary>
    /// オーバーライドしたOnServerAddPlayer
    /// サーバーに参加したことを伝える(具体的にはconnectPlayerに参加したタイミングでAddする)
    /// </summary>
    /// <param name="_conn"></param>
    public override void OnServerAddPlayer(NetworkConnectionToClient _conn) {
        
        GameObject player = Instantiate(playerPrefab);
        NetworkServer.AddPlayerForConnection(_conn, player);

        serverManager.connectPlayer.Add(_conn.identity);
    }

    /// <summary>
    /// オーバーライドしたOnServerDisconnect
    /// クライアントが抜けたタイミングでconnectPlayerからRemoveする
    /// </summary>
    /// <param name="_conn"></param>
    public override void OnServerDisconnect(NetworkConnectionToClient _conn) {
        serverManager.connectPlayer.Remove(_conn.identity);
        base.OnServerDisconnect(_conn);
        
    }

    public override void OnServerChangeScene(string newSceneName) {
        base.OnServerChangeScene(newSceneName);

        GameSceneManager.instance.ResetIsChangedScene();
    }
}
