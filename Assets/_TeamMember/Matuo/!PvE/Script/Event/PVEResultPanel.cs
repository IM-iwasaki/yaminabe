using UnityEngine;
using Mirror;

public class PVEResultPanel : MonoBehaviour {

    public void OnClickRematch() {
        if (!NetworkServer.active) return;

        RuleManager.Instance?.Initialize();
        GameManager.Instance.EndGame();
        //プレイヤーの状態を戻す
        ServerManager.instance.ResetCharacterStatus();
        GameSceneManager.Instance.LoadPvESceneForAll();
        GameManager.Instance.StartPveGameFromList(false);
        PlayerListManager.Instance.ResetAllScores();
    }

    public void OnClickReturnLobby() {
        if (!NetworkServer.active) return;

        RuleManager.Instance?.Initialize();
        GameManager.Instance.EndGame();
        //プレイヤーの状態を戻す
        ServerManager.instance.ResetCharacterStatus();
        GameSceneManager.Instance.LoadLobbySceneForAll();
        PlayerListManager.Instance.ResetAllScores();
    }
}