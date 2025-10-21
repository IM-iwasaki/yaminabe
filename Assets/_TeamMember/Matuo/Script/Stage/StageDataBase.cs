using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "StageDatabase", menuName = "Stage/StageDatabase")]
public class StageDataBase : ScriptableObject {
    [Header("ステージリスト")]
    public List<StageInfo> stages = new();

    /// <summary>
    /// インデックスからステージデータを取得
    /// </summary>
    public StageInfo GetStageByIndex(int index) {
        if (index < 0 || index >= stages.Count) return null;
        return stages[index];
    }

    /// <summary>
    /// ステージ名からステージデータを取得
    /// </summary>
    public StageInfo GetStageByName(string name) {
        return stages.Find(s => s.stageName == name);
    }
}