using Mirror;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 【Mirror対応】
/// アイテムスポーン全体を管理するマネージャー。
/// - 各カテゴリ（武器／消費アイテム）から確率に応じてランダム生成。
/// - 各アイテムにはカテゴリ内での個別生成確率を設定可能。
/// - 定期的に全アイテムをリスポーンする。
/// </summary>
public class ItemSpawnManager : NetworkSystemObject<ItemSpawnManager> {

    // ==========================================
    // ▼ 各アイテム個別設定クラス
    // ==========================================
    [System.Serializable]
    public class SpawnableItem {
        [Header("生成するプレハブ")]
        public GameObject prefab;   // 実際に生成するアイテムプレハブ

        [Range(0, 100)]
        [Header("このアイテムがカテゴリ内で生成される確率（％）")]
        public int spawnChancePercent = 100;   // カテゴリ内での個別出現確率
    }

    // ==========================================
    // ▼ カテゴリ（武器・消費アイテムなど）設定クラス
    // ==========================================
    [System.Serializable]
    public class ItemCategory {
        [Header("このカテゴリに含まれるアイテム群")]
        public List<SpawnableItem> items = new List<SpawnableItem>(); // 登録されたアイテム群

        [Range(0, 100)]
        [Header("カテゴリ全体として生成される確率（％）")]
        public int spawnProbabilityPercent = 100; // カテゴリ全体が生成される確率（カテゴリスキップ用）
    }

    #region === インスペクター設定項目 ===

    [Header("=== スポーン設定 ===")]
    [Header("シーン内のスポーンポイントを探すためのタグ名")]
    [SerializeField] private string spawnPointTag = "ItemSpawnPoint"; // スポーンポイントタグ名

    [Header("=== カテゴリ設定 ===")]
    [Header("武器カテゴリの生成設定")]
    [SerializeField] private ItemCategory weaponCategory = new ItemCategory();      // 武器カテゴリ
    [Header("消費アイテムカテゴリの生成設定")]
    [SerializeField] private ItemCategory consumableCategory = new ItemCategory();  // 消費アイテムカテゴリ

    [Header("=== 比率設定 ===")]
    [Range(0f, 1f)]
    [Header("スポーンポイント全体に対して武器が生成される比率\n（例：0.4で全体の40％が武器）")]
    [SerializeField] private float weaponSpawnRatio = 0.4f;

    [Header("=== リスポーン設定 ===")]
    [Header("すべてのアイテムをリセットして再生成する間隔（秒）")]
    [SerializeField] private float respawnInterval = 30f;

    #endregion

    // ==========================================
    // ▼ 内部変数
    // ==========================================
    private Transform[] spawnPoints;                                            // スポーンポイント一覧
    private readonly List<GameObject> spawnedItems = new List<GameObject>();    // 現在シーンに存在している生成済みアイテム一覧

    // ==========================================
    // ▼ 初期化処理（サーバー側のみ実行）
    // ==========================================
    public override void Initialize() {
        if (!isServer) return; // Mirrorの仕様上、生成はサーバー側でのみ行う

        //  開始時にスポーンポイントを取得・アイテム生成
        SetupSpawnPoint();

    }

    // ====================================================================================
    // ▼ ステージ生成時にスポーンポイントを取得・アイテム生成を開始する処理
    // ====================================================================================
    public void SetupSpawnPoint() {
        if (!isServer) return; // Mirrorの仕様上、生成はサーバー側でのみ行う

        //  一度全部リセットする
        ResetSpawnPoint();

        // タグからスポーンポイントを全取得
        GameObject[] points = GameObject.FindGameObjectsWithTag(spawnPointTag);
        spawnPoints = points.Select(p => p.transform).ToArray();

        if (spawnPoints.Length == 0) {
            Debug.LogWarning($"[ItemSpawnManager] スポーンポイント（タグ: {spawnPointTag}）が見つかりません。");
            return;
        }

        // 初回スポーン実行
        SpawnAllItems();

        // 一定時間ごとにリスポーン処理を自動呼び出し
        InvokeRepeating(nameof(RespawnAllItems), respawnInterval, respawnInterval);
    }

    // ==========================================
    // ▼ スポーン関連のリセット
    // ==========================================
    public void ResetSpawnPoint() {
        // 既存Invokeを解除
        CancelInvoke(nameof(RespawnAllItems));

        // 既存アイテムを削除
        foreach (var obj in spawnedItems) {
            if (obj != null)
                NetworkServer.Destroy(obj);
        }
        spawnedItems.Clear();

        //  既存スポーンポイントを削除
        spawnPoints = null;
    }

