using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager :NetworkSystemObject<GameSceneManager> {
    private string oldSceneName = null;
    
    [SerializeField, Header("読み込むロビーシーンの名前")]
    private string lobbySceneName = "LobbyScene";
    [SerializeField,Header("読み込むゲームシーンの名前")]
    private string gameSceneName = "GameScene";
    

    
    [Server]
    public void LoadScene(string _sceneName) {
        //ロードする前(現在)のシーンの名前をキャッシュ
        oldSceneName = SceneManager.GetActiveScene().name;
        //重ねるシーンをロード
        SceneManager.LoadSceneAsync(_sceneName, LoadSceneMode.Additive);
    }

    [Server]
    public void UnLoadScene(string _sceneName) {
        //ロード時にキャッシュしたシーンを解放
        SceneManager.UnloadSceneAsync(_sceneName);
    }

    /// <summary>
    /// 全てのクライアントに対してゲームシーンを重ね、前のシーンを解放する
    /// ホストが特定のタイミングで呼び出す
    /// </summary>
    [ClientRpc]
    public void LoadGameSceneForAll() {

        LoadScene(gameSceneName);
        UnLoadScene(oldSceneName);
    }

    [ClientRpc]
    public void LoadLobbySceneForAll() {
        LoadScene(lobbySceneName);
        UnLoadScene(oldSceneName);
    }


}
