using UnityEngine;
using Mirror;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine.AI;

/// <summary>
/// ステージ生成とリスポーン地点管理
/// </summary>
public class StageManager : NetworkSystemObject<StageManager> {
    [Header("ステージ一覧")]
    public List<StageData> stages = new();
    [Header("PvEステージ一覧")]
    public List<PVEStageData> pveStages = new();

    public CaptureHoko currentHoko;

    private GameObject currentStageInstance;
    private int currentPveIndex = 0;
    // リスポーン地点
    [SerializeField] private readonly SyncList<Transform> normalRespawnPoints = new();
    [SerializeField] private readonly SyncList<Transform> redRespawnPoints = new();
    [SerializeField] private readonly SyncList<Transform> blueRespawnPoints = new();

    // 現在のリスポーンモード
    private RespawnMode currentRespawnMode = RespawnMode.Team;

    //今後ルールが増えることがあれば追加する
    [Header("ルール毎のオブジェクト")]
    public GameObject areaPrefab;
    public GameObject hokoPrefab;
    private GameObject currentRuleObject;

    [Header("PVE用AreaPrefab")]
    public GameObject pveAreaPrefab;

    [Header("PVE用HokoPrefab")]
    public GameObject pveHokoPrefab;

    protected override void Awake() {
        base.Awake();
    }

    /// <summary>
    /// ステージを生成（サーバー専用）
    /// </summary>
    [Server]
    public void SpawnStage(StageData stageData, GameRuleType rule) {
        if (stageData == null || stageData.stagePrefab == null) return;

        // 既存ステージを削除
        if (currentStageInstance != null)
            NetworkServer.Destroy(currentStageInstance);

        // ステージ生成
        currentStageInstance = Instantiate(stageData.stagePrefab);

        //ルールごとに生成するオブジェクトを変更する
        ApplyRuleObjects(rule);

        NetworkServer.Spawn(currentStageInstance);
        ItemSpawnManager.Instance.SetupSpawnPoint();

        // リスポーン地点登録
        RegisterRespawnPoints(currentStageInstance);
    }

    /// <summary>
    /// 古谷　ルールごとのオブジェクト生成
    /// </summary>
    [Server]
    void ApplyRuleObjects(GameRuleType rule) {
        // 既存のルールオブジェクトをタグで検索して削除
        var exist = GameObject.FindGameObjectsWithTag("RuleObject");
        foreach (var obj in exist) {
            NetworkServer.Destroy(obj);
        }

        currentRuleObject = null;
        currentHoko = null; // 古い参照はクリア

        // DeathMatch の場合は生成なし
        if (rule == GameRuleType.DeathMatch)
            return;

        // 作るプレハブを選ぶ
        GameObject prefab = null;
        switch (rule) {
            case GameRuleType.Area: prefab = areaPrefab; break;
            case GameRuleType.Hoko: prefab = hokoPrefab; break;
        }

        if (prefab == null)
            return;

        currentRuleObject = Instantiate(prefab, new Vector3(0, 2, 0), Quaternion.identity);
        currentRuleObject.tag = "RuleObject";
        NetworkServer.Spawn(currentRuleObject);

        // Hoko なら CaptureHoko コンポーネントを保持
        if (rule == GameRuleType.Hoko) {
            currentHoko = currentRuleObject.GetComponent<CaptureHoko>();
        }
    }

    /// <summary>
    /// PVEステージの生成順番取得用
    /// </summary>
    /// <param name="random"></param>
    /// <returns></returns>
    [Server]
    public PVEStageData GetNextPveStage(bool random) {
        if (pveStages.Count == 0) return null;

        if (random) {
            return pveStages[Random.Range(0, pveStages.Count)];
        }

        var stage = pveStages[currentPveIndex];
        currentPveIndex = (currentPveIndex + 1) % pveStages.Count;
        return stage;
    }

    /// <summary>
    /// PVE用のステージ生成
    /// </summary>
    /// <param name="stage"></param>
    [Server]
    public void SpawnPveStage(PVEStageData stage) {
        if (stage == null || stage.stagePrefab == null) return;

        // 既存ステージ削除
        if (currentStageInstance != null)
            NetworkServer.Destroy(currentStageInstance);

        // ステージ生成
        currentStageInstance = Instantiate(stage.stagePrefab);
        NetworkServer.Spawn(currentStageInstance);

        // リスポーン地点登録
        RegisterRespawnPoints(currentStageInstance);
        SetRespawnMode(RespawnMode.Team);

        SpawnPveAreas();
        SpawnPveHokos();

        BakePveGroundNavMesh(currentStageInstance);
    }

