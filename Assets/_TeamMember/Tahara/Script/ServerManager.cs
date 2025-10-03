using Mirror;
using UnityEngine;

/// <summary>
/// ServerManager
/// サーバー側での処理を管理するクラス(Playerのステータス更新やオブジェクトの生成等)
/// </summary>
public class ServerManager : NetworkSystemObject<ServerManager> {
    protected override void Awake() {
        base.Awake();
        GetComponent<NetworkIdentity>().serverOnly = true;
    }
}
