using UnityEngine;
using Mirror;

/// <summary>
/// エディタでリザルト画面を確認するためのテストスクリプト
/// </summary>
public class ResultDebug : NetworkBehaviour {
    private ResultManager resultManager;

    void Start() {
        resultManager = FindObjectOfType<ResultManager>();
        if (resultManager == null)
            Debug.LogError("ResultManager がシーンに存在しません");
    }

    void Update() {
        if (Input.GetKeyUp(KeyCode.Escape)) {
            // Hostのみゲーム終了をテスト
            if (isServer) {
                Debug.Log("ESC押下 → ゲーム終了テスト（サーバー呼び出し）");
                resultManager.RpcShowResult(); // 全員にリザルト表示
            }
        }
    }
}
