using UnityEngine;
/// <summary>
/// 各クライアントに配置するロードで壊されないUI
/// </summary>
public class GameUI : MonoBehaviour
{
    /// <summary>
    /// インスタンス
    /// </summary>
    public static GameUI instance;
    private void Awake() {
        DontDestroyOnLoad(gameObject);
        if (instance == null) {
            instance = this;
        }
        else {
            Destroy(gameObject);
        }

    }
}
