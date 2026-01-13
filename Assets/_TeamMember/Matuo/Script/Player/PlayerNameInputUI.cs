using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerNameInputUI : MonoBehaviour {
    [Header("UI参照")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button changeButton;
    [SerializeField] private TextMeshProUGUI currentNameText;
    [SerializeField] private GameObject inputPanel;

    private void Start() {
        // UIイベント登録
        if (changeButton != null) changeButton.onClick.AddListener(() => inputPanel.SetActive(true));
        if (confirmButton != null) confirmButton.onClick.AddListener(OnConfirmClicked);

        // 現在の名前を表示
        LoadAndShowCurrentName();

        // 最初は入力パネルを閉じておく
        if (inputPanel != null) inputPanel.SetActive(false);
    }

    private void LoadAndShowCurrentName() {
        PlayerData data = PlayerSaveData.Load();
        string name = string.IsNullOrEmpty(data.playerName) ? "未設定" : data.playerName;
        if (currentNameText != null)
            currentNameText.text = $"Name: {name}";
    }

    private void OnConfirmClicked() {
        string newName = inputField.text.Trim();
        if (string.IsNullOrEmpty(newName)) return;

        // データ保存
        PlayerData data = PlayerSaveData.Load();
        data.playerName = newName;
        PlayerSaveData.Save(data);

        // キャラクターへ反映
        CharacterBase character = FindObjectOfType<CharacterBase>();
        if (character != null) {
            // 同期対応しているならCommand呼び出し
            if (character.isLocalPlayer)
                character.CmdSetPlayerName(newName);
            else
                character.parameter.PlayerName = newName;
        }

        // UI更新
        currentNameText.text = $"Name: {newName}";
        inputField.text = "";
        inputPanel.SetActive(false);
    }
}