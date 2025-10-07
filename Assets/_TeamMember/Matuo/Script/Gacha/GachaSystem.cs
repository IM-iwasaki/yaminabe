using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ガチャシステム
/// ScriptableObjectで管理
/// </summary>
public class GachaSystem : MonoBehaviour {
    /// <summary>
    /// ガチャで使用するアイテムデータベース
    /// </summary>
    public GachaData database;

    /// <summary>
    /// 単発ガチャ
    /// </summary>
    public GachaItem PullSingle() {
        // 全アイテムのrate合計を計算
        int totalRate = 0;
        foreach (var item in database.items)
            totalRate += item.rate;

        // 0からtotalRateの範囲でランダム
        int randomValue = Random.Range(0, totalRate);
        int current = 0;

        // ランダム値に応じてアイテムを選択
        foreach (var item in database.items) {
            current += item.rate;
            if (randomValue < current) {
                SpawnItem(item); // 選ばれたアイテムを生成
                return item;
            }
        }

        // 万一の保険
        return null;
    }

    /// <summary>
    /// 複数回ガチャ
    /// </summary>
    /// <param name="count"></param>
    public List<GachaItem> PullMultiple(int count) {
        List<GachaItem> result = new List<GachaItem>();
        for (int i = 0; i < count; i++) {
            result.Add(PullSingle());
        }
        return result;
    }

    /// <summary>
    /// 指定したアイテムの生成
    /// </summary>
    /// <param name="item">生成するアイテム</param>
    private void SpawnItem(GachaItem item) {
        Debug.Log("ガチャで " + item.itemName + " を当てました！");
    }
}