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
    private readonly SyncList<Transform> normalRespawnPoints = new();
    private readonly SyncList<Transform> redRespawnPoints = new();
    private readonly SyncList<Transform> blueRespawnPoints = new();

    // 現在のリスポーンモード
    private RespawnMode currentRespawnMode = RespawnMode.Team;

    protected override void Awake() {
        base.Awake();
        
    }

    /// <summary>
    /// ステージを生成（サーバー専用）
    /// </summary>
    [Server]
    public void SpawnStage(StageData stageData) {
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
       
    }

    /// <summary>
    /// ステージ内のリスポーン地点をタグから登録
    /// </summary>
    [Server]
    private void RegisterRespawnPoints(GameObject stageObj) {
        normalRespawnPoints.Clear();
        redRespawnPoints.Clear();
        blueRespawnPoints.Clear();

        Transform RespawnRoot = GameObject.Find("RespawnPoints").transform;

        foreach (Transform point in RespawnRoot.GetComponentsInChildren<Transform>(true)) {
            if (point.CompareTag("NormalRespawnPoint"))
                normalRespawnPoints.Add(point);
            else if (point.CompareTag("RedRespawnPoint"))
                redRespawnPoints.Add(point);
            else if (point.CompareTag("BlueRespawnPoint"))
                blueRespawnPoints.Add(point);
            Debug.Log("Add point : " + point);
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