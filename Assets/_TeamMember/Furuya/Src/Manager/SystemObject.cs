// SystemObject.cs
using UnityEngine;

/// <summary>
/// 通常の（非ネットワーク）シングルトン用ベースクラス。
/// Initialize() と InitializationOrder を提供する。
/// </summary>
public abstract class SystemObject<T> : MonoBehaviour, ISystem where T : Component {
    private static T _instance;
    public static T Instance {
        get {
            if (_instance == null) {
                _instance = FindObjectOfType<T>();
                if (_instance == null) {
                    GameObject obj = new GameObject(typeof(T).Name);
                    _instance = obj.AddComponent<T>();
                    DontDestroyOnLoad(obj);
                }
            }
            return _instance;
        }
    }

    [SerializeField, Tooltip("初期化の優先度 (小さいほど先に初期化)")]
    protected int initializationOrder = 0;
    public virtual int InitializationOrder => initializationOrder;

    protected virtual void Awake() {
        if (_instance == null) {
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this) {
            Destroy(gameObject);
            return;
        }
    }

    /// <summary>派生先で初期化処理を実装する（Start の代わりに使う）</summary>
    public virtual void Initialize() { }
}
