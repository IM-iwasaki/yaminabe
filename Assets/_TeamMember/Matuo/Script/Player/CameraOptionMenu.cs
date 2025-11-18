using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class CameraOptionMenu : MonoBehaviour {
    [Header("対象のPlayerCamera")]
    public PlayerCamera playerCamera;

    [Header("UI参照")]
    public Canvas optionCanvas;
    public Slider sensitivitySlider;

    public bool isOpen { get; private set; } = false;

    public void Start() {
        // ローカルプレイヤー以外の UI は無効化しておく
        optionCanvas.enabled = false;

        // PlayerPrefs から感度を読み込み
        float saved = PlayerPrefs.GetFloat("CameraSensitivity", playerCamera.rotationSpeed);
        playerCamera.rotationSpeed = saved;
        sensitivitySlider.value = saved;

        // UIを初期的に非表示にする
        optionCanvas.enabled = false;

        // スライダー変更イベント登録
        sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
    }
    /// <summary>
    /// オプションメニュー開閉切り替え
    /// </summary>
    public void ToggleMenu() {
        isOpen = !isOpen;
        optionCanvas.enabled = isOpen;

        Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;
    }

    /// <summary>
    /// 感度スライダーの値変更時
    /// </summary>
    private void OnSensitivityChanged(float value) {
        if (playerCamera != null)
            playerCamera.rotationSpeed = value;

        PlayerPrefs.SetFloat("CameraSensitivity", value);
        PlayerPrefs.Save();
    }
}