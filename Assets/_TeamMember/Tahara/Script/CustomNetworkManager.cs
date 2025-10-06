using Mirror;
using UnityEngine;

public class CustomNetworkManager : NetworkManager
{
    [SerializeField]
    private ServerManager serverManager = null;

    public override void OnServerAddPlayer(NetworkConnectionToClient _conn) {
        base.OnServerAddPlayer(_conn);
        GameObject player = Instantiate(playerPrefab);
        NetworkServer.AddPlayerForConnection(_conn, player);

        ServerManager.instance.connectPlayer.Add(player.GetComponent<NetworkIdentity>());

    }
}
