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
    private readonly List<Transform> normalRespawnPoints = new();
    private readonly List<Transform> redRespawnPoints = new();
    private readonly List<Transform> blueRespawnPoints = new();

    // 現在のリスポーンモード
    private RespawnMode currentRespawnMode = RespawnMode.Team;

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
    public IReadOnlyList<Transform> GetTeamSpawnPoints(TeamData.teamColor team) {
        return team switch {
            TeamData.teamColor.Red => redRespawnPoints,
            TeamData.teamColor.Blue => blueRespawnPoints,
            _ => normalRespawnPoints
        };
    }

    /// <summary>
    /// 現在のモードに応じてスポーン地点を1つ取得
    /// （デスマッチなら共通ランダム、チーム戦ならチーム専用を使用）
    /// </summary>
    public Transform GetSpawnPoint(TeamData.teamColor team) {
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