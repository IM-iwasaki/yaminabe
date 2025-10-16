using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class TitleManager : MonoBehaviour {
    public static TitleManager instance = null;
    public string ipAddress = null;
    public bool isHost = false;

    public TMP_InputField inputField = null;
    public TextMeshProUGUI stringIPAddress = null;

    private void Awake() {
        DontDestroyOnLoad(gameObject);

        instance = this;
    }

    public void OnStartHostButton() {
        //明示的にホスト状態をtrueにし、ロビーシーンに移行
        isHost = true;

        SceneManager.LoadScene("LobbyScene");

    }

    public void OnStartClientButton() {
        //IPアドレス未設定を防ぐために早期リターン
        if (ipAddress == null)
            return;
        //明示的にホスト状態をfalseにし、ロビーシーンに移行
        isHost = false;
        SceneManager.LoadScene("LobbyScene");

    }

    public void SetIPAddress() {
        ipAddress = inputField.text;
    }

    private void Update() {
        if (ipAddress == null)
            stringIPAddress.text = "404NotFound";
        else
            stringIPAddress.text = ipAddress;
    }
}
