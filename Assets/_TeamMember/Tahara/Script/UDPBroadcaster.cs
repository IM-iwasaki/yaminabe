using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;

/// <summary>
/// 定期的にIPアドレスを送信する
/// </summary>
public class UDPBroadcaster : MonoBehaviour
{
    /// <summary>
    /// UDP形式で送信するメッセージの構造体
    /// </summary>
    [System.Serializable]
    public class UdpMessage {
        public string ip;
        public int port;
        public string gameName;
        public string hostName;
    }

    /// <summary>
    /// メッセージの実体
    /// </summary>
    public UdpMessage message = new UdpMessage();
    /// <summary>
    /// 送るIPアドレスの文字列
    /// </summary>
    public string sendIPAddress = null;
    /// <summary>
    /// メッセージをjsonファイルに変更した時に保存する変数
    /// </summary>
    private string json = null;
    private void Awake() {
        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        //送信するメッセージを初期化
        MessageInitialized();   
    }
    /// <summary>
    /// IPアドレスを送信
    /// </summary>
    public void StartSendIP() {
        //定期的に送信
        InvokeRepeating(nameof(SendMesseageToClient), 0.0f, 0.1f);
    }

    /// <summary>
    /// メッセージの初期化
    /// </summary>
    private void MessageInitialized() {
        message.ip = GetIpAddress();
        message.port = 9876;
        message.gameName = "TPS";
        message.hostName = System.Environment.MachineName;
        Debug.Log(message.ip);
    }

    /// <summary>
    /// IPアドレスを取得
    /// </summary>
    /// <returns></returns>
    private string GetIpAddress() {
        string hostName = Dns.GetHostName();
        IPAddress[] ips = Dns.GetHostAddresses(hostName);

        foreach (var sendIP in ips) {
            if (sendIP.AddressFamily.Equals(AddressFamily.InterNetwork)) {
                return sendIP.ToString();
            }
        }
        return null;
    }

    /// <summary>
    /// 定期的にクライアントにメッセージを送る
    /// </summary>
    public void SendMesseageToClient() {
        UdpClient client = new UdpClient();
        client.EnableBroadcast = true;

        IPEndPoint endPoint = new IPEndPoint(IPAddress.Broadcast,message.port);
        //jsonファイルに変更
        json = JsonUtility.ToJson(message);
        byte[] data = Encoding.UTF8.GetBytes(json);

        client.Send(data, data.Length, endPoint);
        client.Close();

    }
}
