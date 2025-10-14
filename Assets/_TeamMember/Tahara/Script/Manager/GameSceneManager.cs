using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager :NetworkSystemObject<GameSceneManager>
{
    [SerializeField]
    private string gameSceneName = "GameScene";

    public void LoadGameScene() {
        if (!isServer) return;
        SceneManager.LoadSceneAsync(gameSceneName, LoadSceneMode.Additive);
    }

    [ClientRpc]
    public void LoadGameSceneForAll() {
        GameSceneManager.Instance.LoadGameScene();
    }


}
