using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

/// <summary>
/// タイトル管理クラス
/// ホストかクライアントかで処理が変わる
/// </summary>
public class TitleManager : MonoBehaviour {
    //インスタンス
    public static TitleManager instance = null;
    //参加するサーバーのIPアドレス(IPv4)
    public string ipAddress = null;
    //ホストかどうか
    public bool isHost { get; private set; } = false;
    //クライアントかどうか
    public bool isClient { get; private set; } = false;
    //今はタイトル画面なのか
    public bool isTitle = true;
    //IPアドレス入力用(現在は自動取得可能なので使わないかも)
    public TMP_InputField inputField = null;
    //IPアドレスを探している状況を教えるUI
    public TextMeshProUGUI SearchOrMissingText = null;
    //ボタン押下判定用変数
    static private bool once = false;
    //ロードするロビーシーンの名前
    [SerializeField]
    private string lobbySceneName = null;
    //IPアドレスを送信するクラス(使い分けするためにメンバで管理)
    [SerializeField]
    private UDPBroadcaster sender = null;
    //IPアドレスを受信するクラス(使い分けするためにメンバで管理)
    [SerializeField]
    private UDPListener receiver = null;
    

    private void Awake() {
        DontDestroyOnLoad(gameObject);

        instance = this;
    }

    /// <summary>
    /// ホストになるボタンを押下した時の処理
    /// </summary>
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

    /// <summary>
    /// クライアントになるボタンを押下した時の処理
    /// </summary>
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

    /// <summary>
    /// クライアント用IPアドレス検索関数
    /// </summary>
    /// <returns></returns>
    private IEnumerator WaitReceivedIP() {
        receiver.StartReceiveIP();

        //タイムアウトまでのカウントとタイマー
        float timeout = 10.0f;
        float timer = 0.0f;

        //取得できたかタイムアウトするまで待機
        while (!receiver.isGetIP && timer < timeout) {
            timer += Time.deltaTime;
            SearchOrMissingText.text = "Now Searching...";
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

            SearchOrMissingText.text = "Not Found";
            yield return new WaitForSeconds(1.0f);
            if (once)
                once = false;
        }


    }
    //InputField用関数
    public void SetIPAddress() {
        ipAddress = inputField.text;
    }
}
