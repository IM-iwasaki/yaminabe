using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ガチャシステム
/// </summary>
public class GachaSystem : MonoBehaviour {
    [Header("ガチャ設定")]
    public GachaData data;

    [Header("単発ガチャの価格")]
    [Min(0)]
    public int gachaCost = 100;

    /// <summary>
    /// ガチャ結果通知イベント
    /// </summary>
    public event Action<GachaItem> OnItemPulled;

    /// <summary>
    /// 単発ガチャを引く
    /// </summary>
    /// <returns>貧乏ならnull</returns>
    public GachaItem PullSingle() {
        // 所持金チェック
        if (PlayerWallet.Instance == null) return null;
        // 支払い処理
        if (!PlayerWallet.Instance.SpendMoney(gachaCost)) {
            Debug.Log("貧乏過ぎて引けないよん");
            return null;
        }

        // 抽選処理
        var item = PullSingleInternal();
        if (item != null)
            PlayerItemManager.Instance.UnlockGachaItem(item);

        OnItemPulled?.Invoke(item);

        return item;
    }

    /// <summary>
    /// 複数回ガチャを引く
    /// </summary>
    /// <param name="count">引く回数</param>
    /// <returns>貧乏なら空</returns>
    public List<GachaItem> PullMultiple(int count) {
        List<GachaItem> results = new();
        if (PlayerWallet.Instance == null || count <= 0) return results;

        int totalCost = gachaCost * count;
        // 支払い処理
        if (!PlayerWallet.Instance.SpendMoney(totalCost)) {
            Debug.Log("貧乏過ぎて引けないよん");
            return results;
        }
        // 指定回数分抽選
        for (int i = 0; i < count; i++) {
            var item = PullSingleInternal();
            if (item != null) {
                results.Add(item);
                PlayerItemManager.Instance.UnlockGachaItem(item);
                OnItemPulled?.Invoke(item);
            }
        }
        return results;
    }

    /// <summary>
    /// ガチャ抽選処理
    /// </summary>
    private GachaItem PullSingleInternal() {
        if (data == null) return null;

        // レアリティ抽選
        int roll = UnityEngine.Random.Range(0, 100);
        int current = 0;
        Rarity selectedRarity = Rarity.Common;

        foreach (var r in data.rarityRates) {
            current += r.rate;
            if (roll < current) {
                selectedRarity = r.rarity;
                break;
            }
        }

        // アイテム抽選
        var pool = data.GetItemsByRarity(selectedRarity);
        if (pool == null || pool.Count == 0) return null;

        // 各アイテムの rate に応じた抽選
        int totalRate = 0;
        foreach (var item in pool) totalRate += item.rate;
        if (totalRate <= 0) return null;

        int randomValue = UnityEngine.Random.Range(0, totalRate);
        int currentWeight = 0;
        foreach (var item in pool) {
            currentWeight += item.rate;
            if (randomValue < currentWeight) {
#if UNITY_EDITOR
                Debug.Log($"ガチャ結果: {item.itemName} ({selectedRarity})");
#endif
                return item;
            }
        }

        // 万一の保険
        return null;
    }
}