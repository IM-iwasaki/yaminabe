using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager :NetworkBehaviour {
    private string oldSceneName = null;

    public static GameSceneManager instance = null;
    [SerializeField, Header("読み込むロビーシーンの名前")]
    private string lobbySceneName = "LobbyScene";
    [SerializeField,Header("読み込むゲームシーンの名前")]
    private string gameSceneName = "GameScene";

    public override void OnStartServer() {
        base.OnStartServer();
        instance = this;

        Debug.Log(netIdentity.isServer + "," + netId);
    }
    
    public void LoadScene(string _sceneName) {
        //重ねるシーンをロード
        SceneManager.LoadSceneAsync(_sceneName, LoadSceneMode.Additive);
        CustomNetworkManager.singleton.ServerChangeScene(_sceneName);
    }

    /// <summary>
    /// 全てのクライアントに対してゲームシーンを重ね、前のシーンを解放する
    /// ホストが特定のタイミングで呼び出す
    /// </summary>
    [ClientRpc]
    public void LoadGameSceneForAll() {
        LoadScene(gameSceneName);
    }

    [ClientRpc]
    public void LoadLobbySceneForAll() {
        LoadScene(lobbySceneName);
    }
    //private IEnumerator LoadGameSceneRoutine() {
    //    oldSceneName = SceneManager.GetActiveScene().name;

    //    AsyncOperation loadOp = SceneManager.LoadSceneAsync(gameSceneName, LoadSceneMode.Additive);
    //    yield return new WaitUntil(() => loadOp.isDone);

    //    SceneManager.SetActiveScene(SceneManager.GetSceneByName(gameSceneName));

    //    AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(oldSceneName);
    //    yield return new WaitUntil(() => unloadOp.isDone);
    //}


}
