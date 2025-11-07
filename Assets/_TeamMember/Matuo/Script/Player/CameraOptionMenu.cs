using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Mirror;

public class CameraOptionMenu : NetworkBehaviour {
    [Header("対象のPlayerCamera")]
    public PlayerCamera playerCamera;

    [Header("UI参照")]
    public Canvas optionCanvas;
    public Slider sensitivitySlider;

    private bool isOpen = false;

    public override void OnStartLocalPlayer() {
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

    private void Update() {
        // ローカルプレイヤーだけ入力を受け付ける
        if (!isLocalPlayer) return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame) {
            ToggleMenu();
        }
    }

    /// <summary>
    /// オプションメニュー開閉切り替え
    /// </summary>
    private void ToggleMenu() {
        isOpen = !isOpen;
        optionCanvas.enabled = isOpen;

        Cursor.visible = isOpen;
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