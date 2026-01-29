using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerListUIManager : MonoBehaviour
{
    public static PlayerListUIManager Instance;

    [Header("生成させるプレイヤーリストプレハブ")]
    [SerializeField] private GameObject playerListUI;

    private void Awake() {
        Instance = this;
    }

    /// <summary>
    /// プレイヤーリストの更新
    /// </summary>
    /// <param name="server"></param>
    public void UpdatePlayerList(ServerManager server) {
        //  一度プレイヤーリストを初期化
        ResetPlayerList();
        //  プレイヤー1人1人のプレハブを作成
        foreach (var conn in server.connectPlayer) {
            //  キャラクターパラメータの情報をキャッシュ
            CharacterParameter player = conn.GetComponent<CharacterParameter>();
            //  子オブジェクトとして生成
            Instantiate(playerListUI, transform);
            //  プレイヤーの名前をセット
            playerListUI.GetComponent<TextMeshPro>().SetText(player.PlayerName);
            //  チェックボックス判定
            CanReadyPlayerUI(player);
        }

    }

    /// <summary>
    /// プレイヤーが準備完了か否かをUIで見せる
    /// </summary>
    /// <param name="player"></param>
    private void CanReadyPlayerUI(CharacterParameter player) {
        Transform checkBox = playerListUI.transform.GetChild(0);
        checkBox.gameObject.SetActive(player.ready);
    }

    /// <summary>
    /// プレイヤーリストのリセット
    /// </summary>
    private void ResetPlayerList() {
        DestroyAllChildren(transform);
    }

    /// <summary>
    /// 指定の親オブジェクトの子オブジェクトを全部削除する
    /// </summary>
    /// <param name="parent"></param>
    private void DestroyAllChildren(Transform parent) {
        foreach (Transform child in parent) {
            Destroy(child.gameObject);
        }
    }

}
