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
    /// ホスト専用UI
    /// </summary>
    [SerializeField]
    private HostUI hostUI = null;

    /// <summary>
    /// タイトルシーンから移動してきたときに通る処理
    /// </summary>
    public override void Start() {
#if DEBUG
        if (TitleManager.instance == null) {
            base.Start();
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
    }

    /// <summary>
    /// クライアント開始時
    /// </summary>
    public override void OnStartClient() {
        base.OnStartClient();
        if (NetworkServer.active) {
            GameObject uiRoot = GameObject.Find("GameUI");
            HostUI host = Instantiate(hostUI,uiRoot.transform);
            hostUI = host;
            hostUI.Init();
        }

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
        if (!serverManager.connectPlayer.Contains(_conn.identity))
            serverManager.connectPlayer.Add(_conn.identity);
        ChatManager.Instance.CmdSendSystemMessage(serverManager.connectPlayer.Count + "is Connected ");
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

        //ローカルクライアントが抜けた場合
        if (_conn.connectionId > 0) {
            //参加者全員に通知
            ChatManager.Instance.CmdSendSystemMessage("Leave Player");
            if (_conn.identity != null)
                serverManager.connectPlayer.Remove(_conn.identity);

            base.OnServerDisconnect(_conn);
            return;
        }
    }
    /// <summary>
    /// シーンが変わった時に発火
    /// 主にルール系の変更とかを担当させるべき
    /// </summary>
    /// <param name="newSceneName"></param>
    public override void OnServerChangeScene(string newSceneName) {
        if (newSceneName == GameSceneManager.Instance.gameSceneName) {
            if (HostUI.isVisibleUI)
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
            int stageIndex = Mathf.Abs(hostUI.stageIndex % StageManager.Instance.stages.Count);
            GameManager.Instance.StartGame(RuleManager.Instance.currentRule, StageManager.Instance.stages[stageIndex]);
            // 全クライアントに送る
            CountdownManager.Instance.SendCountdown(3);
        }
        //プレイヤー1人1人をチーム毎のリスポーン地点に移動させる
        foreach (var conn in serverManager.connectPlayer) {
            //必要な変数をキャッシュ
            GeneralCharacter character = conn.GetComponent<GeneralCharacter>();
            int teamID = character.parameter.TeamID;
            NetworkTransformHybrid startPos = character.GetComponent<NetworkTransformHybrid>();
            //ゲームシーンなら指定のリスポーン箇所を取得し、転送
            if (sceneName == GameSceneManager.Instance.gameSceneName) {

                //各リスポーン地点に転送
                if (RuleManager.Instance.currentRule == GameRuleType.DeathMatch) {
                    teamID = -1;
                    character.parameter.TeamID = teamID;
                }
                    
                var RespawnPos = StageManager.Instance.GetTeamSpawnPoints((TeamColor)teamID);
                startPos.ServerTeleport(RespawnPos[Random.Range(0, RespawnPos.Count)].position, Quaternion.identity);
            }
            //ロビーシーンなら開始地点に転送
            else if (sceneName == GameSceneManager.Instance.lobbySceneName) {
                //重なることを考慮してランダムで座標をずらす
                Vector3 respawnPos = new Vector3(Random.Range(1, serverManager.connectPlayer.Count), 5, 0);
                startPos.ServerTeleport(respawnPos, Quaternion.identity);
                //レートの数値を反映して表示
                RateDisplay.instance.ChangeRateUI();
            }
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
        base.OnStopClient();
        Cursor.lockState = CursorLockMode.None;
        Destroy(gameObject);
        SceneManager.LoadScene("TitleScene");
    }

    public override void OnClientDisconnect() {
        base.OnClientDisconnect();

    }

    /// <summary>
    /// アプリ終了時の解放処理
    /// </summary>
    public override void OnApplicationQuit() {
        // サーバー or クライアントとして接続中なら安全に終了
        if (NetworkServer.active || NetworkClient.isConnected) {
            StopHost();
        }
    }


    public override void OnStopHost() {
        base.OnStopHost();
        Destroy(gameObject);
    }
}
