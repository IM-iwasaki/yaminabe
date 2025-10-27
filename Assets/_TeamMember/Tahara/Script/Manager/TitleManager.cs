using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class TitleManager : MonoBehaviour {
    public static TitleManager instance = null;
    public string ipAddress = null;
    public bool isHost = false;
    public bool isClient = false;
    public bool isTitle = true;

    [SerializeField]
    private string lobbySceneName = null;

    public TMP_InputField inputField = null;
    public TextMeshProUGUI stringIPAddress = null;

    private void Awake() {
        DontDestroyOnLoad(gameObject);

        instance = this;
    }

    public void OnStartHostButton() {
        //明示的にホスト状態をtrueにし、ロビーシーンに移行
        isHost = true;

        SceneManager.LoadScene(lobbySceneName);
        isTitle = false;
    }

    public void OnStartClientButton() {
        //IPアドレス未設定を防ぐために早期リターン
        if (ipAddress == null)
            return;
        //明示的にクライアント状態をtrueにし、ロビーシーンに移行
        isClient = true;
        SceneManager.LoadScene(lobbySceneName);
        isTitle = false;
    }

    public void SetIPAddress() {
            ipAddress = inputField.text;
    }

    private void Update() {
        if (isTitle == false) return;
        if (ipAddress == null)
            stringIPAddress.text = "404NotFound";
        else
            stringIPAddress.text = ipAddress;
    }
}
