using Mirror;
using UnityEngine;
using System.Collections;

public struct CountdownMessage : NetworkMessage {
    public int seconds;
}

public class CountdownManager : NetworkSystemObject<CountdownManager> {
    [Server]
    public void SendCountdown(int seconds) {
        NetworkServer.SendToAll(new CountdownMessage { seconds = seconds });
        Debug.Log("[Server] Countdown sent: " + seconds);
    }
}
