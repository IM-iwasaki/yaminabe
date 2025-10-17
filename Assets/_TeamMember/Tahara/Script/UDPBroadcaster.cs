using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;

/// <summary>
/// 定期的にIPアドレスを送信する
/// </summary>
public class UDPBroadcaster : MonoBehaviour
{
    [System.Serializable]
    public class UdpMessage {
        public string ip;
        public int port;
        public string gameName;
        public string hostName;
    }

    public UdpMessage message = new UdpMessage();
    public string sendIPAddress = null;
    public string json = null;
    // Start is called before the first frame update
    void Start()
    {
        //送信するメッセージを初期化
        MessageInitialized();
        //定期的に送信
        InvokeRepeating("SendMesseageToClient", 0.0f, 1.0f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void MessageInitialized() {
        message.ip = GetIpAddress();
        message.port = 9876;
        message.gameName = "TPS";
        message.hostName = System.Environment.MachineName;
        Debug.Log(message.ip);
    }

    public string GetIpAddress() {
        string hostName = Dns.GetHostName();
        IPAddress[] ips = Dns.GetHostAddresses(hostName);

        foreach (var sendIP in ips) {
            if (sendIP.AddressFamily.Equals(AddressFamily.InterNetwork)) {
                
                return sendIP.ToString();
                
            }
        }
        Debug.Log("nullやでー");
        return null;
    }

    //定期的にclientにメッセージを送る
    public void SendMesseageToClient() {
        UdpClient client = new UdpClient();
        client.EnableBroadcast = true;

        IPEndPoint endPoint = new IPEndPoint(IPAddress.Broadcast,message.port);
        //jsonファイルに変更
        json = JsonUtility.ToJson(message);
        Debug.Log("Send json:" + json);
        byte[] data = Encoding.UTF8.GetBytes(json);

        client.Send(data, data.Length, endPoint);
        client.Close();

    }
}
