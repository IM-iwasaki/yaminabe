using UnityEngine;
using Mirror;
using System.Collections.Generic;

/// <summary>
/// ステージ生成とリスポーン地点管理
/// StageDatabase に対応し、インデックス指定でステージ生成可能
/// UI 選択もイベントで連動可能
/// </summary>
public class StageManager : NetworkSystemObject<StageManager> {
    [Header("ステージデータベース")]
    public StageDataBase stageDatabase; // ScriptableObjectでステージリスト管理

    private GameObject currentStageInstance; // 現在生成中のステージ

    // リスポーン地点
    private readonly List<Transform> normalRespawnPoints = new();
    private readonly List<Transform> redRespawnPoints = new();
    private readonly List<Transform> blueRespawnPoints = new();

    private RespawnMode currentRespawnMode = RespawnMode.Team; // デフォルト
    private int currentStageIndex = 0; // UI選択＆生成用の現在インデックス

    /// <summary>
    /// ステージ切替時にUI更新するためのイベント
    /// 引数: 選択中ステージのインデックス
    /// </summary>
    public event System.Action<int> OnStageIndexChanged;

    /// <summary>
    /// 現在のステージインデックスを取得
    /// </summary>
    public int GetCurrentStageIndex() => currentStageIndex;

    /// <summary>
    /// 現在選択中のステージ名を取得（UI表示用）
    /// </summary>
    public string GetCurrentStageName() {
        if (stageDatabase?.stages == null || stageDatabase.stages.Count == 0)
            return "None";

        StageInfo info = stageDatabase.GetStageByIndex(currentStageIndex);
        return info != null ? info.stageName : "None";
    }

    /// <summary>
    /// UIで次のステージを選択（生成はされない）
    /// </summary>
    public void SelectNextStage() {
        if (stageDatabase?.stages == null || stageDatabase.stages.Count == 0) return;
        currentStageIndex = (currentStageIndex + 1) % stageDatabase.stages.Count;
        OnStageIndexChanged?.Invoke(currentStageIndex);
    }

    /// <summary>
    /// UIで前のステージを選択（生成はされない）
    /// </summary>
    public void SelectPreviousStage() {
        if (stageDatabase?.stages == null || stageDatabase.stages.Count == 0) return;
        currentStageIndex = (currentStageIndex - 1 + stageDatabase.stages.Count) % stageDatabase.stages.Count;
        OnStageIndexChanged?.Invoke(currentStageIndex);
    }

    /// <summary>
    /// 指定インデックスでステージ生成（GameManagerから呼ぶ用）
    /// SpawnStage は必ずServerで呼ぶこと
    /// </summary>
    [Server]
    public void SpawnStage(int index) {
        if (stageDatabase?.stages == null || stageDatabase.stages.Count == 0) return;

        currentStageIndex = Mathf.Clamp(index, 0, stageDatabase.stages.Count - 1);

        StageInfo stageInfo = stageDatabase.GetStageByIndex(currentStageIndex);
        if (stageInfo?.stagePrefab == null) return;

        // 既存ステージ削除
        if (currentStageInstance != null)
            NetworkServer.Destroy(currentStageInstance);

        // ステージ生成
        currentStageInstance = Instantiate(stageInfo.stagePrefab);
        NetworkServer.Spawn(currentStageInstance);

        // リスポーン地点登録
        RegisterRespawnPoints(currentStageInstance);
    }

    /// <summary>
    /// ステージ内のリスポーンポイントをタグから登録
    /// </summary>
    private void RegisterRespawnPoints(GameObject stageObj) {
        normalRespawnPoints.Clear();
        redRespawnPoints.Clear();
        blueRespawnPoints.Clear();

        foreach (Transform point in stageObj.GetComponentsInChildren<Transform>(true)) {
            if (point.CompareTag("NormalRespawnPoint")) normalRespawnPoints.Add(point);
            else if (point.CompareTag("RedRespawnPoint")) redRespawnPoints.Add(point);
            else if (point.CompareTag("BlueRespawnPoint")) blueRespawnPoints.Add(point);
        }
    }

    /// <summary>
    /// リスポーンモード設定（サーバー専用）
    /// </summary>
    [Server]
    public void SetRespawnMode(RespawnMode mode) => currentRespawnMode = mode;
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
    /// ※この中身は絶対に変えない
    /// </summary>
    public Transform GetSpawnPoint(TeamData.teamColor team = TeamData.teamColor.Invalid) {
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