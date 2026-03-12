using UnityEngine;
using System.Net.Sockets;
using System.Text;
using System.Collections.Concurrent;
using System.Collections;
using System.Net;
using System.Collections.Generic;
using System;
using System.Linq;
using kcp2k;
/// <summary>
/// IPアドレスを定期的に受信する
/// </summary>
public class UDPListener : MonoBehaviour {
    /// <summary>
    /// 取り出せた時にだけ処理できる安全なキュー
    /// </summary>
    ConcurrentQueue<UdpMessage> messageQueue = new ConcurrentQueue<UdpMessage>();

    public List<UdpMessage> discoveredHosts = new List<UdpMessage>();

    public event Action<UdpMessage> onHostUpdated;

    private Coroutine receiveCoroutine;
    private UdpClient udpClient;
    private Socket socket;
    /// <summary>
    /// 受信するメッセージ
    /// </summary>
    [System.Serializable]
    public struct UdpMessage {
        public string ip;
        public int port;
        public string gameName;
        public string hostName;
        public bool gamePlaying;
    }
    /// <summary>
    /// タイトルシーンでIPアドレスが取得できたかどうかを判定する用変数
    /// </summary>
    public bool isGetIP = false;

    private void Awake() {
        isGetIP = false;
    }

    // Update is called once per frame
    void Update() {
        if (!TitleManager.instance) return;

        if (messageQueue.TryDequeue(out UdpMessage msg)) {
            //同一IPは一つのみ登録
            if (!discoveredHosts.Contains(msg)) {
                discoveredHosts.Add(msg);
                onHostUpdated?.Invoke(msg);
            }

            isGetIP = true;
        }
    }

    /// <summary>
    /// IPアドレスの受信を開始する
    /// </summary>
    public void StartReceiveIP() {
        //一度ホスト一覧をリセットして探す
        discoveredHosts.Clear();
        StartCoroutine(ReceiveMessageFromBroadcaster());
    }

    /// <summary>
    /// IPアドレスの定期受信
    /// </summary>
    /// <returns></returns>
    public IEnumerator ReceiveMessageFromBroadcaster() {
        IPEndPoint localEP = new IPEndPoint(IPAddress.Any, 55555);
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        socket.Bind(localEP);

        udpClient = new UdpClient();
        udpClient.Client = socket;
        while (true) {
            if (udpClient.Available > 0) {
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                byte[] result = udpClient.Receive(ref remoteEP);
                string json = Encoding.UTF8.GetString(result);
                UdpMessage message = JsonUtility.FromJson<UdpMessage>(json);
                //キューに追加
                messageQueue.Enqueue(message);
            }
            yield return null;
        }

    }

    /// <summary>
    /// IPアドレス受信終了
    /// </summary>
    public void StopReceiveIP() {
        if (receiveCoroutine != null) {
            StopCoroutine(receiveCoroutine);
            receiveCoroutine = null;
        }

        udpClient?.Close();
        udpClient?.Dispose();
        udpClient = null;

        socket?.Close();
        socket?.Dispose();
        socket = null;

    }
}
