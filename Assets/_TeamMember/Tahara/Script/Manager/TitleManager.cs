using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class TitleManager : MonoBehaviour {
    public static TitleManager instance = null;
    public string ipAddress = null;
    public bool isHost { get; private set; } = false;
    public bool isClient { get; private set; } = false;
    public bool isTitle = true;

    [SerializeField]
    private string lobbySceneName = null;
    [SerializeField]
    private UDPBroadcaster sender = null;
    [SerializeField]
    private UDPListener receiver = null;
    public TMP_InputField inputField = null;
    public TextMeshProUGUI stringIPAddress = null;

    private void Awake() {
        DontDestroyOnLoad(gameObject);

        instance = this;
    }

    public void OnStartHostButton() {
        //明示的にホスト状態をtrueにし、ロビーシーンに移行
        isHost = true;
        sender.StartSendIPAddres();
        SceneManager.LoadScene(lobbySceneName);
        isTitle = false;
    }

    public void OnStartClientButton() {
        //IPアドレス未設定を防ぐために早期リターン
        if (ipAddress == null)
            return;

        //明示的にクライアント状態をtrueにし、IPアドレスが取得できたらロビーシーンに移行
        StartCoroutine(WaitReceivedIP());
        
    }

    private IEnumerator WaitReceivedIP() {
        receiver.StartReceiveIP();

        float timeout = 10.0f;
        float timer = 0.0f;

        while (!receiver.isGetIP && timer < timeout) {
            timer += Time.deltaTime;
            yield return null;
        }
        //取得できた
        if (receiver.isGetIP) {
            isClient = true;
            SceneManager.LoadScene(lobbySceneName);
            isTitle = false;
        }
        else {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }


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
