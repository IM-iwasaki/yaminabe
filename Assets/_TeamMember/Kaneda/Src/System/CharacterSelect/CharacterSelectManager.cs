using Mirror;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

/// <summary>
/// キャラクター選択モード管理マネージャー
/// ・UI表示/非表示、プレイヤー操作停止、CameraChangeController呼び出しを管理
/// </summary>
public class CharacterSelectManager : NetworkBehaviour {

    #region 変数定義

    //  定数
    private readonly string SKIN_TAG = "Skin";

    //  変数
    [Header("カメラ制御")]
    // カメラ移動用Controller
    [SerializeField] private CameraChangeController cameraManager;
    // 選択画面カメラ位置
    [SerializeField] private Transform cameraTargetPoint;
    // 選択画面UI
    [Header("UI")]
    [SerializeField] private GameObject selectUI;

    [Header("セレクトオブジェクト")]
    [SerializeField] private SelectObjectManager selectObj;

    // 現在選択中のプレイヤー
    private GameObject currentPlayer;

    //  カーソルのOnOff
    private bool isOpen;

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

        // OptionMenu が開いているならキャラ選択を開かない
        if (IsBlockedByOptionMenu()) {
            return;
        }
        if (currentPlayer != null) return;
        currentPlayer = player;


        SetCharacterSelectState(true);

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

        //  プレイヤーの見た目非表示
        Transform parent = player.transform;
        Transform skin = FindChildWithTag(parent, SKIN_TAG);
        skin.gameObject.SetActive(false);

        //  カーソルをOnにする
        ChangeCursorView();

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

        AudioManager.Instance.CmdPlayUISE("決定");
        selectObj.ConfirmPlayerChange(currentPlayer);

        // UIを非表示（戻る操作開始時）
        if (selectUI != null)
            selectUI.SetActive(false);



        // カメラを戻す
        if (cameraManager != null)
            cameraManager.ReturnCamera();

        //  プレイヤーの見た目表示
        Transform parent = currentPlayer.transform;
        Transform skin = FindChildWithTag(parent, SKIN_TAG);
        skin.gameObject.SetActive(true);

        //  プレイヤー側のローカルUIを表示させる
        if (currentPlayer.GetComponent<PlayerLocalUIController>()) {
            currentPlayer.GetComponent<PlayerLocalUIController>().OnLocalUIObject();
        }

        //  カーソルをOffにする
        ChangeCursorView();

        SetCharacterSelectState(false);
        currentPlayer = null;
    }

    /// <summary>
    /// 遅延してUIを表示（カメラ移動完了後にUIを表示する補助）
    /// </summary>
    /// <param name="camController"></param>
    /// <returns></returns>
    private System.Collections.IEnumerator ShowUIAfterDelay(CameraChangeController camController) {
        // CameraChangeController の移動時間と同じだけ待つ
        float duration = camController != null ? camController.moveDuration : 1.5f;
        yield return new WaitForSeconds(duration);

        if (selectUI != null)
            selectUI.SetActive(true);
    }

    /// <summary>
    /// 指定した親以下から特定のタグを持つ全ての子オブジェクトをリストで取得
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="tag"></param>
    /// <returns></returns>
    private Transform FindChildWithTag(Transform parent, string tag) {

        // まず直接の子を確認
        foreach (Transform child in parent) {
            if (child.CompareTag(tag))
                return child;

            // 子の中も再帰的に探索
            Transform found = FindChildWithTag(child, tag);
            if (found != null)
                return found;
        }

        // 見つからなかった場合
        return null;
    }
    #endregion

    #region カーソルONOFF

    /// <summary>
    /// カーソルをOnOffする
    /// </summary>
    private void ChangeCursorView() {
        isOpen = !isOpen;

        Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;
    }

    #endregion

    #region キャラ選択中ブロック
    /// <summary>
    /// キャラ選択画面中かどうか
    /// true: キャラ選択モード / false: 通常
    /// </summary>
    public bool isCharacterSelect = false;

    /// <summary>
    /// キャラ選択状態をまとめて切り替える
    /// </summary>
    /// <param name="active">true なら選択画面中</param>
    private void SetCharacterSelectState(bool active) {
        isCharacterSelect = active;

        // 将来「キャラ選択中だけ有効にしたい処理」が増えたら
        // ここにまとめて書く
    }

    /// <summary>
    /// 現在キャラ選択画面中かどうかを外から確認する用
    /// </summary>
    public bool IsCharacterSelectActive() {
        return isCharacterSelect;
    }
    #endregion

    #region オプション中ブロック
    /// <summary>
    /// シーン内の OptionMenu をキャッシュするためのフィールド
    /// インスペクタからは設定しない
    /// </summary>
    private OptionMenu cachedOptionMenu;

    /// <summary>
    /// シーン内から OptionMenu を自動で探してくるゲッター
    /// 初回だけ FindObjectOfType し、その後はキャッシュを使う
    /// </summary>
    private OptionMenu Option {
        get {
            if (cachedOptionMenu == null) {
                cachedOptionMenu = FindObjectOfType<OptionMenu>();
            }
            return cachedOptionMenu;
        }
    }

    /// <summary>
    /// オプションメニューが開いているため
    /// ガチャを開けない状態かどうか
    /// </summary>
    public bool IsBlockedByOptionMenu() {
        // OptionMenu が無いならブロックしない
        if (Option == null) return false;

        // OptionMenu 側でメニューが開いているならブロック
        return Option.isOpen;
    }
    #endregion



}
