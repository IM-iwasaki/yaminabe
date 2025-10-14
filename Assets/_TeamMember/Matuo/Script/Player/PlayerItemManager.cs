using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// プレイヤーの取得済みアイテム管理と使用判定
/// </summary>
public class PlayerItemManager : MonoBehaviour {
    [Header("プレイヤーデータ")]
    private PlayerData playerData;

    [SerializeField]
    private List<PlayerItemStatus> unlockedItems = new();

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
        SyncDebugList();
    }

    /// <summary>
    /// アイテムのリストに反映(デバッグ確認用)
    /// </summary>
    private void SyncDebugList()
    {
        unlockedItems = new List<PlayerItemStatus>(playerData.items);
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
            });
        } else {
            item.isUnlocked = true;
        }

        SavePlayerData();
    }

    /// <summary>
    /// 取得済みアイテムリストをコピーで取得
    /// </summary>
    public List<PlayerItemStatus> GetOwnedItems() => new(playerData.items);
}