    private void BakePveGroundNavMesh(GameObject stageRoot) {

        // 既存の NavMeshSurface を全部消す
        foreach (var s in stageRoot.GetComponentsInChildren<NavMeshSurface>(true)) {
            DestroyImmediate(s);
        }

        // ステージに NavMeshSurface を1つだけ追加
        var surface = stageRoot.AddComponent<NavMeshSurface>();

        surface.collectObjects = CollectObjects.Children;
        surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
        surface.layerMask = LayerMask.GetMask("PVEGround", "PVEWall");

        surface.BuildNavMesh();
    }

    /// <summary>
    /// PVE用エリア生成
    /// </summary>
    [Server]
    private void SpawnPveAreas() {
        var spawnPoints = currentStageInstance.GetComponentsInChildren<AreaSpawnPoint>(true);

        foreach (var sp in spawnPoints) {
            var areaObj = Instantiate(pveAreaPrefab,sp.transform.position,sp.transform.rotation);

            NetworkServer.Spawn(areaObj);

            var area = areaObj.GetComponent<CaptureAreaPVE>();
            area.Initialize(sp);
        }
    }

    /// <summary>
    /// PVE用ホコをスポーンする
    /// </summary>
    [Server]
    private void SpawnPveHokos() {
        var spawnPoints = currentStageInstance.GetComponentsInChildren<HokoSpawnPoint>(true);

        foreach (var sp in spawnPoints) {
            for (int i = 0; i < sp.spawnCount; i++) {

                Vector3 offset = Random.insideUnitSphere * 0.5f;
                offset.y = 0f;

                var hoko = Instantiate(pveHokoPrefab,sp.transform.position + offset,Quaternion.identity);

                NetworkServer.Spawn(hoko);
            }
        }
    }

    /// <summary>
    /// ステージ内のリスポーン地点をタグから登録
    /// </summary>
    [Server]
    private void RegisterRespawnPoints(GameObject stageObj) {
        normalRespawnPoints.Clear();
        redRespawnPoints.Clear();
        blueRespawnPoints.Clear();

        foreach (Transform point in stageObj.GetComponentsInChildren<Transform>(true)) {
            if (point.CompareTag("NormalRespawnPoint"))
                normalRespawnPoints.Add(point);
            else if (point.CompareTag("RedRespawnPoint"))
                redRespawnPoints.Add(point);
            else if (point.CompareTag("BlueRespawnPoint"))
                blueRespawnPoints.Add(point);
        }
    }

    /// <summary>
    /// リスポーンモード設定（サーバー側のみ）
    /// </summary>
    [Server]
    public void SetRespawnMode(RespawnMode mode) {
        currentRespawnMode = mode;
    }

    /// <summary>
    /// 現在のリスポーンモードを取得
    /// </summary>
    public RespawnMode GetRespawnMode() => currentRespawnMode;

    /// <summary>
    /// 共通リスポーン地点のリストを返す
    /// </summary>
    public IReadOnlyList<Transform> GetNormalSpawnPoints() => normalRespawnPoints;

    /// <summary>
    /// チームごとのリスポーン地点を返す
    /// </summary>
    public IReadOnlyList<Transform> GetTeamSpawnPoints(TeamData.TeamColor team) {
        return team switch {
            TeamData.TeamColor.Red => redRespawnPoints,
            TeamData.TeamColor.Blue => blueRespawnPoints,
            _ => normalRespawnPoints
        };
    }

    /// <summary>
    /// 現在のモードに応じてスポーン地点を1つ取得
    /// （デスマッチなら共通ランダム、チーム戦ならチーム専用を使用）
    /// </summary>
    public Transform GetSpawnPoint(TeamData.TeamColor team) {
        if (currentRespawnMode == RespawnMode.Random) {
            if (normalRespawnPoints.Count == 0) return null;
            return normalRespawnPoints[Random.Range(0, normalRespawnPoints.Count)];
        }
        else {
            var points = GetTeamSpawnPoints(team);
            if (points.Count == 0) return null;
            return points[Random.Range(0, points.Count)];
        }
    }

    /// <summary>
    /// 現在のステージを削除（サーバー専用）
    /// </summary>
    [Server]
    public void ClearStage() {
        if (currentStageInstance != null)
            NetworkServer.Destroy(currentStageInstance);

        currentStageInstance = null;
        normalRespawnPoints.Clear();
        redRespawnPoints.Clear();
        blueRespawnPoints.Clear();
    }
}

/// <summary>
/// リスポーンモード
/// </summary>
public enum RespawnMode {
    Random,
    Team
}