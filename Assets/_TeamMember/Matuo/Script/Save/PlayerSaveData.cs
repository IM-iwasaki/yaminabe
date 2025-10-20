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
#if UNITY_EDITOR
        // エディター上ではセーブしない
        if (!Application.isPlaying) return;
#endif
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(filePath, json);
        Debug.Log("ここにセーブしたよ: " + filePath);
    }

    /// <summary>
    /// プレイヤーデータをロードする
    /// ファイルが無ければ初期状態のデータを返す
    /// </summary>
    public static PlayerData Load() {
#if UNITY_EDITOR
        // エディター上ではロードしない
        if (!Application.isPlaying) {
            return new PlayerData {
                currentMoney = 0,
                items = new List<string>()
            };
        }
#endif

        if (!File.Exists(filePath)) {
            return new PlayerData {
                currentMoney = 0,
                items = new List<string>()
            };
        }

        string json = File.ReadAllText(filePath);
        return JsonUtility.FromJson<PlayerData>(json);
    }
}