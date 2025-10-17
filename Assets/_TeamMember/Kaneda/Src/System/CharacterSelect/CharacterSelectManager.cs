using UnityEngine;

/// <summary>
/// キャラクター選択モード管理マネージャー
/// ・UI表示/非表示、プレイヤー操作停止、CameraChangeController呼び出しを管理
/// </summary>
public class CharacterSelectManager : MonoBehaviour {
    #region 変数定義

    [Header("カメラ制御")]
    [SerializeField] private CameraChangeController cameraManager;  // カメラ移動用Controller
    [SerializeField] private Transform cameraTargetPoint;           // 選択画面カメラ位置

    [Header("UI")]
    [SerializeField] private GameObject selectUI;                  // 選択画面UI

    private GameObject currentPlayer; // 現在選択中のプレイヤー

    //  キャラクターを毎秒どれだけ回転させるか
    private Vector3 characterRotation = new Vector3(0, 50f, 0);
    [Header("オブジェクトを回す")]
    [SerializeField] private GameObject rotateObject;
    #endregion

    #region Awake,Start,Update
    private void Awake() {
        selectUI.SetActive(false);
    }

    private void Update() {
        //  オブジェクトを回転させる
        rotateObject.transform.Rotate(characterRotation * Time.deltaTime);
    }
    #endregion

    #region キャラクター選択時のUI表示非表示、カメラの挙動
    /// <summary>
    /// キャラクター選択モードを開始
    /// </summary>
    /// <param name="player">操作中のプレイヤー</param>
    public void StartCharacterSelect(GameObject player) {
        if (currentPlayer != null) return;
        currentPlayer = player;

        // プレイヤー操作停止
        //  後で入れ込む

        // UIを非表示（移動開始前）
        if (selectUI != null)
            selectUI.SetActive(false);

        // カメラ移動開始
        if (cameraManager != null && cameraTargetPoint != null) {
            cameraManager.MoveCamera(
                player,
                cameraTargetPoint.position,
                cameraTargetPoint.rotation
            );
        }

        // 移動完了後にUIを表示する場合は、
        // CameraChangeControllerのコルーチンが終わったタイミングで呼ぶか
        // ここで遅延コルーチンを追加しても良い
        if (selectUI != null)
            StartCoroutine(ShowUIAfterDelay(cameraManager));
    }

    /// <summary>
    /// キャラクター選択モードを終了
    /// </summary>
    public void EndCharacterSelect() {
        if (currentPlayer == null) return;

        // UIを非表示（戻る操作開始時）
        if (selectUI != null)
            selectUI.SetActive(false);

        // カメラを戻す
        if (cameraManager != null)
            cameraManager.ReturnCamera();

        // プレイヤー操作再開
        //  後で入れ込む

        currentPlayer = null;
    }

    /// <summary>
    /// 遅延してUIを表示（カメラ移動完了後にUIを表示する補助）
    /// </summary>
    private System.Collections.IEnumerator ShowUIAfterDelay(CameraChangeController camController) {
        // CameraChangeController の移動時間と同じだけ待つ
        float duration = camController != null ? camController.moveDuration : 1.5f;
        yield return new WaitForSeconds(duration);

        if (selectUI != null)
            selectUI.SetActive(true);
    }
    #endregion

}
