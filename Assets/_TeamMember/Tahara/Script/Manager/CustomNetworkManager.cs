using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static TeamData;

public class CustomNetworkManager : NetworkManager {
    [SerializeField]
    private ServerManager serverManager = null;

    public override void Awake() {
        if (TitleManager.instance == null) {
            base.Awake();
            return;
        }
        if (TitleManager.instance.isHost)
            //ホストとして開始
            StartHost();
        else if (TitleManager.instance.isClient) {
            //クライアントとして開始
            networkAddress = TitleManager.instance.ipAddress;
            StartClient();
        }

    }

    public override void OnStartServer() {
        base.OnStartServer();
        // サーバーが起動したタイミングで SystemManager に Network 系の Spawn を任せる
        if (SystemManager.Instance != null) {
            SystemManager.Instance.SpawnNetworkSystems();
        }
        else {
            Debug.LogWarning("SystemManager が見つかりません。SystemManager は最初のシーンに配置しておいてください。");
        }
        //起動時タイトルマネージャーのインスタンスが存在していたら、
        if (TitleManager.instance != null) {
            //その後は不必要なので更新しないようにする
            TitleManager.instance.enabled = false;
        }
        //生成後ロビーシーンに遷移
        //GameSceneManager.Instance.LoadLobbySceneForAll();
    }

    /// <summary>
    /// サーバーに接続したタイミングで処理される
    /// 主にサーバー接続可能人数を判定
    /// </summary>
    /// <param name="_conn"></param>
    public override void OnServerConnect(NetworkConnectionToClient _conn) {
        //もし参加人数が既定の数超えていたら
        if (NetworkServer.connections.Count >= maxConnections) {
            Debug.Log("参加人数をオーバーしています");
            _conn.Disconnect();
            return;
        }
        base.OnServerConnect(_conn);
    }

    /// <summary>
    /// オーバーライドしたOnServerAddPlayer
    /// サーバーに参加したことを伝える(具体的にはconnectPlayerに参加したタイミングでAddする)
    /// </summary>
    /// <param name="_conn"></param>
    public override void OnServerAddPlayer(NetworkConnectionToClient _conn) {

        GameObject player = Instantiate(playerPrefab);
        NetworkServer.AddPlayerForConnection(_conn, player);

        serverManager.connectPlayer.Add(_conn.identity);
        ChatManager.instance.CmdSendSystemMessage("Connect Player");
    }

    /// <summary>
    /// オーバーライドしたOnServerDisconnect
    /// クライアントが抜けたタイミングでconnectPlayerからRemoveする
    /// </summary>
    /// <param name="_conn"></param>
    public override void OnServerDisconnect(NetworkConnectionToClient _conn) {
        serverManager.connectPlayer.Remove(_conn.identity);
        base.OnServerDisconnect(_conn);
        Debug.Log("サーバーが切断されました！");
        //Destroy(TitleManager.instance.gameObject);
        if (!NetworkServer.localConnection.Equals(_conn)) {
            ChatManager.instance.CmdSendSystemMessage("Leave Player");
            return;
        }
        SceneManager.LoadScene("TitleScene");
    }
    /// <summary>
    /// シーンが変わった時に発火
    /// 主にルール系の変更とかを担当させるべき
    /// </summary>
    /// <param name="newSceneName"></param>
    public override void OnServerChangeScene(string newSceneName) {
        if (newSceneName == GameSceneManager.Instance.gameSceneName) {
            HostUI.isVisibleUI = false;
            HostUI.ShowOrHideUI(false);


            GameSceneManager.Instance.ResetIsChangedScene();
        }

    }

    /// <summary>
    /// シーンが完全に切り替わってから呼ばれる関数、主にゲームスタートを担う
    /// </summary>
    /// <param name="sceneName"></param>
    public override void OnServerSceneChanged(string sceneName) {
        //ゲームシーンに遷移したならゲームスタート
        if (sceneName == GameSceneManager.Instance.gameSceneName)
            GameManager.Instance.StartGame(RuleManager.Instance.currentRule, StageManager.Instance.stages[(int)RuleManager.Instance.currentRule]);
        //プレイヤー1人1人をチーム毎のリスポーン地点に移動させる
        foreach (var conn in serverManager.connectPlayer) {
            //必要な変数をキャッシュ
            CharacterBase character = conn.GetComponent<CharacterBase>();
            int teamID = character.TeamID;
            NetworkTransformHybrid startPos = character.GetComponent<NetworkTransformHybrid>();
            //ゲームシーンなら指定のリスポーン箇所を取得し、転送
            if (sceneName == GameSceneManager.Instance.gameSceneName) {
                //各リスポーン地点に転送
                if (RuleManager.Instance.currentRule == GameRuleType.DeathMatch)
                    teamID = -1;
                var RespawnPos = StageManager.Instance.GetTeamSpawnPoints((teamColor)teamID);
                startPos.ServerTeleport(RespawnPos[Random.Range(0, RespawnPos.Count)].position, Quaternion.identity);
            }
            //ロビーシーンなら開始地点(0,0,0)に転送
            else if (sceneName == GameSceneManager.Instance.lobbySceneName) {
                Transform position = conn.transform;
                position.position = new Vector3(0, 0, 0);
                startPos.ServerTeleport(position.position, Quaternion.identity);
            }


        }
        FadeManager.Instance.StartFadeIn(0.5f);
    }

    /// <summary>
    /// シーンが変わった時に
    /// </summary>
    /// <param name="newSceneName"></param>
    /// <param name="sceneOperation"></param>
    /// <param name="customHandling"></param>
    public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling) {
        base.OnClientChangeScene(newSceneName, sceneOperation, customHandling);
        FadeManager.Instance.StartFadeIn(0.5f);
        GameSceneManager.Instance.ResetIsChangedScene();
    }

    //public override void OnStopServer() {
    //    GameSceneManager.Instance.LoadTitleSceneForAll();
    //}

    public override void OnStopClient() {
        SceneManager.LoadScene("TitleScene");
    }
}
