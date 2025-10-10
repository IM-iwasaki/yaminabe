using UnityEngine;
using System.Collections;

/// <summary>
/// サブカメラを使ったスムーズカメラ切替＋UI表示（UIはSetActiveで制御）
/// Fキーで切替、移動中は入力無効
/// </summary>
public class SubCameraSwitcherUI : MonoBehaviour {
    [Header("カメラターゲット")]
    public Transform subCameraTarget;   // サブカメラの移動先位置

    [Header("カメラ設定")]
    public Camera mainCamera;           // ゲーム用メインカメラ
    public Camera subCamera;            // 移動用サブカメラ

    [Header("遷移設定")]
    public float transitionDuration = 1f; // 補間時間

    [Header("UI")]
    public Canvas debugCanvas;          // UI Canvas

    private bool showingSubCamera = false; // 現在サブカメラか
    private bool isTransitioning = false;  // 移動中か

    void Start() {
        // サブカメラは最初非アクティブ
        subCamera.gameObject.SetActive(false);

        // UIは最初非表示
        debugCanvas.gameObject.SetActive(false);
    }

    void Update() {
        // 移動中でなければFキー判定
        if (!isTransitioning && Input.GetKeyDown(KeyCode.F)) {
            if (!showingSubCamera) {
                StartCoroutine(MoveSubCameraCoroutine(true));
            }
            else {
                StartCoroutine(MoveSubCameraCoroutine(false));
            }
        }
    }

    IEnumerator MoveSubCameraCoroutine(bool toSubCamera) {
        isTransitioning = true;

        if (toSubCamera) {
            // サブカメラをメインカメラ位置に瞬間移動
            subCamera.transform.position = mainCamera.transform.position;
            subCamera.transform.rotation = mainCamera.transform.rotation;
            subCamera.gameObject.SetActive(true); // サブカメラを有効
        }

        Vector3 startPos = subCamera.transform.position;
        Quaternion startRot = subCamera.transform.rotation;
        Vector3 targetPos = toSubCamera ? subCameraTarget.position : mainCamera.transform.position;
        Quaternion targetRot = toSubCamera ? subCameraTarget.rotation : mainCamera.transform.rotation;

        float elapsed = 0f;

        while (elapsed < transitionDuration) {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / transitionDuration);

            subCamera.transform.position = Vector3.Lerp(startPos, targetPos, t);
            subCamera.transform.rotation = Quaternion.Slerp(startRot, targetRot, t);

            // UIは移動中は非表示
            debugCanvas.gameObject.SetActive(false);

            yield return null;
        }

        // 最終位置に正確にセット
        subCamera.transform.position = targetPos;
        subCamera.transform.rotation = targetRot;

        if (toSubCamera) {
            // 移動完了後にUIを表示
            debugCanvas.gameObject.SetActive(true);
        }
        else {
            // 元に戻ったらサブカメラ非アクティブ
            subCamera.gameObject.SetActive(false);
            debugCanvas.gameObject.SetActive(false); // 必要に応じてUIも非表示
        }

        showingSubCamera = toSubCamera;
        isTransitioning = false;
    }
}
