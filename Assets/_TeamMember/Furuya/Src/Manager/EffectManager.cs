using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : SystemObject<EffectManager> {

    // シングルトンとして使えるようにインスタンスを公開
    public static EffectManager instance { get; private set; }

    [Header("エフェクトプレハブの登録リスト")]
    [Tooltip("IDと紐づけるエフェクトプレハブの一覧")]
    public List<EffectEntry> effectEntries = new List<EffectEntry>(); // 登録用のリスト（インスペクター設定）

    // 内部的に管理する辞書（IDからプレハブを引けるように）
    private Dictionary<string, GameObject> effectDictionary = new Dictionary<string, GameObject>();

    public override void Initialize() {

        // 辞書にエフェクトを登録（ID → プレハブ）
        foreach (var entry in effectEntries) {
            if (entry != null && !effectDictionary.ContainsKey(entry.id)) {
                effectDictionary.Add(entry.id, entry.prefab);
            }
        }
    }

    /// <summary>
    /// 指定IDのエフェクトを位置・回転付きで生成し、一定時間後に破棄する
    /// </summary>
    /// <param name="id">エフェクトID</param>
    /// <param name="position">出現位置</param>
    /// <param name="rotation">出現時の回転</param>
    /// <param name="destroyDelay">自動破棄までの時間（秒）</param>
    public void SpawnEffect(string id, Vector3 position, Quaternion rotation, float destroyDelay = 3f) {
        if (!effectDictionary.ContainsKey(id)) {
            Debug.LogWarning($"エフェクトIDが見つかりません: {id}");
            return;
        }

        GameObject prefab = effectDictionary[id];
        GameObject effectInstance = Instantiate(prefab, position, rotation);

        // 一定時間後に自動で破棄
        Destroy(effectInstance, destroyDelay);
    }
}

/// <summary>
/// IDと紐づけるエフェクトの情報クラス（インスペクター用）
/// </summary>
[System.Serializable]
public class EffectEntry {
    public string id;            // 任意のID（例: "slash", "dash", "heal"など）
    public GameObject prefab;    // 実際のパーティクルやエフェクトプレハブ
}

