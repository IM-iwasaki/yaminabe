using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ガチャシステム
/// 単発ガチャ・10連ガチャの抽選処理と結果表示を担当
/// </summary>
public class GachaSystem : MonoBehaviour {
    private readonly string SKIN_TAG = "Skin"; // プレイヤーのスキンオブジェクトを判定するタグ

    [Header("ガチャ設定")]
    public GachaData data;

    [Header("単発ガチャの価格")]
    [Min(0)]
    public int gachaCost = 100;

    [Header("カメラ制御")]
    [SerializeField] private CameraChangeController cameraManager; // 選択画面用カメラ移動
    [SerializeField] private Transform cameraTargetPoint;           // 選択画面カメラ位置

    [Header("UI")]
    [SerializeField] private GameObject gachaUI;                  // ガチャ選択画面UI

    [Header("ガチャアニメーション")]
    [SerializeField] private Animator gachaAnim;                  // ガチャ演出用アニメーター

    private GameObject currentPlayer; // 現在選択中のプレイヤー
    public bool isOpen;               // ガチャ画面の開閉状態およびカーソル状態

    /// <summary>
    /// ガチャ画面中かどうかのフラグ
    /// </summary>
    private bool isGacha = false;

    // 結果表示用
    private Canvas resultCanvas;      // 結果UIを配置するCanvas
    private Camera resultCamera;      // 結果を描画するためのカメラ

    private void Awake() {
        // 最初はガチャUIを非表示
        gachaUI.SetActive(false);
    }

    private void Update() {
        // ガチャ画面が閉じられた場合、結果Canvasを自動で破棄
        if (!isOpen && resultCanvas != null) {
            Destroy(resultCanvas.gameObject);
            resultCanvas = null;
        }
    }

    public event Action<GachaItem> OnItemPulled; // ガチャ抽選結果通知イベント

    #region ガチャ抽選

    /// <summary>
    /// 単発ガチャを引く
    /// </summary>
    public GachaItem PullSingle() {
        if (PlayerWallet.Instance == null) return null;

        // 所持金チェックと支払い
        if (!PlayerWallet.Instance.SpendMoney(gachaCost)) return null;

        // ガチャ演出
        StartCoroutine(PlayGachaAnimation());

        // 抽選処理
        var item = PullSingleInternal();
        if (item != null) {
            PlayerItemManager.Instance.UnlockGachaItem(item); // アイテム解放
            StartCoroutine(ShowSingleResult(item));           // 結果表示
        }

        OnItemPulled?.Invoke(item); // イベント通知
        return item;
    }

    /// <summary>
    /// 複数回（10連など）ガチャを引く
    /// </summary>
    public List<GachaItem> PullMultiple(int count) {
        List<GachaItem> results = new();
        if (PlayerWallet.Instance == null || count <= 0) return results;

        int totalCost = gachaCost * count;
        if (!PlayerWallet.Instance.SpendMoney(totalCost))
            return results;

        StartCoroutine(PlayGachaAnimation());

        // 指定回数分抽選
        for (int i = 0; i < count; i++) {
            var item = PullSingleInternal();
            if (item != null) {
                results.Add(item);
                PlayerItemManager.Instance.UnlockGachaItem(item);
            }
        }

        // 結果表示
        StartCoroutine(ShowMultipleResults(results));

        return results;
    }

