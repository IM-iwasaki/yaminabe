using Mirror;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

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
