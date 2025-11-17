using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using static TeamData;
/// <summary>
/// 元あるNetworkManagerの派生クラス
/// </summary>
public class CustomNetworkManager : NetworkManager {
    /// <summary>
    /// シーンにあるサーバーマネージャー
    /// </summary>
    [SerializeField]
    private ServerManager serverManager = null;

    /// <summary>
    /// タイトルシーンから移動してきたときに通る処理
    /// </summary>
    public override void Awake() {
#if DEBUG
        if (TitleManager.instance == null) {
            base.Awake();
            return;
        }
#endif
        if (TitleManager.instance.isHost) {
            //ホストとして開始
            StartHost();
        }
        else if (TitleManager.instance.isClient) {
            //クライアントとして開始
            networkAddress = TitleManager.instance.ipAddress;
            StartClient();
        }
        //サーバー参加時にカーソルロック
        Cursor.lockState = CursorLockMode.Locked;
    }
    /// <summary>
    /// サーバー開始時処理
    /// </summary>
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
        ChatManager.instance.CmdSendSystemMessage(serverManager.connectPlayer.Count + "is Connected ");
    }

    /// <summary>
    /// クライアントが参加した時の処理
    /// </summary>
    public override void OnClientConnect() {
        base.OnClientConnect();
        if (TitleManager.instance.isClient) {
            Destroy(FindObjectOfType<UDPBroadcaster>().gameObject);
        }
            
    }

    /// <summary>
    /// オーバーライドしたOnServerDisconnect
    /// クライアントが抜けたタイミングでconnectPlayerからRemoveする
    /// </summary>
    /// <param name="_conn"></param>
    public override void OnServerDisconnect(NetworkConnectionToClient _conn) {
        base.OnServerDisconnect(_conn);
        //ローカルクライアントが抜けた場合
        if (!NetworkServer.localConnection.Equals(_conn)) {
            //参加者全員に通知
            ChatManager.instance.CmdSendSystemMessage("Leave Player");
            return;
        }
        GameSceneManager.Instance.LoadTitleSceneForAll();
    }
    /// <summary>
    /// シーンが変わった時に発火
    /// 主にルール系の変更とかを担当させるべき
    /// </summary>
    /// <param name="newSceneName"></param>
    public override void OnServerChangeScene(string newSceneName) {
        if (newSceneName == GameSceneManager.Instance.gameSceneName) {
            if(HostUI.isVisibleUI)
            HostUI.ShowOrHideUI();
            GameSceneManager.Instance.ResetIsChangedScene();
        }
        Cursor.lockState = HostUI.isVisibleUI ? CursorLockMode.None : CursorLockMode.Locked;
    }

    /// <summary>
    /// シーンが完全に切り替わってから呼ばれる関数、主にゲームスタートを担う
    /// </summary>
    /// <param name="sceneName"></param>
    public override void OnServerSceneChanged(string sceneName) {
        //ゲームシーンに遷移したならゲームスタート
        if (sceneName == GameSceneManager.Instance.gameSceneName) {
            HostUI hostUi = FindObjectOfType<HostUI>();
            int stageIndex = Mathf.Abs(hostUi.stageIndex % StageManager.Instance.stages.Count);
            GameManager.Instance.StartGame(RuleManager.Instance.currentRule, StageManager.Instance.stages[stageIndex]);
        }
        //プレイヤー1人1人をチーム毎のリスポーン地点に移動させる
        foreach (var conn in serverManager.connectPlayer) {
            //必要な変数をキャッシュ
            GeneralCharacter character = conn.GetComponent<GeneralCharacter>();
            int teamID = character.TeamID;
            NetworkTransformHybrid startPos = character.GetComponent<NetworkTransformHybrid>();
            //ゲームシーンなら指定のリスポーン箇所を取得し、転送
            if (sceneName == GameSceneManager.Instance.gameSceneName) {

                //各リスポーン地点に転送
                if (RuleManager.Instance.currentRule == GameRuleType.DeathMatch)
                    teamID = -1;
                var RespawnPos = StageManager.Instance.GetTeamSpawnPoints((TeamColor)teamID);
                startPos.ServerTeleport(RespawnPos[Random.Range(0, RespawnPos.Count)].position, Quaternion.identity);
            }
            //ロビーシーンなら開始地点に転送
            else if (sceneName == GameSceneManager.Instance.lobbySceneName) {
                //重なることを考慮してランダムで座標をずらす
                Vector3 respawnPos = new Vector3(Random.Range(1,10), 0, 0);
                startPos.ServerTeleport(respawnPos, Quaternion.identity);
            }
            //初期化
            character.Initalize();

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
        if (GameSceneManager.Instance)
            GameSceneManager.Instance.ResetIsChangedScene();
    }

    /// <summary>
    /// クライアントが止まった時の処理
    /// </summary>
    public override void OnStopClient() {
        SceneManager.LoadScene("TitleScene");
    }
}
