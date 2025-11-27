using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class OptionMenu : MonoBehaviour {
    [Header("対象のPlayerCamera")]
    public PlayerCamera playerCamera;

    [Header("UI参照")]
    public Canvas optionCanvas;
    public Slider sensitivitySlider;

    [Header("Input")]
    public PlayerInput playerInput;

    /// <summary>
    /// シーン内の GachaSystem をキャッシュするためのフィールド
    /// </summary>
    private GachaSystem cachedGachaSystem;

    /// <summary>
    /// シーン内から GachaSystem を自動で探してくるゲッター
    /// 初回だけ FindObjectOfType し、その後はキャッシュを使う
    /// </summary>
    private GachaSystem Gacha {
        get {
            if (cachedGachaSystem == null) {
                cachedGachaSystem = FindObjectOfType<GachaSystem>();
            }
            return cachedGachaSystem;
        }
    }

    private InputActionRebindingExtensions.RebindingOperation currentOp;
    public bool isOpen { get; private set; } = false;

    private Button jumpButton;
    private Button fireMainButton;
    private Button fireSubButton;
    private Button skillButton;
    private Button reloadButton;

    private TextMeshProUGUI statusText;

    // ボタン選択状態管理
    private Button selectedButton;
    private Color normalColor = new Color(0.8f, 0.8f, 0.8f);
    private Color selectedColor = new Color(0.5f, 0.5f, 0.5f);

    // ボタン内テキスト表示用
    private TextMeshProUGUI jumpText;
    private TextMeshProUGUI fireMainText;
    private TextMeshProUGUI fireSubText;
    private TextMeshProUGUI skillText;
    private TextMeshProUGUI reloadText;

    void Start() {
        optionCanvas.enabled = false;

        // カメラ感度ロード
        float saved = PlayerPrefs.GetFloat("CameraSensitivity", playerCamera.rotationSpeed);
        playerCamera.rotationSpeed = saved;
        sensitivitySlider.value = saved;
        sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);

        #region キーバインド読み込み

        string[] actions = { "Jump", "Fire_Main", "SubWeapon", "Skill", "Reload" };
        foreach (var act in actions)
            LoadRebind(act);

        // Canvas内のボタン取得
        jumpButton = optionCanvas.transform.Find("Rebind Jump")?.GetComponent<Button>();
        fireMainButton = optionCanvas.transform.Find("Rebind Fire_Main")?.GetComponent<Button>();
        fireSubButton = optionCanvas.transform.Find("Rebind SubWeapon")?.GetComponent<Button>();
        skillButton = optionCanvas.transform.Find("Rebind Skill")?.GetComponent<Button>();
        reloadButton = optionCanvas.transform.Find("Rebind Reload")?.GetComponent<Button>();

        // ボタン内TextMeshProUGUI取得
        jumpText = jumpButton?.transform.Find("Text")?.GetComponent<TextMeshProUGUI>();
        fireMainText = fireMainButton?.transform.Find("Text")?.GetComponent<TextMeshProUGUI>();
        fireSubText = fireSubButton?.transform.Find("Text")?.GetComponent<TextMeshProUGUI>();
        skillText = skillButton?.transform.Find("Text")?.GetComponent<TextMeshProUGUI>();
        reloadText = reloadButton?.transform.Find("Text")?.GetComponent<TextMeshProUGUI>();

        // ステータステキスト
        statusText = optionCanvas.transform.Find("RebindStatusText")?.GetComponent<TextMeshProUGUI>();

        // ボタンにリスナー追加
        jumpButton?.onClick.AddListener(() => StartRebind("Jump", 0));
        fireMainButton?.onClick.AddListener(() => StartRebind("Fire_Main", 0));
        fireSubButton?.onClick.AddListener(() => StartRebind("SubWeapon", 0));
        skillButton?.onClick.AddListener(() => StartRebind("Skill", 0));
        reloadButton?.onClick.AddListener(() => StartRebind("Reload", 0));

        // ボタンテキスト更新
        UpdateButtonText("Jump", jumpText);
        UpdateButtonText("Fire_Main", fireMainText);
        UpdateButtonText("SubWeapon", fireSubText);
        UpdateButtonText("Skill", skillText);
        UpdateButtonText("Reload", reloadText);

        #endregion
    }

    /// <summary>
    /// オプションメニューの開閉を切り替える
    /// </summary>
    public void ToggleMenu() {


        //// これから「開こうとしている」ときだけガチャ状態をチェック
        //if (!isOpen && IsBlockedByGacha()) {
        //    Debug.Log("OptionMenu: ガチャ画面中のためオプションメニューを開きません。");
        //    return;
        //}


        isOpen = !isOpen;
        optionCanvas.enabled = isOpen;
        Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;
    }

    //===========================
    // ガチャ状態によるブロック判定
    //===========================

    /// <summary>
    /// ガチャ画面が開かれているため
    /// オプションメニューを開けない状態かどうか
    /// </summary>
    //public bool IsBlockedByGacha() {
    //    // シーンに GachaSystem が存在しないならブロックしない
    //    if (Gacha == null) return false;

    //    // GachaSystem 側のフラグをそのまま返す
    //    return Gacha.IsGachaActive();
    //}




    /// <summary>
    /// 感度スライダーの値変更時
    /// </summary>
    private void OnSensitivityChanged(float value) {
        playerCamera.rotationSpeed = value;
        PlayerPrefs.SetFloat("CameraSensitivity", value);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 指定Actionのリバインドを開始
    /// </summary>
    public void StartRebind(string actionName, int bindingIndex) {
        if (currentOp != null)
            currentOp.Cancel();

        var action = playerInput.actions[actionName];
        action.Disable();

        // 選択ボタンを暗く
        UpdateSelectedButton(actionName);

        // 現在のバインドをステータスに表示
        string currentKey = action.bindings[bindingIndex].effectivePath;
        string displayKey = InputControlPath.ToHumanReadableString(
            currentKey,
            InputControlPath.HumanReadableStringOptions.OmitDevice
        );
        statusText.text = $"{actionName} waiting for input... (Esc to cancel)";

        currentOp = action.PerformInteractiveRebinding(bindingIndex)
            .WithCancelingThrough("<Keyboard>/escape")
            .OnComplete(op => FinishRebind(actionName, bindingIndex, op))
            .OnCancel(op => CancelRebind(actionName))
            .Start();
    }

    /// <summary>
    /// リバインド完了時の処理
    /// </summary>
    private void FinishRebind(string actionName, int bindingIndex,
        InputActionRebindingExtensions.RebindingOperation op) {

        var action = playerInput.actions[actionName];
        op.Dispose();
        currentOp = null;
        action.Enable();

        string keyName = action.bindings[bindingIndex].effectivePath;
        string displayName = InputControlPath.ToHumanReadableString(
            keyName,
            InputControlPath.HumanReadableStringOptions.OmitDevice);

        statusText.text = $"{actionName} set to {displayName}";

        // ボタンテキスト更新
        switch (actionName) {
            case "Jump": UpdateButtonText("Jump", jumpText); break;
            case "Fire_Main": UpdateButtonText("Fire_Main", fireMainText); break;
            case "SubWeapon": UpdateButtonText("SubWeapon", fireSubText); break;
            case "Skill": UpdateButtonText("Skill", skillText); break;
            case "Reload": UpdateButtonText("Reload", reloadText); break;
        }

        // 選択ボタンの色を元に戻す
        if (selectedButton != null) {
            var img = selectedButton.GetComponent<Image>();
            if (img != null) img.color = normalColor;
            selectedButton = null;
        }

        SaveRebind(actionName);
    }

    /// <summary>
    /// リバインドキャンセル時の処理
    /// </summary>
    private void CancelRebind(string actionName) {
        var action = playerInput.actions[actionName];
        currentOp?.Dispose();
        currentOp = null;
        action.Enable();

        statusText.text = "Rebind canceled";

        if (selectedButton != null) {
            var img = selectedButton.GetComponent<Image>();
            if (img != null) img.color = normalColor;
            selectedButton = null;
        }
    }

    /// <summary>
    /// 選択中のボタンを暗くする
    /// </summary>
    private void UpdateSelectedButton(string actionName) {
        if (selectedButton != null) {
            var img = selectedButton.GetComponent<Image>();
            if (img != null) img.color = normalColor;
        }

        switch (actionName) {
            case "Jump": selectedButton = jumpButton; break;
            case "Fire_Main": selectedButton = fireMainButton; break;
            case "SubWeapon": selectedButton = fireSubButton; break;
            case "Skill": selectedButton = skillButton; break;
            case "Reload": selectedButton = reloadButton; break;
        }

        if (selectedButton != null) {
            var img = selectedButton.GetComponent<Image>();
            if (img != null) img.color = selectedColor;
        }
    }

    /// <summary>
    /// ボタン内テキストに現在のバインドを表示
    /// </summary>
    private void UpdateButtonText(string actionName, TextMeshProUGUI tmp) {
        if (tmp == null) return;

        var action = playerInput.actions[actionName];
        if (action.bindings.Count == 0) return;

        string keyName = action.bindings[0].effectivePath;
        string displayName = InputControlPath.ToHumanReadableString(
            keyName,
            InputControlPath.HumanReadableStringOptions.OmitDevice
        );

        tmp.text = $"{actionName}: {displayName}";
    }

    /// <summary>
    /// 指定Actionのバインディングを保存
    /// </summary>
    private void SaveRebind(string actionName) {
        string json = playerInput.actions[actionName].SaveBindingOverridesAsJson();
        PlayerPrefs.SetString($"{actionName}_rebind", json);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 指定Actionのバインディングをロード
    /// </summary>
    private void LoadRebind(string actionName) {
        string saved = PlayerPrefs.GetString($"{actionName}_rebind", "");
        if (!string.IsNullOrEmpty(saved))
            playerInput.actions[actionName].LoadBindingOverridesFromJson(saved);
    }
}