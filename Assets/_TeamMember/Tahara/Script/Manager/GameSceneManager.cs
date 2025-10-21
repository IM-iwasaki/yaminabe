using Mirror;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameSceneManager : NetworkSystemObject<GameSceneManager> {
    //public static GameSceneManager instance = null;
    [SerializeField, Header("読み込むロビーシーンの名前")]
    private string lobbySceneName = "LobbyScene";
    [SerializeField, Header("読み込むゲームシーンの名前")]
    private string gameSceneName = "GameScene";

    private bool isChanged = false;
    public override void OnStartServer() {
        base.OnStartServer();

        Debug.Log(netIdentity.isServer + "," + netId);
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 特定のシーンに移行する(GameScene)
    /// プレイヤーとカメラの同期がなくなってるので切り替えたタイミングで
    /// OnSceneChanged()を呼ぶ
    /// ホストが特定のタイミングで呼び出す
    /// </summary>
    //[ClientRpc]
    public void LoadGameSceneForAll() {
        //フェードアウト
        if (!isChanged) {
            isChanged = true;
            FadeManager.Instance.StartFadeOut(0.5f);
            LoadAndActivateScene(gameSceneName);
        }
        FadeManager.Instance.StartFadeIn(0.5f);
    }
    
    public void LoadAndActivateScene(string sceneName) {
        StartCoroutine(LoadSceneAndActivate(sceneName));
    }

    private IEnumerator LoadSceneAndActivate(string sceneName) {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        // ロード完了まで待機
        while (!asyncLoad.isDone) {
            yield return null;
        }

        // シーンがロードされた後にアクティブに設定
        Scene loadedScene = SceneManager.GetSceneByName(sceneName);
        if (loadedScene.IsValid() && loadedScene.isLoaded) {
            SceneManager.SetActiveScene(loadedScene);
            Debug.Log($"Scene '{sceneName}' is now active.");
            
        }
        else {
            Debug.LogError($"Scene '{sceneName}' could not be activated.");
        }
        SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName(lobbySceneName));
    }


    /// <summary>
    /// 特定のシーンに全員を移行する(LobbyScene)
    /// </summary>
    public void LoadLobbySceneForAll() {
        if (!isChanged) {
            isChanged = true;
            FadeManager.Instance.StartFadeOut(0.5f);
            NetworkSceneTransitionSystem.Instance.ChangeScene(lobbySceneName);
        }
    }
    public void ResetIsChangedScene() {
        isChanged = false;
    }

}
