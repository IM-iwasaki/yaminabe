using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerListUIManager : MonoBehaviour
{
    public static PlayerListUIManager Instance;

    /// <summary>
    /// シーンにあるサーバーマネージャー
    /// </summary>
    [SerializeField]
    private ServerManager server = null;

    [Header("生成させるプレイヤーリストプレハブ")]
    [SerializeField] private GameObject playerListUI;

    [Header("親ルート取得")]
    [SerializeField] private GameObject playerListRoot;

    private void Awake() {
        Instance = this;
        playerListRoot.SetActive(false);
    }

    //  ホストだけ表示するUI
    public void ShowUI() {
        playerListRoot.SetActive(true);
    }

    /// <summary>
    /// プレイヤーリストの更新
    /// </summary>
    /// <param name="server"></param>
    public void UpdatePlayerList() {
        //  一度プレイヤーリストを初期化
        ResetPlayerList();
        //  プレイヤー1人1人のプレハブを作成
        foreach (var conn in server.connectPlayer) {
            //  キャラクターパラメータの情報をキャッシュ
            CharacterParameter player = conn.GetComponent<CharacterParameter>();
            //  子オブジェクトとして生成
            GameObject nameText = Instantiate(playerListUI, playerListRoot.transform);
            //  プレイヤーの名前をセット
            nameText.GetComponent<TextMeshProUGUI>().SetText(player.PlayerName);
            //  チェックボックス判定
            CanReadyPlayerUI(player, nameText);
        }

    }

    /// <summary>
    /// プレイヤーが準備完了か否かをUIで見せる
    /// </summary>
    /// <param name="player"></param>
    public void CanReadyPlayerUI(CharacterParameter player, GameObject nameText) {
        Transform checkBox = nameText.transform.GetChild(0);
        checkBox.gameObject.SetActive(player.ready);
    }

    /// <summary>
    /// プレイヤーリストのリセット
    /// </summary>
    private void ResetPlayerList() {
        DestroyAllChildren(playerListRoot.transform);
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
