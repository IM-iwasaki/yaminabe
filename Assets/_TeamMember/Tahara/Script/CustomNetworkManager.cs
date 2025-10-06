using Mirror;
using UnityEngine;

public class CustomNetworkManager : NetworkManager
{
    [SerializeField]
    private ServerManager serverManager = null;

    public override void OnServerAddPlayer(NetworkConnectionToClient _conn) {
        //base.OnServerAddPlayer(_conn);
        GameObject player = Instantiate(playerPrefab);
        NetworkServer.AddPlayerForConnection(_conn, player);

        serverManager.connectPlayer.Add(player.GetComponent<NetworkIdentity>());
    }

    public override void OnServerDisconnect(NetworkConnectionToClient _conn) {
        base.OnServerDisconnect(_conn);
        serverManager.connectPlayer.RemoveAt(serverManager.connectPlayer.Count - 1);
    }
}
