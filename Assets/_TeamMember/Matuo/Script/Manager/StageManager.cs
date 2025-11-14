using UnityEngine;
using Mirror;
using System.Collections.Generic;

/// <summary>
/// ステージ生成とリスポーン地点管理
/// </summary>
public class StageManager : NetworkSystemObject<StageManager> {
    [Header("ステージ一覧")]
    public List<StageData> stages = new();

    private GameObject currentStageInstance;
    // リスポーン地点
    [SerializeField]private readonly SyncList<Transform> normalRespawnPoints = new();
    [SerializeField]private readonly SyncList<Transform> redRespawnPoints = new();   
    [SerializeField]private readonly SyncList<Transform> blueRespawnPoints = new();  

    // 現在のリスポーンモード
    private RespawnMode currentRespawnMode = RespawnMode.Team;

    protected override void Awake() {
        base.Awake();
        
    }

    /// <summary>
    /// ステージを生成（サーバー専用）
    /// </summary>
    [Server]
    public void SpawnStage(StageData stageData, GameRuleType rule) {
        if (stageData == null || stageData.stagePrefab == null) return;

        // 既存ステージを削除
        if (currentStageInstance != null)
            NetworkServer.Destroy(currentStageInstance);

        // ステージ生成
        currentStageInstance = Instantiate(stageData.stagePrefab);
        NetworkServer.Spawn(currentStageInstance);
        ItemSpawnManager.Instance.SetupSpawnPoint();

        // リスポーン地点登録
        RegisterRespawnPoints(currentStageInstance);

        //ルールごとに生成するオブジェクトを変更する
        RpcUpdateRuleObjects(rule);
    }

    /// <summary>
    /// 古谷　ルールごとのオブジェクト取得
    /// </summary>
    /// <param name="rule"></param>
    [ClientRpc]
    void RpcUpdateRuleObjects(GameRuleType rule) {
        ApplyRuleObjects(rule);
    }

    void ApplyRuleObjects(GameRuleType rule) {
        var areaObjects = GameObject.FindGameObjectsWithTag("AreaObject");
        var hokoObjects = GameObject.FindGameObjectsWithTag("HokoObject");
        var deathMatchObjects = GameObject.FindGameObjectsWithTag("DeathMatchObject");

        foreach (var obj in areaObjects) obj.SetActive(false);
        foreach (var obj in hokoObjects) obj.SetActive(false);
        foreach (var obj in deathMatchObjects) obj.SetActive(false);

        switch (rule) {
            case GameRuleType.Area:
                foreach (var obj in areaObjects) obj.SetActive(true);
                break;
            case GameRuleType.Hoko:
                foreach (var obj in hokoObjects) obj.SetActive(true);
                break;
            case GameRuleType.DeathMatch:
                foreach (var obj in deathMatchObjects) obj.SetActive(true);
                break;
        }
    }


    /// <summary>
    /// ステージ内のリスポーン地点をタグから登録
    /// </summary>
    [Server]
    private void RegisterRespawnPoints(GameObject stageObj) {
        normalRespawnPoints.Clear();
        redRespawnPoints.Clear();
        blueRespawnPoints.Clear();

        foreach (Transform point in stageObj.GetComponentsInChildren<Transform>(true)) {
            if (point.CompareTag("NormalRespawnPoint"))
                normalRespawnPoints.Add(point);
            else if (point.CompareTag("RedRespawnPoint"))
                redRespawnPoints.Add(point);
            else if (point.CompareTag("BlueRespawnPoint"))
                blueRespawnPoints.Add(point);
        }
    }

    /// <summary>
    /// リスポーンモード設定（サーバー側のみ）
    /// </summary>
    [Server]
    public void SetRespawnMode(RespawnMode mode) {
        currentRespawnMode = mode;
    }

    /// <summary>
    /// 現在のリスポーンモードを取得
    /// </summary>
    public RespawnMode GetRespawnMode() => currentRespawnMode;

    /// <summary>
    /// 共通リスポーン地点のリストを返す
    /// </summary>
    public IReadOnlyList<Transform> GetNormalSpawnPoints() => normalRespawnPoints;

    /// <summary>
    /// チームごとのリスポーン地点を返す
    /// </summary>
    public IReadOnlyList<Transform> GetTeamSpawnPoints(TeamData.TeamColor team) {
        return team switch {
            TeamData.TeamColor.Red => redRespawnPoints,
            TeamData.TeamColor.Blue => blueRespawnPoints,
            _ => normalRespawnPoints
        };
    }

    /// <summary>
    /// 現在のモードに応じてスポーン地点を1つ取得
    /// （デスマッチなら共通ランダム、チーム戦ならチーム専用を使用）
    /// </summary>
    public Transform GetSpawnPoint(TeamData.TeamColor team) {
        if (currentRespawnMode == RespawnMode.Random) {
            if (normalRespawnPoints.Count == 0) return null;
            return normalRespawnPoints[Random.Range(0, normalRespawnPoints.Count)];
        } else {
            var points = GetTeamSpawnPoints(team);
            if (points.Count == 0) return null;
            return points[Random.Range(0, points.Count)];
        }
    }

    /// <summary>
    /// 現在のステージを削除（サーバー専用）
    /// </summary>
    [Server]
    public void ClearStage() {
        if (currentStageInstance != null)
            NetworkServer.Destroy(currentStageInstance);

        currentStageInstance = null;
        normalRespawnPoints.Clear();
        redRespawnPoints.Clear();
        blueRespawnPoints.Clear();
    }
}

/// <summary>
/// リスポーンモード
/// </summary>
public enum RespawnMode {
    Random,
    Team
}