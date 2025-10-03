
using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// アイテムスポーンを管理するマネージャー
/// 武器と消費アイテムをカテゴリごとにランダム生成
/// </summary>
public class ItemSpawnManager : NetworkSystemObject<ItemSpawnManager> {

    [System.Serializable]
    public class ItemCategory {
        public List<GameObject> prefabs; // このカテゴリに属するプレハブ群
        public int spawnCount;           // 生成する合計数
        [Range(0, 100)]
        public int spawnProbabilityPercent = 100; // 生成確率 (％)
    }

    #region 仕様説明用ヘッダー欄
    [Header("-仕様説明-")]
    [Header("weaponとconsumableはそれぞれ生成用のプール")]
    [Header("プールの中からランダムに抽選され\nspawnCount分生成される")]
    [Header("生成はスポーンポイントに重複せずに\n設定された場所に生成される")]
    [Header("それぞれのプールの中に生成したいプレハブを入れる")]
    [Header("下のspawnCountにはプールの中から何個生成したいか書く")]
    [Header("注意：スポーンポイントとそれぞれのプールの中から\n生成される合計量を一緒にすること")]
    [Header("spawnProbabilityPercentは0から100％で\nそれぞれのプールの中で生成されるか否かの確立を決めれる")]
    #endregion

    [Header("スポーンポイントを探すタグ名")]
    [SerializeField] private string spawnPointTag = "ItemSpawnPoint";

    private Transform[] spawnPoints;

    [Header("武器カテゴリ")]
    [SerializeField] private ItemCategory weaponCategory;

    [Header("消費アイテムカテゴリ")]
    [SerializeField] private ItemCategory consumableCategory;

    [Header("リスポーン間隔（秒）")]
    [SerializeField] private float respawnInterval = 30f;

    private List<GameObject> spawnedItems = new List<GameObject>();

    public override void Initialize() {
        if (isServer) {
            // タグでスポーンポイントを探す
            GameObject[] points = GameObject.FindGameObjectsWithTag(spawnPointTag);
            spawnPoints = new Transform[points.Length];
            for (int i = 0; i < points.Length; i++) {
                spawnPoints[i] = points[i].transform;
            }

            SpawnAllItems();
            InvokeRepeating(nameof(RespawnAllItems), respawnInterval, respawnInterval);
        }
    }

    /// <summary>
    /// すべてのアイテムをカテゴリごとに生成
    /// </summary>
    [Server]
    private void SpawnAllItems() {
        // 使用可能なスポーンポイントをシャッフル
        List<Transform> availablePoints = new List<Transform>(spawnPoints);
        Shuffle(availablePoints);

        // 武器カテゴリをスポーン
        SpawnCategoryItems(weaponCategory, availablePoints);

        // 消費カテゴリをスポーン
        SpawnCategoryItems(consumableCategory, availablePoints);
    }

    /// <summary>
    /// 指定カテゴリのアイテムを生成
    /// </summary>
    private void SpawnCategoryItems(ItemCategory category, List<Transform> availablePoints) {
        for (int i = 0; i < category.spawnCount; i++) {
            if (availablePoints.Count == 0) break; // スポーンポイント切れ

            // スポーンポイントをランダムに取得
            int index = Random.Range(0, availablePoints.Count);
            Transform point = availablePoints[index];
            availablePoints.RemoveAt(index); // 使用済みなので削除

            // 確率チェック（％管理）
            int rand = Random.Range(0, 100); // 0～99
            if (rand >= category.spawnProbabilityPercent) {
                // この場合は何も生成せず、スポーンポイントだけ消費される
                continue;
            }

            // このカテゴリからランダムに1つ選択
            GameObject prefab = category.prefabs[Random.Range(0, category.prefabs.Count)];

            // 生成
            GameObject obj = Instantiate(prefab, point.position, Quaternion.identity);
            spawnedItems.Add(obj);
        }
    }

    /// <summary>
    /// リスポーン処理（古いアイテムを削除して再生成）
    /// </summary>
    [Server]
    private void RespawnAllItems() {
        if (!isServer)
            return;
        foreach (var obj in spawnedItems) {
            if (obj != null) Destroy(obj);
        }
        spawnedItems.Clear();
        SpawnAllItems();
    }

    /// <summary>
    /// リストをシャッフル
    /// </summary>
    private void Shuffle<T>(List<T> list) {
        for (int i = 0; i < list.Count; i++) {
            int rand = Random.Range(i, list.Count);
            T temp = list[i];
            list[i] = list[rand];
            list[rand] = temp;
        }
    }
}
