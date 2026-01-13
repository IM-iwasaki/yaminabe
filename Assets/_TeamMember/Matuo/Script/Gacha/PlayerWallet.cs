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
    private Coroutine moneyDisplayCoroutine;

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
        moneyText.fontSize = 72; // 文字サイズ
        moneyText.alignment = TextAlignmentOptions.TopLeft;
        moneyText.color = Color.yellow;

        // 左上に固定
        moneyText.rectTransform.anchorMin = new Vector2(0, 1);
        moneyText.rectTransform.anchorMax = new Vector2(0, 1);
        moneyText.rectTransform.pivot = new Vector2(0, 1);
        moneyText.rectTransform.anchoredPosition = new Vector2(10, -10);

        // 横幅固定で改行しない
        moneyText.enableWordWrapping = false;

        // 最初は非表示
        moneyText.gameObject.SetActive(false);
    }

    /// <summary>
    /// 所持金テキストの更新
    /// changeAmountが0なら通常表示、正負で増減表示
    /// </summary>
    /// <param name="changeAmount">変化量</param>
    private void UpdateMoneyText(int changeAmount) {
        if (moneyText == null) return;

        if (changeAmount == 0) {
            moneyText.text = $"Money: {currentMoney}";
        } else {
            string sign = changeAmount > 0 ? "+" : "";
            moneyText.text = $"Money: {currentMoney - changeAmount} {sign}{changeAmount}";
        }
    }

    /// <summary>
    /// 増減分を一時的に表示し、1.5秒後に非表示に戻す
    /// </summary>
    private IEnumerator ShowMoneyChange(int changeAmount) {
        UpdateMoneyText(changeAmount);
        moneyText.gameObject.SetActive(true);

        yield return new WaitForSeconds(1.5f);

        // ガチャ中など常時表示モードでなければ非表示
        if (!keepMoneyUIVisible) {
            moneyText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 現在の所持金を取得
    /// </summary>
    public int GetMoney() => currentMoney;

    /// <summary>
    /// お金を追加する（マイナスも可）
    /// </summary>
    /// <param name="amount">追加する金額（負数で減算）</param>
    public void AddMoney(int amount) {
        int oldMoney = currentMoney;
        currentMoney += amount;
        if (currentMoney < 0) currentMoney = 0;

        OnMoneyChanged?.Invoke(currentMoney);
        SaveMoney();

        if (moneyDisplayCoroutine != null) StopCoroutine(moneyDisplayCoroutine);
        moneyDisplayCoroutine = StartCoroutine(ShowMoneyChange(amount));
    }

    /// <summary>
    /// 指定した金額を支払う
    /// </summary>
    /// <param name="amount">支払う金額</param>
    /// <returns>成功したらtrue</returns>
    public bool SpendMoney(int amount) {
        if (amount <= 0) return false;
        if (currentMoney < amount) return false;

        int oldMoney = currentMoney;
        currentMoney -= amount;
        if (currentMoney < 0) currentMoney = 0;

        OnMoneyChanged?.Invoke(currentMoney);
        PlayerItemManager.Instance?.UpdateMoney(currentMoney);

        if (moneyDisplayCoroutine != null) StopCoroutine(moneyDisplayCoroutine);
        moneyDisplayCoroutine = StartCoroutine(ShowMoneyChange(-amount));

        return true;
    }

    /// <summary>
    /// 所持金のリセット
    /// </summary>
    public void ResetMoney() {
        currentMoney = startMoney;
        OnMoneyChanged?.Invoke(currentMoney);
        SaveMoney();
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
        UpdateMoneyText(0);
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
}