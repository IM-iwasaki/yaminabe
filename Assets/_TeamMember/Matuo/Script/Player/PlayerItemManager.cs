using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// プレイヤーの取得済みアイテム管理と使用判定
/// </summary>
public class PlayerItemManager : MonoBehaviour {
    public static PlayerItemManager Instance { get; private set; }

    [Header("プレイヤーデータ")]
    private PlayerData playerData;

    //キャラクターデータベース
    [SerializeField] 
    private CharacterDatabase characterDatabase;

    [SerializeField]
    private List<string> unlockedItems = new(); // デバッグ用リスト

    private void Awake() {
        // シングルトン
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // シーンをまたいでも保持

        LoadPlayerData();
    }

    private void LoadPlayerData() {
        playerData = SaveSystem.Load();

        if (playerData.items == null)
            playerData.items = new List<string>();

        // デフォルトキャラクターと最初のスキンを登録
        if (playerData.items.Count == 0) {
            var defaultCharacter = characterDatabase?.characters;
            if (defaultCharacter != null && defaultCharacter.Count > 0 && defaultCharacter[0].skins.Count > 0) {
                string defaultItemName = $"{defaultCharacter[0].characterName}_{defaultCharacter[0].skins[0].skinName}";
                playerData.items.Add(defaultItemName);
            }
        }

        SyncDebugList();
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
        return playerData.items.Exists(item => item.StartsWith(characterName + "_"));
    }

    /// <summary>
    /// 指定したスキンを持っているか判定
    /// </summary>
    public bool HasSkin(string skinName) {
        return playerData.items.Contains(skinName);
    }

    /// <summary>
    /// ガチャで入手したキャラクターを解放（スキン1つ目のみ）
    /// </summary>
    public void UnlockCharacterFromGacha(string characterName) {
        var defaultSkinName = $"{characterName}_{GetFirstSkinName(characterName)}";
        UnlockItem(defaultSkinName);
    }

    /// <summary>
    /// キャラクターの最初のスキン名を取得
    /// </summary>
    private string GetFirstSkinName(string characterName) {
        var database = FindObjectOfType<CharacterDatabase>();
        var character = database.characters.Find(c => c.characterName == characterName);
        return character != null && character.skins.Count > 0 ? character.skins[0].skinName : "";
    }
}