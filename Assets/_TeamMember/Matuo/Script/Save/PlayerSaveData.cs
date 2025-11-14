using System.IO;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// プレイヤーデータのセーブ／ロードを行う静的クラス
/// </summary>
public static class PlayerSaveData {
    private static string filePath => Path.Combine(Application.persistentDataPath, "playerData.json");

    /// <summary>
    /// プレイヤーデータを保存する
    /// </summary>
    public static void Save(PlayerData data) {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(filePath, json);
    }

    /// <summary>
    /// プレイヤーデータをロードする
    /// ファイルが無ければ初期状態のデータを返す
    /// </summary>
    public static PlayerData Load() {

        if (!File.Exists(filePath)) {
            return new PlayerData {
                currentMoney = 0,
                currentRate = 0,
                items = new List<string>()
            };
        }

        string json = File.ReadAllText(filePath);
        return JsonUtility.FromJson<PlayerData>(json);
    }
}