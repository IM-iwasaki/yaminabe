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

    private InputActionRebindingExtensions.RebindingOperation currentOp;

    public bool isOpen { get; private set; } = false;

    private Button jumpButton;
    private Button fireMainButton;
    private Button fireSubButton;
    private Button skillButton;
    private Button reloadButton;

    private TextMeshProUGUI statusText;

    void Start() {
        optionCanvas.enabled = false;

        // カメラ感度ロード
        float saved = PlayerPrefs.GetFloat("CameraSensitivity", playerCamera.rotationSpeed);
        playerCamera.rotationSpeed = saved;
        sensitivitySlider.value = saved;
        sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
        #region　キーバインド読み込み
        // 保存済みバインドを読み込む
        string[] actions = { "Jump", "Fire_Main", "Fire_Sub", "Skill", "Reload" };
        foreach (var act in actions)
            LoadRebind(act);

        // Canvas内のボタンを自動取得
        jumpButton = optionCanvas.transform.Find("Rebind Jump")?.GetComponent<Button>();
        fireMainButton = optionCanvas.transform.Find("Rebind Fire_Main")?.GetComponent<Button>();
        fireSubButton = optionCanvas.transform.Find("Rebind Fire_Sub")?.GetComponent<Button>();
        skillButton = optionCanvas.transform.Find("Rebind Skill")?.GetComponent<Button>();
        reloadButton = optionCanvas.transform.Find("Rebind Reload")?.GetComponent<Button>();

        // ステータステキスト
        statusText = optionCanvas.transform.Find("RebindStatusText")?.GetComponent<TextMeshProUGUI>();

        // ボタンにリスナーを追加
        jumpButton?.onClick.AddListener(() => StartRebind("Jump", 0));
        fireMainButton?.onClick.AddListener(() => StartRebind("Fire_Main", 0));
        fireSubButton?.onClick.AddListener(() => StartRebind("Fire_Sub", 0));
        skillButton?.onClick.AddListener(() => StartRebind("Skill", 0));
        reloadButton?.onClick.AddListener(() => StartRebind("Reload", 0));
        #endregion 
    }

    /// <summary>
    /// オプションメニューの開閉を切り替える
    /// </summary>
    public void ToggleMenu() {
        isOpen = !isOpen;
        optionCanvas.enabled = isOpen;
        Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;
    }

    /// <summary>
    /// 感度スライダーの値が変更された時の処理
    /// </summary>
    /// <param name="value">スライダーの新しい値</param>
    private void OnSensitivityChanged(float value) {
        playerCamera.rotationSpeed = value;
        PlayerPrefs.SetFloat("CameraSensitivity", value);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 指定Actionのリバインドを開始する
    /// </summary>
    /// <param name="actionName">リバインド対象のAction名</param>
    /// <param name="bindingIndex">バインディングインデックス</param>
    public void StartRebind(string actionName, int bindingIndex) {
        if (currentOp != null)
            currentOp.Cancel();

        var action = playerInput.actions[actionName];

        action.Disable();

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

        // ここで実際のキー名を取得
        string keyName = action.bindings[bindingIndex].effectivePath;
        string displayName = InputControlPath.ToHumanReadableString(
            keyName,
            InputControlPath.HumanReadableStringOptions.OmitDevice);

        statusText.text = $"{actionName} set to {displayName}";

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
    }

    /// <summary>
    /// 指定Actionのバインディングを保存する
    /// </summary>
    private void SaveRebind(string actionName) {
        string json = playerInput.actions[actionName].SaveBindingOverridesAsJson();
        PlayerPrefs.SetString($"{actionName}_rebind", json);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 指定Actionのバインディングをロードする
    /// </summary>
    private void LoadRebind(string actionName) {
        string saved = PlayerPrefs.GetString($"{actionName}_rebind", "");
        if (!string.IsNullOrEmpty(saved))
            playerInput.actions[actionName].LoadBindingOverridesFromJson(saved);
    }
}