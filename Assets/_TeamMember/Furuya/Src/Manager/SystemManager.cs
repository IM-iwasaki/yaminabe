// SystemManager.cs
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;

/// <summary>
/// Inspectorでローカル系・ネットワーク系の System プレハブを登録しておくと
/// 起動時に順序を考慮して初期化してくれる。Network系はサーバー側で Spawn される想定。
/// </summary>
[DefaultExecutionOrder(-1000)]
public class SystemManager : MonoBehaviour {
    public static SystemManager Instance { get; private set; }

    [Header("ローカル系 SystemObject プレハブ (SystemObject<T> を継承したもの)")]
    [SerializeField] private List<GameObject> systemObjectPrefabs;

    [Header("Network系 System プレハブ (NetworkSystemObject<T> を継承したもの) — サーバーで Spawn して一意にする")]
    [SerializeField] private List<GameObject> networkSystemPrefabs;

    void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else {
            Destroy(gameObject);
            return;
        }

        InstantiateLocalSystems();

        // 既に NetworkServer がアクティブならすぐ Spawn する（ホスト起動など）
        if (NetworkServer.active) {
            SpawnNetworkSystems();
        }
        // dedicated server またはホストが後から起動するケースは NetworkManager.OnStartServer() から SpawnNetworkSystems() を呼んでください。
    }


    /// <summary>
    /// ローカル用システムオブジェクトを生成
    /// </summary>
    void InstantiateLocalSystems() {
        if (systemObjectPrefabs == null || systemObjectPrefabs.Count == 0) return;

        var collected = new List<ISystem>();
        foreach (var prefab in systemObjectPrefabs) {
            if (prefab == null) { Debug.LogWarning("systemObjectPrefabs に null が含まれています"); continue; }
            GameObject go = Instantiate(prefab);
            DontDestroyOnLoad(go);
            collected.AddRange(go.GetComponentsInChildren<ISystem>(true));
        }

        // 初期化順に呼ぶ
        foreach (var sys in collected.OrderBy(s => s.InitializationOrder)) {
            try {
                sys.Initialize();
            }
            catch (System.Exception ex) {
                Debug.LogError($"System Initialize で例外: {sys.GetType().Name} - {ex}");
            }
        }
    }

    /// <summary>
    /// サーバー側から呼んで Network 系の System プレハブを Spawn する。
    /// ※ NetworkServer がアクティブであること（サーバー起動後）に呼んでください。
    /// </summary>
    public void SpawnNetworkSystems() {
        if (!NetworkServer.active) {
            Debug.LogWarning("SpawnNetworkSystems はサーバーで呼んでください (NetworkServer.active が false)");
            return;
        }
        if (networkSystemPrefabs == null || networkSystemPrefabs.Count == 0) return;

        foreach (var prefab in networkSystemPrefabs) {
            if (prefab == null) { Debug.LogWarning("networkSystemPrefabs に null が含まれています"); continue; }
            GameObject go = Instantiate(prefab);
            // NetworkIdentity を持っていることを前提に Spawn
            NetworkServer.Spawn(go);
            // NetworkSystemObject の OnStartServer() で Initialize() を呼んでいるので
            // ここで明示的な Initialize 呼び出しは不要（重複防止）
        }
    }
}
