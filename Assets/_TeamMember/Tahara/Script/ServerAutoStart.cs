using UnityEngine;

public class ServerAutoStart : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        CustomNetworkManager.singleton.StartServer();
    }
}
