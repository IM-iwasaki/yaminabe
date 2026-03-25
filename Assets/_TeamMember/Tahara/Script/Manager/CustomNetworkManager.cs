using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
/// <summary>
/// 元あるNetworkManagerの派生クラス
/// </summary>
public class CustomNetworkManager : NetworkManager {
    /// <summary>
    /// ホスト専用UI
    /// </summary>
    [SerializeField]
    private HostUI hostUI = null;
    /// <summary>
    /// タイトルからロビーに行ったか
    /// </summary>
    private bool titleToLobby = true;
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

        //if (Application.isBatchMode) return;
        GameObject uiRoot = GameObject.Find("GameUI");
        if (NetworkServer.active) {

            HostUI host = Instantiate(hostUI, uiRoot.transform);
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
        var characterData = FindAnyObjectByType<AppearanceChangeManager>().data.characters[0];

        player.GetComponent<GeneralCharacter>().parameter.StatusInport(characterData.statusData);
        //プレイヤー追加処理
        NetworkServer.AddPlayerForConnection(_conn, player);
        if (!ServerManager.instance.connectPlayer.Contains(_conn.identity))
            ServerManager.instance.connectPlayer.Add(_conn.identity);
        
        ChatManager.Instance.CmdSendSystemMessage(ServerManager.instance.connectPlayer.Count + "is Connected ");
        ServerManager.instance.ChangeTeammateMax();
    }

    /// <summary>
    /// クライアントが参加した時の処理
    /// </summary>
    public override void OnClientConnect() {
        base.OnClientConnect();
        if (TitleManager.instance.isClient) {
            Destroy(FindObjectOfType<UDPBroadcaster>().gameObject);
        }
        LoadingUI.instance.ShowLoading();
        StartCoroutine(LoadingUI.instance.HideLoading());
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
            if (ChatManager.Instance != null)
                ChatManager.Instance.CmdSendSystemMessage("Leave Player");
            if (_conn.identity != null)
                ServerManager.instance.connectPlayer.Remove(_conn.identity);

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
        if (newSceneName == GameSceneManager.Instance.gameSceneName || newSceneName == GameSceneManager.Instance.pveSceneName) {
            if (HostUI.isVisibleUI)
                HostUI.ToggleHostUI();
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
            GameManager.Instance.StartPvpGame(RuleManager.Instance.currentRule, StageManager.Instance.stages[stageIndex]);
            // 全クライアントに送る
            //CountdownManager.Instance.SendCountdown(6);
        }
        else if (sceneName == GameSceneManager.Instance.pveSceneName) {
            GameManager.Instance.StartPveGameFromList(false); // trueにすればランダム
        }
        //プレイヤー1人1人をチーム毎のリスポーン地点に移動させる
        foreach (var playerObj in ServerManager.instance.connectPlayer) {
            //必要な変数をキャッシュ
            GeneralCharacter character = playerObj.GetComponent<GeneralCharacter>();
            var conn = playerObj.connectionToClient;
            int teamID = character.parameter.TeamID;
            NetworkTransformHybrid startPos = character.GetComponent<NetworkTransformHybrid>();
            Vector3 respawnPos;
            Vector3 bufferPos = new Vector3(Random.Range(-3.0f, 3.0f), 1.0f, Random.Range(-3.0f, 3.0f));
            //ゲームシーンなら指定のリスポーン箇所を取得し、転送
            if (sceneName == GameSceneManager.Instance.gameSceneName) {
                var RespawnPosList = StageManager.Instance.GetTeamSpawnPoints((TeamData.TeamColor)teamID);
                startPos.ServerTeleport(RespawnPosList[Random.Range(0, RespawnPosList.Count)].position + bufferPos, Quaternion.identity);
            }
            //ロビーシーンなら開始地点に転送
            else if (sceneName == GameSceneManager.Instance.lobbySceneName) {
                //重なることを考慮してランダムで座標をずらす
                respawnPos = new Vector3(Random.Range(1, ServerManager.instance.connectPlayer.Count), 5, 0);
                startPos.ServerTeleport(respawnPos + bufferPos, Quaternion.identity);
                //レートの数値を反映して表示
                RateDisplay.instance.ChangeRateUI();
                character.parameter.TargetSkillUIUpdate(conn);
            }
            //PvEシーンはスポーン位置を一か所(余裕を持たせて)に固定
            else if (sceneName == GameSceneManager.Instance.pveSceneName) {
                startPos.ServerTeleport(bufferPos, Quaternion.identity);
            }
        }
        Physics.simulationMode = SimulationMode.FixedUpdate;

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
        //タイトルからロビーだとルール確定していないのでHokoでtipsを固定
        if (titleToLobby) {
            LoadingUI.instance.ShowLoading();
            titleToLobby = false;
        }
            
        else
            LoadingUI.instance.ShowLoading(RuleManager.Instance.currentRule);
        if (GameSceneManager.Instance)
            GameSceneManager.Instance.ResetIsChangedScene();
    }

    public override void OnClientSceneChanged() {
        base.OnClientSceneChanged();

        // ロード完了後に UI を消す
        StartCoroutine(LoadingUI.instance.HideLoading());
    }

    /// <summary>
    /// クライアントが止まった時の処理
    /// </summary>
    public override void OnStopClient() {
        base.OnStopClient();
        titleToLobby = true;
        FindObjectOfType<UDPListener>()?.StopReceiveIP();
        if (!Application.isBatchMode) {
            Cursor.lockState = CursorLockMode.None;
            Destroy(gameObject);
            SceneManager.LoadScene("TitleScene");
        }
        LoadingUI.instance.ShowLoading();
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
            titleToLobby = true;
            StopHost();
        }
    }


    public override void OnStopHost() {
        base.OnStopHost();
        var udpBroadcaster = FindObjectOfType<UDPBroadcaster>();
        udpBroadcaster?.StopBroadcast();
        FindObjectOfType<UDPListener>()?.StopReceiveIP();
        if (udpBroadcaster != null)
            Destroy(udpBroadcaster.gameObject);
        Destroy(gameObject);
    }
}
