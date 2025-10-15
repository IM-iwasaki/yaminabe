using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager :NetworkBehaviour {
    public static GameSceneManager instance = null;
    [SerializeField, Header("読み込むロビーシーンの名前")]
    private string lobbySceneName = "LobbyScene";
    [SerializeField,Header("読み込むゲームシーンの名前")]
    private string gameSceneName = "GameScene";

    private bool isChanged = false;
    public override void OnStartServer() {
        base.OnStartServer();
        instance = this;

        Debug.Log(netIdentity.isServer + "," + netId);
    }
    

    public void LoadScene(string _sceneName) {
        //重ねるシーンをロード
        CustomNetworkManager.singleton.ServerChangeScene(_sceneName);
        isChanged = true;
    }

    /// <summary>
    /// 特定のシーンに移行する(GameScene)
    /// プレイヤーとカメラの同期がなくなってるので切り替えたタイミングで
    /// OnSceneChanged()を呼ぶ
    /// ホストが特定のタイミングで呼び出す
    /// </summary>
    public void LoadGameSceneForAll() {
        if(!isChanged)
        LoadScene(gameSceneName);
    }

    /// <summary>
    /// 特定のシーンに移行する(LobbyScene)
    /// </summary>
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
