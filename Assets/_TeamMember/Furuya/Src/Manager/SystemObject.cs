using UnityEngine;

//マネージャーを生成する際システムオブジェクトを継承すること
//public class マネージャー名 : SystemObject<マネージャー名>
//Startの代わりにInitializeを使うこと

public abstract class SystemObject<T> : MonoBehaviour where T : Component {
    private static T _instance;

    public static T Instance {
        get {
            if (_instance == null) {
                // シーン内に既に存在するか確認
                _instance = FindObjectOfType<T>();

                if (_instance == null) {
                    // 存在しない場合は新しく生成
                    GameObject obj = new GameObject(typeof(T).Name);
                    _instance = obj.AddComponent<T>();
                    DontDestroyOnLoad(obj); // シーンをまたいで保持
                }
            }
            return _instance;
        }
    }

    protected virtual void Awake() {
        if (_instance == null) {
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this) {
            Destroy(gameObject); // 重複インスタンスを防止
        }
    }

    //初期化処理
    public virtual void Initialize() { }
}