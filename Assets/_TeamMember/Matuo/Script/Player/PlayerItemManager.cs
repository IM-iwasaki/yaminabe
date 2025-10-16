using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// プレイヤーの取得済みアイテム管理と使用判定
/// <summary>
public class PlayerItemManager : MonoBehaviour {
    [Header("プレイヤーデータ")]
    private PlayerData playerData;

    [SerializeField]
    private List<string> unlockedItems = new(); // デバッグ用リスト

    private void Awake() {
        LoadPlayerData();
    }

    private void LoadPlayerData() {
        playerData = SaveSystem.Load();
        if (playerData.items == null)
            playerData.items = new List<string>();
    }

    private void SavePlayerData() {
        SaveSystem.Save(playerData);
        SyncDebugList();
    }

    /// <summary>
    /// デバッグ用にリストを同期
    /// </summary>
    private void SyncDebugList() {
        unlockedItems = new List<string>(playerData.items);
    }

    /// <summary>
    /// アイテムを取得済みにする
    /// </summary>
    public void UnlockItem(string itemName) {
        if (!playerData.items.Contains(itemName)) {
            playerData.items.Add(itemName);
        }
        SavePlayerData();
    }

    /// <summary>
    /// 取得済みアイテムリストをコピーで取得
    /// </summary>
    public List<string> GetOwnedItems() => new(playerData.items);

    /// <summary>
    /// 指定したキャラクターを持っているか判定
    /// </summary>
    public bool HasCharacter(string characterName) {
        return playerData.items.Contains(characterName);
    }

    /// <summary>
    /// 指定したスキンを持っているか判定
    /// </summary>
    public bool HasSkin(string skinName) {
        return playerData.items.Contains(skinName);
    }
}