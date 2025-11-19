using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterDatabase", menuName = "ScriptableObject/CharacterData/CharacterDatabase")]
public class CharacterDatabase : ScriptableObject {
    [Header("登録されているキャラクター一覧")]
    public List<CharacterInfo> characters = new(); // キャラクター情報のリスト

    [System.Serializable]
    public class CharacterInfo {
        [Header("キャラクター基本情報")]
        public string characterName;                  // キャラ名
        public GeneralCharacterStatus statusData;     // ステータスデータ

        [Header("スキン設定")]
        public List<SkinInfo> skins = new();          // スキン情報リスト
    }

    [System.Serializable]
    public class SkinInfo {
        [Header("スキン名")]
        public string skinName;                      // スキンの名前

        [Header("スキンプレハブ")]
        public GameObject skinPrefab;                // スキン用のプレハブ
    }
}