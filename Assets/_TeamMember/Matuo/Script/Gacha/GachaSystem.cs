using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ガチャのシステム
/// </summary>
public class GachaSystem : MonoBehaviour {
    [Header("ガチャ設定")]
    public GachaData database;

    [Header("単発ガチャの価格")]
    [Min(0)]
    public int gachaCost = 100;

    // ガチャ結果通知イベント
    public event Action<GachaItem> OnItemPulled;

    // 所持アイテム
    private List<GachaItem> ownedItems = new List<GachaItem>();

    private void Awake() {
        LoadOwnedItems();
    }

    /// <summary>
    /// 単発ガチャ
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

        var item = PullSingleInternal();
        if (item != null) {
            ownedItems.Add(item);
            SaveOwnedItems();
            OnItemPulled?.Invoke(item);
        }

        return item;
    }

    /// <summary>
    /// 複数回ガチャ
    /// </summary>
    /// <param name="count">引く回数</param>
    /// <returns>排出されたアイテムリスト</returns>
    public List<GachaItem> PullMultiple(int count) {
        List<GachaItem> results = new List<GachaItem>();
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
                ownedItems.Add(item);
                OnItemPulled?.Invoke(item);
            }
        }

        SaveOwnedItems();
        return results;
    }

    /// <summary>
    /// 抽選処理
    /// </summary>
    private GachaItem PullSingleInternal() {
        if (database == null) return null;

        // レアリティ抽選
        int roll = UnityEngine.Random.Range(0, 100);
        int current = 0;
        Rarity selectedRarity = Rarity.Common;
        string rarityLabel = "Common";

        foreach (var r in database.rarityRates) {
            current += r.rate;
            if (roll < current) {
                selectedRarity = r.rarity;
                rarityLabel = r.rarityName; // デバッグ・UI表示用
                break;
            }
        }

        // アイテム抽選
        var pool = database.GetItemsByRarity(selectedRarity);
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
                Debug.Log($"ガチャ結果: {item.itemName} ({rarityLabel})");
#endif
                return item;
            }
        }

        // 万一の保険
        return null;
    }

    /// <summary>
    /// 所持アイテムを保存
    /// </summary>
    private void SaveOwnedItems() {
        var data = SaveSystem.Load();
        data.obtainedItems = ownedItems.ConvertAll(i => i.itemName);
        SaveSystem.Save(data);
    }

    /// <summary>
    /// 所持アイテムをロード
    /// </summary>
    private void LoadOwnedItems() {
        var data = SaveSystem.Load();
        ownedItems.Clear();
        foreach (var name in data.obtainedItems) {
            var item = database.GetAllItems().Find(i => i.itemName == name);
            if (item != null) ownedItems.Add(item);
        }
    }

    /// <summary>
    /// 所持アイテムの取得（コピー）
    /// </summary>
    public List<GachaItem> GetOwnedItems() => new List<GachaItem>(ownedItems);
}