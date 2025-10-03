// NetworkSystemObject.cs
using UnityEngine;
using Mirror;

/// <summary>
/// ネットワーク上で「サーバーが生成して一意に存在する」タイプのシステム基底。
/// サーバー上で Spawn されることを前提にする（NetworkServer.Spawn）。
/// OnStartServer で自動的に Initialize() を呼びます。
/// クライアント側でも Spawn によって生成されたときに Instance にセットされます。
/// </summary>
public abstract class NetworkSystemObject<T> : NetworkBehaviour, ISystem where T : NetworkSystemObject<T> {
    private static T _instance;
    public static T Instance => _instance;

    [SerializeField, Tooltip("初期化の優先度 (小さいほど先に初期化)")]
    protected int initializationOrder = 0;
    public virtual int InitializationOrder => initializationOrder;

    protected virtual void Awake() {
        DontDestroyOnLoad(gameObject);
    }

    // サーバー側で Spawn されたとき（専権的に初期化）
    public override void OnStartServer() {
        base.OnStartServer();
        if (_instance == null) _instance = this as T;
        Initialize(); // サーバー側専用の初期化
    }

    // クライアント側に Spawn されたときの参照セット
    public override void OnStartClient() {
        base.OnStartClient();
        // クライアントの静的参照も保持しておく（読み取り専用的な使い方が可能）
        if (_instance == null) _instance = this as T;
        // クライアント側で実行したい初期化があれば Override して使う
        OnClientInitialized();
    }

    public override void OnStopClient() {
        base.OnStopClient();
        if (_instance == this) _instance = null;
    }

    /// <summary>サーバー側の共通初期化（派生でオーバーライド）</summary>
    public virtual void Initialize() { }

    /// <summary>クライアント側で Spawn された際に必要な処理（任意）</summary>
    protected virtual void OnClientInitialized() { }
}
