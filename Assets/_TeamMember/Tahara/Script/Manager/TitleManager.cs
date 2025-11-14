using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

/// <summary>
/// タイトル管理クラス
/// ホストかクライアントかで処理が変わる
/// </summary>
public class TitleManager : MonoBehaviour {
    /// <summary>
    /// インスタンス
    /// </summary>
    public static TitleManager instance = null;
    /// <summary>
    /// 参加するサーバーのIPアドレス(IPv4)
    /// </summary>
    public string ipAddress = null;
    /// <summary>
    /// ホストかどうか
    /// </summary>
    public bool isHost { get; private set; } = false;
    /// <summary>
    /// クライアントかどうか
    /// </summary>
    public bool isClient { get; private set; } = false;
    /// <summary>
    /// 今はタイトル画面なのか
    /// </summary>
    public bool isTitle = true;
    /// <summary>
    /// IPアドレス入力用(現在は自動取得可能なので使わないかも)
    /// </summary>
    public TMP_InputField inputField = null;
    /// <summary>
    /// IPアドレスを探している状況を教えるUI
    /// </summary>
    public TextMeshProUGUI SearchOrMissingText = null;
    /// <summary>
    /// ボタン押下判定用変数
    /// </summary>
    static private bool once = false;
    /// <summary>
    /// ロードするロビーシーンの名前
    /// </summary>
    [SerializeField]
    private string lobbySceneName = null;
    /// <summary>
    /// IPアドレスを送信するクラス(使い分けするためにメンバで管理)
    /// </summary>
    [SerializeField]
    private UDPBroadcaster sender = null;
    /// <summary>
    /// IPアドレスを受信するクラス(使い分けするためにメンバで管理)
    /// </summary>
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
            TitleAudio.Instance.PlaySE("決定");
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
            TitleAudio.Instance.PlaySE("決定");
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
