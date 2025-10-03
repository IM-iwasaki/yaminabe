using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// アイテムのスポーン情報
/// </summary>
[System.Serializable]
public class ItemSpawnData {
    [Header("生成するアイテムプレハブ")]
    public GameObject prefab;

    [Header("生成数")]
    public int count = 1;
}

/// <summary>
/// アイテムのスポーン/リスポーンを管理
/// </summary>
public class ItemSpawnManager : MonoBehaviour {
    [Header("スポーンさせたいアイテムリスト")]
    public List<ItemSpawnData> spawnItems = new List<ItemSpawnData>();

    [Header("スポーン位置候補")]
    public Transform[] spawnPoints;

    [Header("リスポーン間隔（秒）")]
    public float respawnInterval = 30f;

    // 現在生成されているアイテムのリスト
    private List<GameObject> spawnedObjects = new List<GameObject>();

    void Start() {
        StartCoroutine(RespawnRoutine());
    }

    /// <summary>
    /// リスポーン処理
    /// </summary>
    private IEnumerator RespawnRoutine() {
        while (true) {
            // 既存アイテム削除
            foreach (var obj in spawnedObjects) {
                if (obj != null) Destroy(obj);
            }
            spawnedObjects.Clear();

            // 新規生成
            SpawnAllItems();

            // 次のリスポーンまで待機
            yield return new WaitForSeconds(respawnInterval);
        }
    }

    /// <summary>
    /// アイテムをすべて生成（場所かぶりなし）
    /// </summary>
    private void SpawnAllItems() {
        // スポーンポイントを一時リストにコピー
        List<Transform> availablePoints = new List<Transform>(spawnPoints);

        foreach (var data in spawnItems) {
            for (int i = 0; i < data.count; i++) {
                if (availablePoints.Count == 0) {
                    Debug.LogWarning("スポーンポイントが不足しています！");
                    return;
                }

                // ランダムにスポーンポイントを選び、リストから削除して再利用不可にする
                int index = Random.Range(0, availablePoints.Count);
                Transform spawnPoint = availablePoints[index];
                availablePoints.RemoveAt(index);

                // アイテム生成
                GameObject obj = Instantiate(data.prefab, spawnPoint.position, Quaternion.identity);
                spawnedObjects.Add(obj);
            }
        }
    }
}
