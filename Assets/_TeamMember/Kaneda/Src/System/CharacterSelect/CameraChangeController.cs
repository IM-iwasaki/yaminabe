using Mirror;
using System.Collections;
using UnityEngine;

/// <summary>
/// サブカメラを使ったスムーズカメラ切替のみ担当
/// </summary>
public class CameraChangeController : MonoBehaviour {

    [Header("カメラ移動設定")]
    [SerializeField] public float moveDuration = 1.5f;                     // 補間時間
    [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // 補間カーブ

    [Header("生成するサブカメラ")]
    [SerializeField] private Camera subCamera = null;
    [Header("サブカメラを保管する親オブジェクト")]
    [SerializeField] private GameObject parentSubCamera = null;

    // 内部用
    private Vector3 returnPosition;       // プレイヤー子カメラ位置（戻り先）
    private Quaternion returnRotation;    // プレイヤー子カメラ回転
    private Camera playerCamera;          // プレイヤーのカメラ保存
    private bool isTransitioning = false; // 移動中かどうか
    private Camera subCameraObject;       // サブカメラをOnOffする用

    private void Awake() {
        subCamera.gameObject.SetActive(false);
    }

    /// <summary>
    /// カメラを指定位置へ移動
    /// </summary>
    /// <param name="player">プレイヤーオブジェクト</param>
    /// <param name="targetPosition">移動先位置</param>
    /// <param name="targetRotation">移動先回転</param>
    public void MoveCamera(GameObject player, Vector3 targetPosition, Quaternion targetRotation) {
        if (isTransitioning) return;

        // プレイヤーの子カメラ取得
        playerCamera = player.GetComponentInChildren<Camera>();
        if (playerCamera == null) {
            Debug.LogWarning("プレイヤーの子にカメラが見つかりません。");
            return;
        }

        //  サブカメラを生成
        InstantiateSubCamera(subCamera, parentSubCamera);

        // 戻り位置を保存
        returnPosition = playerCamera.transform.position;
        returnRotation = playerCamera.transform.rotation;

        //  サブカメラを起動、メインカメラを停止
        subCameraObject.gameObject.SetActive(true);
        //playerCamera.gameObject.SetActive(false);

        // 移動コルーチン開始
        StartCoroutine(MoveCameraCoroutine(targetPosition, targetRotation));
    }

    /// <summary>
    /// カメラを元の位置に戻す
    /// </summary>
    public void ReturnCamera() {
        if (isTransitioning) return;

        // 移動コルーチン開始（戻り）
        StartCoroutine(MoveCameraCoroutine(returnPosition, returnRotation, playerCamera));
    }

    /// <summary>
    /// カメラ移動用コルーチン
    /// </summary>
    private IEnumerator MoveCameraCoroutine(Vector3 targetPos, Quaternion targetRot, Camera playerCamera = null) {
        isTransitioning = true;

        Vector3 startPos = subCameraObject.transform.position;
        Quaternion startRot = subCameraObject.transform.rotation;

        float elapsed = 0f;
        while (elapsed < moveDuration) {
            elapsed += Time.deltaTime;
            float t = moveCurve.Evaluate(elapsed / moveDuration);

            subCameraObject.transform.position = Vector3.Lerp(startPos, targetPos, t);
            subCameraObject.transform.rotation = Quaternion.Slerp(startRot, targetRot, t);

            yield return null;
        }

        // 最終補正
        subCameraObject.transform.position = targetPos;
        subCameraObject.transform.rotation = targetRot;

        //  プレイヤーの子カメラがあれば有効化
        if (playerCamera != null) {
            //  メインカメラを起動、サブカメラを停止
            playerCamera.gameObject.SetActive(true);
            subCameraObject.gameObject.SetActive(false);
            Destroy(subCameraObject.gameObject);
            //  取得していたカメラを空にする
            playerCamera = null;
        }

        isTransitioning = false;
    }

    /// <summary>
    /// サブカメラ生成
    /// </summary>
    private void InstantiateSubCamera(Camera subCamera, GameObject parent) {
        //  サブカメラを生成
        if (subCamera == null) {
            Debug.LogWarning("サブカメラがありません");
            return;
        }
        subCameraObject = Instantiate(subCamera, parent.transform);
        //  サブカメラをプレイヤーのカメラの位置に
        subCameraObject.transform.position = playerCamera.transform.position;
        subCameraObject.transform.rotation = playerCamera.transform.rotation;
    }
}
