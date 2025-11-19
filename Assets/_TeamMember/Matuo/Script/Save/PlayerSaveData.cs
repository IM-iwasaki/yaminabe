using System.IO;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// プレイヤーデータのバイナリセーブ／ロードを行う静的クラス
/// </summary>
public static class PlayerSaveData {
    public static string filePath => Path.Combine(Application.persistentDataPath, "playerData.dat");

    /// <summary>
    /// プレイヤーデータを保存する（バイナリ形式）
    /// </summary>
    public static void Save(PlayerData data) {
        using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
        using (var writer = new BinaryWriter(fs)) {
            writer.Write(data.playerName);
            writer.Write(data.currentMoney);
            writer.Write(data.currentRate);
            writer.Write(data.currentReticle);

            // items の数
            writer.Write(data.items.Count);
            foreach (var item in data.items) {
                writer.Write(item);
            }
        }
    }

    /// <summary>
    /// プレイヤーデータをロードする（バイナリ形式）
    /// </summary>
    public static PlayerData Load() {
        if (!File.Exists(filePath)) {
            return new PlayerData {
                currentMoney = 0,
                currentRate = 0,
                items = new List<string>()
            };
        }

        var data = new PlayerData();

        using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        using (var reader = new BinaryReader(fs)) {
            data.playerName = reader.ReadString();
            data.currentMoney = reader.ReadInt32();
            data.currentRate = reader.ReadInt32();

            int itemCount = reader.ReadInt32();
            data.items = new List<string>(itemCount);

            for (int i = 0; i < itemCount; i++) {
                data.items.Add(reader.ReadString());
            }
        }

        return data;
    }
}