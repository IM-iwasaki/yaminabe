using UnityEngine;
using TMPro;
using System;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// プレイヤーのお金を管理するクラス
/// シングルトン構造でゲーム全体から参照可能
/// </summary>
public class PlayerWallet : MonoBehaviour {
    // シングルトン
    public static PlayerWallet Instance { get; private set; }

    [Header("初期設定")]
    [SerializeField] private int startMoney = 0;

    [Header("現在の所持金")]
    [SerializeField] private int currentMoney;

    /// <summary>
    /// 所持金が変化したときに呼ばれるイベント
    /// </summary>
    public event Action<int> OnMoneyChanged;

    // UI用
    private Canvas moneyCanvas;
    private TextMeshProUGUI moneyText;

    // ガチャ中など、所持金UIを常時表示するかどうか
    private bool keepMoneyUIVisible = false;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 起動時にはUIを生成しない
        LoadMoney();
    }  

    /// <summary>
    /// 現在の所持金を取得
    /// </summary>
    public int GetMoney() => currentMoney;

    /// <summary>
    /// お金を追加する（マイナスも可）
    /// </summary>
    public void AddMoney(int amount) {
        currentMoney += amount;
        if (currentMoney < 0) currentMoney = 0;

        OnMoneyChanged?.Invoke(currentMoney);
        SaveMoney();

        UpdateMoneyText();
        ShowFloatingMoney(amount);
    }

    /// <summary>
    /// 指定した金額を支払う
    /// </summary>
    public bool SpendMoney(int amount) {
        if (amount <= 0) return false;
        if (currentMoney < amount) return false;

        currentMoney -= amount;
        if (currentMoney < 0) currentMoney = 0;

        OnMoneyChanged?.Invoke(currentMoney);
        PlayerItemManager.Instance?.UpdateMoney(currentMoney);

        UpdateMoneyText();
        ShowFloatingMoney(-amount);

        return true;
    }

    /// <summary>
    /// 所持金のリセット
    /// </summary>
    public void ResetMoney() {
        currentMoney = startMoney;
        OnMoneyChanged?.Invoke(currentMoney);
        SaveMoney();
        UpdateMoneyText();
    }

    /// <summary>
    /// 所持金のセーブ
    /// </summary>
    private void SaveMoney() {
        var data = PlayerSaveData.Load();
        data.currentMoney = currentMoney;
        PlayerSaveData.Save(data);
    }

    /// <summary>
    /// セーブされている所持金のロード
    /// </summary>
    private void LoadMoney() {
        var data = PlayerSaveData.Load();
        currentMoney = data.currentMoney;
        OnMoneyChanged?.Invoke(currentMoney);
    }

    /// <summary>
    /// ガチャ開始時に所持金UIを表示
    /// </summary>
    public void ShowMoneyUI() {
        CreateMoneyUI();
        keepMoneyUIVisible = true;
        UpdateMoneyText();
        moneyText.gameObject.SetActive(true);
    }

    /// <summary>
    /// ガチャ終了時に所持金UIを非表示
    /// </summary>
    public void HideMoneyUI() {
        keepMoneyUIVisible = false;
        if (moneyText != null)
            moneyText.gameObject.SetActive(false);
    }
    #region 所持金UI
    /// <summary>
    /// 所持金表示用CanvasとTMP Textを生成
    /// </summary>
    private void CreateMoneyUI() {
        // 既に生成済みなら何もしない
        if (moneyCanvas != null) return;

        // Canvas作成
        GameObject canvasGO = new GameObject("MoneyCanvas");
        moneyCanvas = canvasGO.AddComponent<Canvas>();
        moneyCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();
        DontDestroyOnLoad(canvasGO);

        // TMP Text作成
        GameObject textGO = new GameObject("MoneyText");
        textGO.transform.SetParent(canvasGO.transform);
        moneyText = textGO.AddComponent<TextMeshProUGUI>();
        moneyText.fontSize = 72;
        moneyText.alignment = TextAlignmentOptions.TopLeft;
        moneyText.color = Color.yellow;

        // 左上に固定
        RectTransform rt = moneyText.rectTransform;
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(10, -10);

        // 横幅固定で改行しない
        moneyText.enableWordWrapping = false;

        // 最初は非表示
        moneyText.gameObject.SetActive(false);
    }

    /// <summary>
    /// 所持金テキストの更新
    /// </summary>
    private void UpdateMoneyText() {
        if (moneyText == null) return;
        moneyText.text = $"Money: {currentMoney}";
    }
    #endregion

    #region 増減UI
    /// <summary>
    /// フローティング表示（±○○）
    /// </summary>
    private void ShowFloatingMoney(int amount) {
        if (moneyText == null) return;

        // フローティング用Text生成
        GameObject floatGO = new GameObject("FloatingMoneyText");
        floatGO.transform.SetParent(moneyText.transform.parent, false);

        TextMeshProUGUI floatText = floatGO.AddComponent<TextMeshProUGUI>();
        floatText.fontSize = 72;
        floatText.alignment = TextAlignmentOptions.TopLeft;

        string sign = amount > 0 ? "+" : "";
        floatText.text = $"{sign}{amount}";

        floatText.color = amount > 0 ? Color.yellow : Color.red;

        RectTransform rt = floatText.rectTransform;

        // 所持金UIの幅に応じて右へ自動オフセット
        float offsetX = moneyText.preferredWidth + 20f;
        rt.anchorMin = moneyText.rectTransform.anchorMin;
        rt.anchorMax = moneyText.rectTransform.anchorMax;
        rt.pivot = moneyText.rectTransform.pivot;
        rt.anchoredPosition =
            moneyText.rectTransform.anchoredPosition
            + new Vector2(offsetX, -20f);

        StartCoroutine(FloatingAnimation(floatText));
    }

    /// <summary>
    /// フローティングアニメーション
    /// </summary>
    private IEnumerator FloatingAnimation(TextMeshProUGUI text) {
        float duration = 1.5f;
        float timer = 0f;

        Vector2 startPos = text.rectTransform.anchoredPosition;
        Color startColor = text.color;

        while (timer < duration) {
            timer += Time.deltaTime;
            float t = timer / duration;

            // 上に移動
            text.rectTransform.anchoredPosition =
                startPos + Vector2.up * 60f * t;

            // フェードアウト
            Color c = startColor;
            c.a = Mathf.Lerp(1f, 0f, t);
            text.color = c;

            yield return null;
        }

        Destroy(text.gameObject);
    }
    #endregion
}