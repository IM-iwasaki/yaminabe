using UnityEngine;

/// <summary>
/// ESCキーでメニューUIを開閉し、
/// ゲーム終了ボタンで自分だけゲームを終了するためのスクリプト。
/// オンライン中でも他プレイヤーに影響しない安全設計。
/// </summary>
public class PauseMenu : MonoBehaviour {
    [Header("メニューUIの親オブジェクト（Canvas）")]
    // メニュー全体（背景パネル＋ボタン類）をまとめたCanvasを指定
    public GameObject menuUI;

    // 現在メニューが開いているかどうか
    private bool isMenuActive = false;

    void Start() {
        // ゲーム開始時はメニューを非表示にする
        if (menuUI != null)
            menuUI.SetActive(false);
    }

    void Update() {
        // ESCキーを押したらメニューの表示/非表示を切り替え
        if (Input.GetKeyDown(KeyCode.Escape)) {
            ToggleMenu();
        }
    }

    /// <summary>
    /// メニューの開閉を切り替える処理
    /// </summary>
    public void ToggleMenu() {
        isMenuActive = !isMenuActive;           // 状態を反転
        menuUI.SetActive(isMenuActive);         // UIを有効/無効に切り替え
        // メニューを開いている時は操作を効かなくするあとUIとかも非表示にしたい

    }

    /// <summary>
    /// ゲームを終了する処理
    /// 自分のクライアントだけが終了する（他プレイヤーに影響なし）
    /// </summary>
    public void QuitGame() {


        // プレイヤーがゲームから向けたら通知するの追加する
#if UNITY_EDITOR
        // Unityエディタ上では再生モードを停止
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // ビルド後はアプリケーションを終了
        Application.Quit();
#endif
    }
}
