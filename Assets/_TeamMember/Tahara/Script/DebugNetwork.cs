using Mirror;
using UnityEngine;

public class DebugNetwork : NetworkBehaviour {

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    private void Update() {
        if (Input.GetKeyDown(KeyCode.Return))
            LoadScene();
    }

    //[Command]
    public void LoadScene() {
        if (!isServer) return;

        GameSceneManager.instance.LoadGameSceneForAll();
    }
}
