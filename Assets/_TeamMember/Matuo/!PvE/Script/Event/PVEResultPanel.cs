using UnityEngine;
using Mirror;

public class PVEResultPanel : MonoBehaviour {

    public void OnClickNextStage() {
        if (!NetworkServer.active) return;

        RuleManager.Instance?.Initialize();
        GameManager.Instance.EndGame();
        // プレイヤー状態リセット
        ServerManager.instance.ResetCharacterStatus();

        // 次のステージへ
        GameSceneManager.Instance.LoadPvESceneForAll();
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