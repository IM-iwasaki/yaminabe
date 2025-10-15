using UnityEngine;
using Mirror;
using System.Collections.Generic;

/// <summary>
/// ステージ生成を管理するクラス
/// ScriptableObjectのStageDataを参照してSpawnする
/// </summary>
public class StageManager : NetworkSystemObject<StageManager> {
    [Header("ステージ一覧")]
    public List<StageData> stages = new();

    private GameObject currentStageInstance;

    /// <summary>
    /// ランダムでステージを選んで生成（サーバー専用）
    /// </summary>
    [Server]
    public void SpawnRandomStage() {
        if (stages == null || stages.Count == 0) return;


        StageData randomStage = stages[Random.Range(0, stages.Count)];
        SpawnStage(randomStage);
    }

    /// <summary>
    /// 指定ステージを生成（サーバー専用）
    /// </summary>
    [Server]
    public void SpawnStage(StageData stageData) {
        if (stageData == null || stageData.stagePrefab == null) return;

        // 既存ステージ削除
        if (currentStageInstance != null)
            NetworkServer.Destroy(currentStageInstance);

        // 新しいステージ生成
        GameObject stageObj = Instantiate(stageData.stagePrefab);
        NetworkServer.Spawn(stageObj);
        currentStageInstance = stageObj;

        Debug.Log($"ステージ生成: {stageData.stageName}");
    }

    /// <summary>
    /// 現在のステージを破棄
    /// </summary>
    [Server]
    public void ClearStage() {
        if (currentStageInstance != null) {
            NetworkServer.Destroy(currentStageInstance);
            currentStageInstance = null;
        }
    }

    /// <summary>
    /// クライアント側でも参照可能なステージ情報取得
    /// </summary>
    public StageData GetStageDataByName(string name) {
        return stages.Find(s => s.stageName == name);
    }
}