    /// <summary>
    /// ガチャ抽選の内部処理
    /// レアリティ→アイテムの順でランダム抽選
    /// </summary>
    private GachaItem PullSingleInternal() {
        if (data == null) return null;

        // レアリティ抽選
        int roll = UnityEngine.Random.Range(0, 100);
        int current = 0;
        Rarity selectedRarity = Rarity.Common;

        foreach (var r in data.rarityRates) {
            current += r.rate;
            if (roll < current) {
                selectedRarity = r.rarity;
                break;
            }
        }

        // レアリティに応じたアイテムプール取得
        var pool = data.GetItemsByRarity(selectedRarity);
        if (pool == null || pool.Count == 0) return null;

        // アイテムの個別出現率に応じた抽選
        int totalRate = 0;
        foreach (var item in pool) totalRate += item.rate;
        if (totalRate <= 0) return null;

        int randomValue = UnityEngine.Random.Range(0, totalRate);
        int currentWeight = 0;
        foreach (var item in pool) {
            currentWeight += item.rate;
            if (randomValue < currentWeight) {
                return item;
            }
        }

        return null;
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

    /// <summary>
    /// ガチャ状態をまとめて切り替える
    /// </summary>
    private void SetGachaState(bool open) {
        isGacha = open;
    }

    // ガチャの状態を返す
    public bool IsGachaActive() {
        return isGacha;
    }

    #endregion

    #region ガチャ画面モード

    /// <summary>
    /// ガチャ選択画面開始
    /// </summary>
    public void StartGachaSelect(GameObject player) {

        // OptionMenu が開いているならガチャを開かない
        if (IsBlockedByOptionMenu()) {
            return;
        }

        if (currentPlayer != null) return;
        currentPlayer = player;

        SetGachaState(true);

        // ガチャ開始時に所持金UIを表示
        PlayerWallet.Instance?.ShowMoneyUI();

        if (gachaUI != null) gachaUI.SetActive(false);

        if (cameraManager != null && cameraTargetPoint != null) {
            cameraManager.MoveCamera(player, cameraTargetPoint.position, cameraTargetPoint.rotation);
        }

        Transform skin = FindChildWithTag(currentPlayer.transform, SKIN_TAG);
        if (skin != null) skin.gameObject.SetActive(false);

        isOpen = true;
        ChangeCursorView();

        if (gachaUI != null)
            StartCoroutine(ShowUIAfterDelay(cameraManager));
    }

    /// <summary>
    /// ガチャ選択画面終了
    /// </summary>
    public void EndGachaSelect() {
        if (currentPlayer == null) return;

        // ガチャ終了時に所持金UIを非表示
        PlayerWallet.Instance?.HideMoneyUI();

        OffGachaAnim();

        if (gachaUI != null) {
            gachaUI.SetActive(false);
        }

        Transform skin = FindChildWithTag(currentPlayer.transform, SKIN_TAG);
        if (skin != null) skin.gameObject.SetActive(true);

        if (cameraManager != null) cameraManager.ReturnCamera();

        isOpen = false;
        ChangeCursorView();

        SetGachaState(false);

        currentPlayer = null;
    }

    private IEnumerator ShowUIAfterDelay(CameraChangeController camController) {
        float duration = camController != null ? camController.moveDuration : 1.5f;
        yield return new WaitForSeconds(duration);

        if (gachaUI != null)
            gachaUI.SetActive(true);
    }

    private Transform FindChildWithTag(Transform parent, string tag) {
        foreach (Transform child in parent) {
            if (child.CompareTag(tag)) return child;

            Transform found = FindChildWithTag(child, tag);
            if (found != null) return found;
        }
        return null;
    }

    #endregion

    #region カーソル制御

    private void ChangeCursorView() {
        Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;
    }

    #endregion

    #region ガチャアニメーション

    private void OnGachaAnim() => gachaAnim.SetBool("Open", true);
    private void OffGachaAnim() => gachaAnim.SetBool("Open", false);

    private IEnumerator PlayGachaAnimation() {
        OffGachaAnim();
        yield return null;
        OnGachaAnim();
    }

    #endregion

    #region 結果表示

    /// <summary>
    /// 単発ガチャ結果のRawImage表示
    /// </summary>
    private IEnumerator ShowSingleResult(GachaItem item) {
        if (item == null || item.resultPrefab == null) yield break;

        float iconSize = 256f;

        // Canvas生成
        if (resultCanvas == null) {
            GameObject canvasObj = new GameObject("ResultCanvas");
            resultCanvas = canvasObj.AddComponent<Canvas>();
            resultCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // RawImage生成
        GameObject iconObj = new GameObject(item.itemName);
        iconObj.transform.SetParent(resultCanvas.transform, false);
        RawImage img = iconObj.AddComponent<RawImage>();
        RectTransform rt = iconObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(iconSize, iconSize);
        rt.anchoredPosition = Vector2.zero;

        // プレハブを一時生成してRenderTextureに描画
        GameObject temp = Instantiate(item.resultPrefab, Vector3.zero, Quaternion.identity);
        temp.SetActive(true);

        RenderTexture rtTex = new RenderTexture((int)iconSize, (int)iconSize, 16);
        if (resultCamera == null) {
            GameObject camObj = new GameObject("ResultCamera");
            resultCamera = camObj.AddComponent<Camera>();
            resultCamera.clearFlags = CameraClearFlags.SolidColor;
            resultCamera.backgroundColor = Color.black;
            resultCamera.enabled = false;
        }
        resultCamera.targetTexture = rtTex;

        // カメラ配置（正面から）
        Vector3 offset = temp.transform.forward * 2f + Vector3.up * 1f;
        resultCamera.transform.position = temp.transform.position + offset;
        resultCamera.transform.LookAt(temp.transform.position + Vector3.up * 1f);

        resultCamera.Render();

        img.texture = rtTex;
        resultCamera.targetTexture = null;

        Destroy(temp);

        yield return null;
    }

    /// <summary>
    /// 10連など複数ガチャ結果のGridLayout表示
    /// </summary>
    private IEnumerator ShowMultipleResults(List<GachaItem> items) {
        if (items == null || items.Count == 0) yield break;

        float iconSize = 256f;
        float spacing = 10f;

        // Canvas生成
        if (resultCanvas == null) {
            GameObject canvasObj = new GameObject("ResultCanvas");
            resultCanvas = canvasObj.AddComponent<Canvas>();
            resultCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // GridParent生成
        GameObject gridParent = new GameObject("GachaResultGrid");
        gridParent.transform.SetParent(resultCanvas.transform, false);
        RectTransform rt = gridParent.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(5 * iconSize + 4 * spacing, 2 * iconSize + spacing);
        rt.anchoredPosition = Vector2.zero;

        GridLayoutGroup grid = gridParent.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(iconSize, iconSize);
        grid.spacing = new Vector2(spacing, spacing);
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 5;

        // カメラ生成
        if (resultCamera == null) {
            GameObject camObj = new GameObject("ResultCamera");
            resultCamera = camObj.AddComponent<Camera>();
            resultCamera.clearFlags = CameraClearFlags.SolidColor;
            resultCamera.backgroundColor = Color.black;
            resultCamera.enabled = false;
        }

        // 各アイテムをRawImageに描画
        foreach (var item in items) {
            if (item.resultPrefab == null) continue;

            GameObject iconObj = new GameObject(item.itemName);
            iconObj.transform.SetParent(gridParent.transform, false);
            RawImage img = iconObj.AddComponent<RawImage>();

            GameObject temp = Instantiate(item.resultPrefab, Vector3.zero, Quaternion.identity);
            temp.SetActive(true);

            RenderTexture rtTex = new RenderTexture((int)iconSize, (int)iconSize, 16);
            resultCamera.targetTexture = rtTex;

            Vector3 offset = temp.transform.forward * 2f + Vector3.up * 1f;
            resultCamera.transform.position = temp.transform.position + offset;
            resultCamera.transform.LookAt(temp.transform.position + Vector3.up * 1f);

            resultCamera.Render();

            img.texture = rtTex;
            resultCamera.targetTexture = null;

            Destroy(temp);

            yield return new WaitForEndOfFrame();
        }
    }

    #endregion
}