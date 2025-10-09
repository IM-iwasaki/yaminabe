using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ガチャの抽選処理を行うクラス
/// GachaDataを参照、PlayerWalletと連携して支払いと抽選を管理すりゅ
/// </summary>
public class GachaSystem : MonoBehaviour {
    [Header("ガチャ設定")]
    public GachaData database;

    [Header("単発ガチャの価格")]
    [Min(0)]
    public int gachaCost = 100;

    /// <summary>
    /// 単発ガチャを引く
    /// </summary>
    /// <returns>排出されたアイテム 貧乏ならnull</returns>
    public GachaItem PullSingle() {
        // 所持金チェック
        if (PlayerWallet.Instance == null) return null;

        // 支払い処理
        if (!PlayerWallet.Instance.SpendMoney(gachaCost)) {
            Debug.Log("貧乏過ぎて引けないよん");
            return null;
        }

        // 抽選（内部ロジック共通化）
        return PullSingleInternal();
    }

    /// <summary>
    /// 複数回ガチャ（10連など）を引く
    /// </summary>
    /// <param name="count">引く回数</param>
    /// <returns>排出されたアイテムリスト</returns>
    public List<GachaItem> PullMultiple(int count) {
        List<GachaItem> results = new List<GachaItem>();

        if (PlayerWallet.Instance == null) return results;
        

        if (count <= 0) return results;

        int totalCost = gachaCost * count;

        // 支払い処理
        if (!PlayerWallet.Instance.SpendMoney(totalCost)) {
            Debug.Log("貧乏過ぎて引けないよん");
            return results;
        }

        // 指定回数分抽選
        for (int i = 0; i < count; i++) {
            var item = PullSingleInternal(); // 抽選
            if (item != null)
                results.Add(item);
        }

        return results;
    }

    /// <summary>
    /// 抽選だけを行う内部関数
    /// </summary>
    private GachaItem PullSingleInternal() {
        if (database == null || database.items == null || database.items.Count == 0)
            return null;

        int totalRate = 0;
        foreach (var item in database.items) {
            if (item != null && item.rate > 0)
                totalRate += item.rate;
        }

        if (totalRate <= 0) return null;

        // 0からtotalRateの範囲でランダム
        int randomValue = Random.Range(0, totalRate);
        int current = 0;

        // ランダム値に応じてアイテムを選択
        foreach (var item in database.items) {
            current += item.rate;
            if (randomValue < current) {
                SpawnItem(item);
                return item;
            }
        }

        // 万一の保険
        return null;
    }

    /// <summary>
    /// 抽選結果の生成と通知処理
    /// </summary>
    /// <param name="item">獲得したアイテム</param>
    private void SpawnItem(GachaItem item) {
        if (item == null) return;
        Debug.Log($"ガチャ結果: {item.itemName} ({item.rarity})");

        // 景品プレハブを生成するならこんな感じ?
        // if (item.prize != null)
        //     Instantiate(item.prize, Vector3.zero, Quaternion.identity);
    }
}