using Mirror;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameSceneManager : NetworkSystemObject<GameSceneManager> {
    //public static GameSceneManager instance = null;
    [Header("読み込むロビーシーンの名前")]
    public string lobbySceneName;
    [Header("読み込むゲームシーンの名前")]
    public string gameSceneName;
    [Header("読み込むゲームシーンの名前")]
    public string titleSceneName;

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
            FadeManager.Instance.StartFadeOut(0.5f);
            NetworkSceneTransitionSystem.Instance.ChangeScene(gameSceneName);
        }

    }
    [ClientRpc]
    public void LoadAndActivateScene(string _sceneName, string _prevSceneName) {
        StartCoroutine(LoadSceneAndActivate(_sceneName, _prevSceneName));
    }

    private IEnumerator LoadSceneAndActivate(string _sceneName, string _prevSceneName) {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(_sceneName, LoadSceneMode.Additive);
        FadeManager.Instance.StartFadeOut(0.5f);
        // ロード完了まで待機
        while (!asyncLoad.isDone) {
            yield return null;
        }

        // シーンがロードされた後にアクティブに設定
        Scene loadedScene = SceneManager.GetSceneByName(_sceneName);
        if (loadedScene.IsValid() && loadedScene.isLoaded) {
            SceneManager.SetActiveScene(loadedScene);
            Debug.Log($"Scene '{_sceneName}' is now active.");

        }
        else {
            Debug.LogError($"Scene '{_sceneName}' could not be activated.");
        }
        //動的にロビーのシーンを解放
        SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName(_prevSceneName));
        FadeManager.Instance.StartFadeIn(1.0f);
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
    [Server]
    public void LoadTitleSceneForAll() {
        if (!isChanged) {
            isChanged = true;
            FadeManager.Instance.StartFadeOut(0.5f);
            NetworkSceneTransitionSystem.Instance.ChangeScene(titleSceneName);
        }
    }


    public void ResetIsChangedScene() {
        isChanged = false;
    }

}
