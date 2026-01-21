using Mirror;
using UnityEngine;

/// <summary>
/// ゲームシーンの管理クラス
/// </summary>
public class GameSceneManager : NetworkSystemObject<GameSceneManager> {
    [Header("読み込むシーンの名前")]
    public string lobbySceneName;
    public string gameSceneName;
    public string titleSceneName;

    /// <summary>
    /// シーン遷移が行われたかどうか
    /// </summary>
    private bool isChanged = false;


    /// <summary>
    /// 特定のシーンに移行する(GameScene)
    /// プレイヤーとカメラの同期がなくなってるので切り替えたタイミングで
    /// OnSceneChanged()を呼ぶ
    /// ホストが特定のタイミングで呼び出す
    /// </summary>
    [Server]
    public void LoadGameSceneForAll() {
        FindAnyObjectByType<UDPBroadcaster>().message.gamePlaying = true;

        //ここで準備完了かどうか判定を取って全員準備完了ならゲームシーンに移動
        foreach (var player in ServerManager.instance.connectPlayer) {
            CharacterBase readyPlayer = player.GetComponent<CharacterBase>();
            //そもそもPlayerが取れていない
            if (!readyPlayer) {
                ChatManager.Instance.CmdSendSystemMessage("Not found player Info");
                return;
            }
            //準備未完了なら
            if (!readyPlayer.parameter.ready) {
                ChatManager.Instance.CmdSendSystemMessage(player.GetComponent<CharacterBase>().parameter.PlayerName + " is not ready");
                return;
            }
            else {
                ChatManager.Instance.CmdSendSystemMessage(player.GetComponent<CharacterBase>().parameter.PlayerName + " is ready!!");
            }
        }
        //チーム決め
        ServerManager.instance.RandomTeamDecide();

        //フェードアウト
        if (!isChanged) {
            isChanged = true;
            NetworkSceneTransitionSystem.Instance.ChangeScene(gameSceneName);
        }
    }

    /// <summary>
    /// 特定のシーンに全員を移行する(LobbyScene)
    /// </summary>
    [Server]
    public void LoadLobbySceneForAll() {
        if (!isChanged) {
            isChanged = true;
            FadeManager.Instance.StartFadeOut(0.5f);
            NetworkSceneTransitionSystem.Instance.ChangeScene(lobbySceneName);
            FindAnyObjectByType<UDPBroadcaster>().message.gamePlaying = false;
        }
    }

    /// <summary>
    /// 全員をタイトルシーンに戻す処理
    /// </summary>
    [Server]
    public void LoadTitleSceneForAll() {
        if (!isChanged) {
            isChanged = true;
            FadeManager.Instance.StartFadeOut(0.5f);
            NetworkSceneTransitionSystem.Instance.ChangeScene(titleSceneName);
        }
    }

    /// <summary>
    /// シーン遷移したという状態をリセットする関数
    /// 重複ロードを回避
    /// </summary>
    public void ResetIsChangedScene() {
        isChanged = false;
    }

}
