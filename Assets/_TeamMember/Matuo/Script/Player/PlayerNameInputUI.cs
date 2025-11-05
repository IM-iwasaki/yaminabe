using UnityEngine;
using UnityEngine.UI;
using TMPro; // ← これが超重要！

public class PlayerNameInputUI : MonoBehaviour {
    private Canvas canvas;
    private TMP_InputField inputField;
    private Button confirmButton;
    private TextMeshProUGUI label;

    void Start() {
        // すでにプレイヤー名が保存されていればスキップ
        PlayerData data = PlayerSaveData.Load();
        if (!string.IsNullOrEmpty(data.playerName) && data.playerName != "Default") {
            Debug.Log($"既存プレイヤー名を使用します: {data.playerName}");
            return;
        }

        CreateUI();
    }

    private void CreateUI() {
        // === Canvas ===
        GameObject canvasObj = new GameObject("NameInputCanvas");
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObj.AddComponent<GraphicRaycaster>();

        // === 背景パネル ===
        GameObject panelObj = new GameObject("Panel");
        panelObj.transform.SetParent(canvasObj.transform);
        var panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.6f);
        var panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = panelRect.offsetMax = Vector2.zero;

        // === テキストラベル ===
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(panelObj.transform);
        label = labelObj.AddComponent<TextMeshProUGUI>();
        label.text = "Default";
        label.fontSize = 36;
        label.alignment = TextAlignmentOptions.Center;
        var labelRect = label.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0.5f, 0.7f);
        labelRect.anchorMax = new Vector2(0.5f, 0.7f);
        labelRect.pivot = new Vector2(0.5f, 0.5f);
        labelRect.anchoredPosition = Vector2.zero;
        labelRect.sizeDelta = new Vector2(600, 100);

        // === 入力フィールド ===
        GameObject inputObj = new GameObject("NameInput");
        inputObj.transform.SetParent(panelObj.transform);
        inputField = inputObj.AddComponent<TMP_InputField>();
        var inputRect = inputObj.GetComponent<RectTransform>();
        inputRect.anchorMin = new Vector2(0.5f, 0.5f);
        inputRect.anchorMax = new Vector2(0.5f, 0.5f);
        inputRect.pivot = new Vector2(0.5f, 0.5f);
        inputRect.anchoredPosition = Vector2.zero;
        inputRect.sizeDelta = new Vector2(400, 60);

        // 背景
        var bg = inputObj.AddComponent<Image>();
        bg.color = Color.white;
        inputField.targetGraphic = bg;

        // 入力テキスト
        var textObj = new GameObject("Text");
        textObj.transform.SetParent(inputObj.transform);
        var textComponent = textObj.AddComponent<TextMeshProUGUI>();
        textComponent.fontSize = 28;
        textComponent.color = Color.black;
        textComponent.alignment = TextAlignmentOptions.Center;
        var textRect = textComponent.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 0);
        textRect.anchorMax = new Vector2(1, 1);
        textRect.offsetMin = textRect.offsetMax = Vector2.zero;
        inputField.textComponent = textComponent;

        // プレースホルダー
        var placeholderObj = new GameObject("Placeholder");
        placeholderObj.transform.SetParent(inputObj.transform);
        var placeholder = placeholderObj.AddComponent<TextMeshProUGUI>();
        placeholder.text = "例: Yamada";
        placeholder.fontSize = 28;
        placeholder.color = new Color(0.5f, 0.5f, 0.5f);
        placeholder.alignment = TextAlignmentOptions.Center;
        var placeholderRect = placeholder.GetComponent<RectTransform>();
        placeholderRect.anchorMin = new Vector2(0, 0);
        placeholderRect.anchorMax = new Vector2(1, 1);
        placeholderRect.offsetMin = placeholderRect.offsetMax = Vector2.zero;
        inputField.placeholder = placeholder;

        // === 決定ボタン ===
        GameObject buttonObj = new GameObject("ConfirmButton");
        buttonObj.transform.SetParent(panelObj.transform);
        confirmButton = buttonObj.AddComponent<Button>();
        var btnImage = buttonObj.AddComponent<Image>();
        btnImage.color = new Color(0.9f, 0.9f, 0.9f);
        var btnRect = buttonObj.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0.35f);
        btnRect.anchorMax = new Vector2(0.5f, 0.35f);
        btnRect.pivot = new Vector2(0.5f, 0.5f);
        btnRect.anchoredPosition = Vector2.zero;
        btnRect.sizeDelta = new Vector2(200, 60);
        confirmButton.targetGraphic = btnImage;

        // ボタンのテキスト
        var btnTextObj = new GameObject("Text");
        btnTextObj.transform.SetParent(buttonObj.transform);
        var btnText = btnTextObj.AddComponent<TextMeshProUGUI>();
        btnText.text = "決定";
        btnText.fontSize = 28;
        btnText.color = Color.black;
        btnText.alignment = TextAlignmentOptions.Center;
        var btnTextRect = btnText.GetComponent<RectTransform>();
        btnTextRect.anchorMin = Vector2.zero;
        btnTextRect.anchorMax = Vector2.one;
        btnTextRect.offsetMin = btnTextRect.offsetMax = Vector2.zero;

        confirmButton.onClick.AddListener(OnConfirmClicked);
    }

    private void OnConfirmClicked() {
        string newName = inputField.text.Trim();

        if (string.IsNullOrEmpty(newName)) {
            label.text = "Default";
            label.color = Color.red;
            return;
        }

        // データ保存
        PlayerData data = PlayerSaveData.Load();
        data.playerName = newName;
        PlayerSaveData.Save(data);

        Debug.Log($"プレイヤー名を保存しました: {newName}");

        // シーン上のキャラに反映
        CharacterBase character = FindObjectOfType<CharacterBase>();
        if (character != null) {
            character.PlayerName = newName;
        }

        // UIを削除
        Destroy(canvas.gameObject);
    }
}