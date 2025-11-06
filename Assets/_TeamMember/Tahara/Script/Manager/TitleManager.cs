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
    static private bool once = false;

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
        if (!once) {
            //明示的にホスト状態をtrueにし、ロビーシーンに移行
            isHost = true;
            sender.StartSendIP();
            SceneManager.LoadScene(lobbySceneName);
            isTitle = false;
            once = true;
        }

    }

    public void OnStartClientButton() {
        if (!once) {
            //IPアドレス未設定を防ぐために早期リターン
            if (ipAddress == null)
                return;
            if (!once) {
                //IPアドレスが取得できたらロビーシーンに移行
                StartCoroutine(WaitReceivedIP());
            }
            once = true;
        }
    }

    private IEnumerator WaitReceivedIP() {
        receiver.StartReceiveIP();

        float timeout = 10.0f;
        float timer = 0.0f;

        while (!receiver.isGetIP && timer < timeout) {
            timer += Time.deltaTime;
            stringIPAddress.text = "Now Searching...";
            yield return null;
        }
        //取得できた
        if (receiver.isGetIP) {
            isClient = true;
            SceneManager.LoadScene(lobbySceneName);
            isTitle = false;
        }
        //取得できなかったのでシーンを再ロード
        else {

            stringIPAddress.text = "Not Found";
            yield return new WaitForSeconds(1.0f);
            if (once)
                once = false;
        }


    }
    public void SetIPAddress() {
        ipAddress = inputField.text;
    }
}
