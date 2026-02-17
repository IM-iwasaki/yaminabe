using Mirror;
using UnityEngine;

public class ShowResultPanelEvent : PVEStageEvent {

    [Header("表示するリザルトパネル")]
    [SerializeField] private GameObject resultPanel;

    private bool isResultActive = true;                 // 二重押し防止
    private ResultManager resultManager;

    protected override void Execute() {
        if (resultPanel == null) return;
        bool isHost = NetworkServer.active;
        if (NetworkServer.active) {
            GameManager.Instance.EndGame();
        }
        // ホストがオプションを開いていたら閉じる
        if (isHost) {
            OptionMenu optionMenu = FindObjectOfType<OptionMenu>();
            if (optionMenu != null && optionMenu.isOpen) {
                optionMenu.ToggleMenu(); // 閉じる
            }
        }

        resultPanel.SetActive(true);

        if (NetworkServer.active && NetworkClient.isConnected) {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}