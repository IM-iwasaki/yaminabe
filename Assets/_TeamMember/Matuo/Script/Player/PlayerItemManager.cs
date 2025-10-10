using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// プレイヤーの取得済みアイテム管理と使用判定
/// </summary>
public class PlayerItemManager : MonoBehaviour {
    [Header("プレイヤーデータ")]
    private PlayerData playerData;

    private void Awake() {
        LoadPlayerData();
    }

    private void LoadPlayerData() {
        playerData = SaveSystem.Load();
        if (playerData.items == null)
            playerData.items = new List<PlayerItemStatus>();
    }

    private void SavePlayerData() {
        SaveSystem.Save(playerData);
    }

    /// <summary>
    /// アイテムを取得済みにする
    /// </summary>
    public void UnlockItem(string itemName) {
        var item = playerData.items.Find(i => i.itemName == itemName);
        if (item == null) {
            playerData.items.Add(new PlayerItemStatus {
                itemName = itemName,
                isUnlocked = true,
                isEquipped = false
            });
        } else {
            item.isUnlocked = true;
        }

        SavePlayerData();
    }

    /// <summary>
    /// アイテムが使用可能か判定
    /// </summary>
    public bool CanUseItem(string itemName) {
        var item = playerData.items.Find(i => i.itemName == itemName);
        return item != null && item.isUnlocked;
    }

    /// <summary>
    /// 指定アイテムを使用中にする
    /// 他のアイテムは使用解除される
    /// </summary>
    public void EquipItem(string itemName) {
        foreach (var i in playerData.items)
            i.isEquipped = false;

        var item = playerData.items.Find(i => i.itemName == itemName);
        if (item != null && item.isUnlocked)
            item.isEquipped = true;

        SavePlayerData();
    }

    /// <summary>
    /// 現在使用中のアイテム名を取得
    /// 使用中がなければ null
    /// </summary>
    public string GetEquippedItem() {
        var item = playerData.items.Find(i => i.isEquipped);
        return item != null ? item.itemName : null;
    }

    /// <summary>
    /// 取得済みアイテムリストをコピーで取得
    /// </summary>
    public List<PlayerItemStatus> GetOwnedItems() => new(playerData.items);
}