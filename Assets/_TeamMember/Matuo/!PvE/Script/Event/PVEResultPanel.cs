using UnityEngine;
using Mirror;

public class PVEResultPanel : MonoBehaviour {

    public void OnClickRematch() {
        if (!NetworkServer.active) return;

        GameManager.Instance.StartPveGameFromList(false);
    }

    public void OnClickReturnLobby() {
        if (!NetworkServer.active) return;

        NetworkManager.singleton.ServerChangeScene("LobbyScene");
    }
}