    // ==========================================
    // ▼ 全カテゴリからの一括スポーン処理
    // ==========================================
    [Server]
    private void SpawnAllItems() {
        // スポーンポイントのリストを複製してシャッフル
        List<Transform> availablePoints = new List<Transform>(spawnPoints);
        Shuffle(availablePoints);

        // 全ポイント数から比率に応じてカテゴリごとの生成数を算出
        int totalPoints = availablePoints.Count;
        int weaponPoints = Mathf.RoundToInt(totalPoints * weaponSpawnRatio); // 武器の生成数
        int consumablePoints = totalPoints - weaponPoints;                  // 消費アイテム生成数

        // カテゴリごとの生成を実行
        int weaponSpawned = SpawnCategoryItems(weaponCategory, availablePoints, weaponPoints);
        int consumableSpawned = SpawnCategoryItems(consumableCategory, availablePoints, consumablePoints);

        // デバッグ出力
        Debug.Log($"[ItemSpawnManager] 武器: {weaponSpawned}/{weaponPoints}, 消費: {consumableSpawned}/{consumablePoints}");
    }

    // ==========================================
    // ▼ 指定カテゴリから指定数のアイテムをスポーン
    // ==========================================
    [Server]
    private int SpawnCategoryItems(ItemCategory category, List<Transform> availablePoints, int spawnPointsCount) {
        // カテゴリ内にアイテムが登録されていない場合はスキップ
        if (category.items.Count == 0) {
            Debug.LogWarning($"[ItemSpawnManager] カテゴリにプレハブが登録されていません。");
            return 0;
        }

        int spawned = 0; // 実際に生成された数

        // スポーンポイントごとにループ
        for (int i = 0; i < spawnPointsCount && availablePoints.Count > 0; i++) {
            // ランダムなスポーンポイントを選択し、リストから除外
            int index = Random.Range(0, availablePoints.Count);
            Transform point = availablePoints[index];
            availablePoints.RemoveAt(index);

            // カテゴリ全体の生成確率を判定
            // 例：カテゴリ確率が80なら、20％の確率でスキップされる
            if (Random.Range(0, 100) >= category.spawnProbabilityPercent)
                continue;

            // カテゴリ内の個別アイテム確率をもとにプレハブを抽選
            GameObject prefab = GetItemByChance(category.items);
            if (prefab == null)
                continue;

            // スポーン位置を少し上げる（地面貫通防止）
            Vector3 spawnPos = point.position + Vector3.up * 1f;

            // プレハブ生成
            GameObject obj = Instantiate(prefab, spawnPos, Quaternion.identity);

            // Mirrorで同期させるためにNetworkIdentityを必須チェック
            var identity = obj.GetComponent<NetworkIdentity>();
            if (identity == null) {
                Debug.LogError($"[ItemSpawnManager] {prefab.name} に NetworkIdentity がありません。");
                Destroy(obj);
                continue;
            }

            // Mirror経由で全クライアントに生成を通知
            NetworkServer.Spawn(obj);

            // 管理リストに追加
            spawnedItems.Add(obj);
            spawned++;
        }

        return spawned;
    }

    // ==========================================
    // ▼ カテゴリ内アイテムの「重み付き確率抽選」
    // ==========================================
    private GameObject GetItemByChance(List<SpawnableItem> items) {
        // 登録されているすべてのアイテムの確率を合計
        int totalChance = items.Sum(item => item.spawnChancePercent);
        if (totalChance <= 0)
            return null; // 全アイテムが確率0の場合はスキップ

        // 0～合計値の範囲でランダム値を取得
        int randomValue = Random.Range(0, totalChance);
        int cumulative = 0;

        // 累積値で範囲を超えた最初のアイテムを採用
        foreach (var item in items) {
            cumulative += item.spawnChancePercent;
            if (randomValue < cumulative)
                return item.prefab;
        }

        return null;
    }

    // ==========================================
    // ▼ 全アイテムを破棄し、再生成するリスポーン処理
    // ==========================================
    [Server]
    private void RespawnAllItems() {
        if (!isServer) return;

        // 既存アイテムをすべて破棄
        foreach (var obj in spawnedItems) {
            if (obj != null)
                NetworkServer.Destroy(obj);
        }

        spawnedItems.Clear(); // リストも初期化

        // 新たに全アイテムを再生成
        SpawnAllItems();
    }

    // ==========================================
    // ▼ 汎用シャッフル処理（リストの順序をランダム化）
    // ==========================================
    private void Shuffle<T>(List<T> list) {
        for (int i = 0; i < list.Count; i++) {
            int rand = Random.Range(i, list.Count);
            (list[i], list[rand]) = (list[rand], list[i]);
        }
    }
}
