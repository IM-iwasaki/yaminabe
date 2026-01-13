using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

/// <summary>
/// キャラクター選択画面のオブジェクトを変更させる
/// </summary>
public class SelectObjectManager : NetworkBehaviour {
    //  値を変更させる定数
    private readonly int SUB_ONE_COUNT = -1;
    private readonly int ADD_ONE_COUNT = 1;
    private readonly int DEFAULT_SKIN_COUNT = 0;

    [Header("キャラクターデータ")]
    [SerializeField] private CharacterDatabase characterData;

    [Header("解放していないキャラクターを代用で表示させる")]
    [SerializeField] private GameObject unuseObject;
    [Header("解放していない場合に表示するUI")]
    [SerializeField] private GameObject unuseUI;

    [Header("キャラクターの名前テキスト")]
    [SerializeField] private TextMeshProUGUI nameText;
    [Header("キャラクターステータステキスト")]
    [SerializeField] private TextMeshProUGUI statusText;

    [Header("スキン選択ボタンを生成させる")]
    [SerializeField] private GameObject skinButton;
    [SerializeField] private Transform buttonParent;

    [Header("バナーデザインデータ")]
    [SerializeField] private BannerData bannerData;

    [Header("バナー画像を表示するオブジェクト")]
    [SerializeField] private Image bannerImage;

    //  親オブジェクトを保存
    private GameObject parent;

    //  キャラクターデータ格納
    private CharacterDatabase.CharacterInfo character;

    //  ネットワークで同期させるキャラクター番号
    private int networkCharacterCount;
    //  ローカルで同期させるキャラクター番号
    private int localCharacterCount = 0;

    //  プレハブ化したオブジェクトを保存
    private GameObject obj;

    //  表示する用のステータスデータ格納
    private int HP = 0;
    private int ATK = 0;
    private int SPD = 0;

    //  ネットワーク用
    private bool networkCanChange = false;
    //  そのキャラクターにチェンジできるかどうか
    private bool localCanChange = false;

    //  ネットワークで同期させるスキン番号
    private int networkSkinCount;
    //  ローカルで使用するスキン番号保持
    private int localSkinCount;

    //  ネットワークで同期させるバナー番号
    private int networkBannerCount;
    //  ローカルで同期させるばバナー番号
    private int localBannerCount = 0;

    //  初期は登録してあるプレハブの一番目を生成しておく
    private void Start() {
        //  親オブジェクトを自身にする
        parent = gameObject;
        //  UIを非表示
        unuseUI.SetActive(false);

        localCharacterCount = 0;

        //  子オブジェクトとして生成
        //  テキストを書き換える
        ChangeObject(localCharacterCount);
    }

    #region ボタン操作用関数
    /// <summary>
    /// 左右切り替えボタン
    /// </summary>
    public void OnChangeCharacterLeft() {
        AudioManager.Instance.CmdPlayUISE("選択");
        ChangeObject(SUB_ONE_COUNT);
    }
    public void OnChangeCharacterRight() {
        AudioManager.Instance.CmdPlayUISE("選択");
        ChangeObject(ADD_ONE_COUNT);
    }

    /// <summary>
    /// スキン切り替えボタン
    /// </summary>
    /// <param name="skinCount"></param>
    public void OnChangeSkin(int skinCount) {
        localSkinCount = skinCount;
        ChangeCharacterObject(skinCount);
    }

    /// <summary>
    /// 左右バナー切り替えボタン
    /// </summary>
    public void OnChangeBannerLeft() {
        AudioManager.Instance.CmdPlayUISE("選択");
        ChangeBanner(SUB_ONE_COUNT);
    }
    public void OnChangeBannerRight() {
        AudioManager.Instance.CmdPlayUISE("選択");
        ChangeBanner(ADD_ONE_COUNT);
    }

    #endregion

    #region ローカルキャラクター選択UIの処理
    /// <summary>
    /// データの中身があるかどうかのチェック
    /// </summary>
    /// <returns></returns>
    private bool CheckCharacterData() {
        if(characterData == null || characterData.characters == null || characterData.characters.Count == 0) {
            Debug.LogError("CharacterDatabaseが空、または設定されていません。");
            return false;
        }

        return true;
    }

    /// <summary>
    /// 数値を増減する（数値が一周したら戻す）
    /// </summary>
    /// <param name="count"></param>
    /// <param name="num"></param>
    /// <returns></returns>
    private int CheckCharacterCount(int count, int num) {
        //  増減
        count += num;
        //  最大値を保存
        int max = characterData.characters.Count - 1;
        //  数値が最大値より大きくなったら0に戻す
        if (count > max) return 0;
        //  数値が0を下回ったら最大値にする
        if (count < 0) return max;
        //  何もなければそのまま返す
        return count;
    }

    /// <summary>
    /// キャラクター選択時のメイン処理
    /// </summary>
    /// <param name="num"></param>
    private void ChangeObject(int num) {
        if (!CheckCharacterData()) return;
        //  数値を増減
        localCharacterCount = CheckCharacterCount(localCharacterCount, num);
        //  characterCount番目のキャラクターを取得して格納
        character = characterData.characters[localCharacterCount];
        //  スキン番号を初期の番号に戻す
        localSkinCount = DEFAULT_SKIN_COUNT;
        //  スキン選択ボタンの取得
        GenerateButtons();
        //  キャラクターがまだ取得されていない場合
        if (!PlayerItemManager.Instance.HasCharacter(character.characterName)) {
            UnuseCharacter();
            return;
        }
        //  キャラクターを所持している場合はそのまま生成
        UseCharacter();
    }

    /// <summary>
    /// 使用不可能キャラクターなら
    /// </summary>
    private void UnuseCharacter() {
        //  UIを表示
        unuseUI.SetActive(true);
        //  キャラチェンジ不可能
        localCanChange = false;
        //  先に生成されているものがあるなら消す
        if (obj != null) Destroy(obj);
        //  使用不可能専用オブジェクトに切り替える
        obj = Instantiate(unuseObject, parent.transform);
        //  名前テキストを切り替える
        ChangeNameText();
        //  ステータステキストを切り替える
        ChangeStatusText();
    }

    /// <summary>
    /// 使用可能キャラクターなら
    /// </summary>
    private void UseCharacter() {
        //  UIを非表示
        unuseUI.SetActive(false);
        //  キャラチェンジ可能
        localCanChange = true;
        //  キャラクターを切り替える
        ChangeCharacterObject(DEFAULT_SKIN_COUNT);
        //  名前テキストを切り替える
        ChangeNameText();
        //  ステータステキストを切り替える
        ChangeStatusText();
    }

    /// <summary>
    /// キャラクターを切り替える
    /// </summary>
    /// <param name="skinCount"></param>
    private void ChangeCharacterObject(int skinCount) {
        //  nullチェック、インデクスの範囲外防止
        if (character.skins == null || character.skins.Count == 0) return;
        skinCount = Mathf.Clamp(skinCount, 0, character.skins.Count - 1);
        //  スキンがまだ取得されていない場合、またはデフォルトスキンを持っていない場合
        if (!PlayerItemManager.Instance.HasSkin(character.characterName, character.skins[skinCount].skinName)
            || !PlayerItemManager.Instance.HasSkin(character.characterName, character.skins[DEFAULT_SKIN_COUNT].skinName)) 
        {
            UnuseCharacter();
            return;
        }
        //  UIを非表示
        unuseUI.SetActive(false);
        //  先に生成されているものがあるなら消す
        if (obj != null) Destroy(obj);
        GameObject prefab = character.skins[skinCount].skinPrefab;
        //  子オブジェクトとして生成
        obj = Instantiate(prefab, parent.transform);
    }

    /// <summary>
    /// 名前変更
    /// </summary>
    private void ChangeNameText() {
        string characterName = character.characterName;
        if (characterName == null) {
            nameText.SetText("Name");
            return;
        }
        nameText.SetText(characterName);
    }

    /// <summary>
    /// ステータステキストを切り替える
    /// </summary>
    private void ChangeStatusText() {
        SetStatusText();
        //  テキストに変換
        statusText.SetText("HP : " + HP + "\n"
                           + "ATK : " + ATK + "\n"
                           + "SPD : " + SPD + "\n");
    }

    /// <summary>
    /// ステータステキストにデータを代入
    /// </summary>
    private void SetStatusText() {
        //  ステータスデータ取得
        CharacterStatus characterStatuses = character.statusData;

        //  ステータスデータがなければ全て0にする
        if (characterStatuses == null) {
            HP = ATK = SPD = 0;
            return;
        }

        //  ステータスデータを代入する
        HP = characterStatuses.maxHP;
        ATK = characterStatuses.baseAttack;
        SPD = characterStatuses.moveSpeed;
    }

    /// <summary>
    /// ボタンをキャラごとに生成
    /// </summary>
    private void GenerateButtons() {
        //  nullチェック
        if (character.skins == null || character.skins.Count == 0) return;
        //  子オブジェクトを全削除
        DestroyAllChildren(buttonParent);
        //  登録されているスキンの数だけ生成
        for (int i = 0, max = character.skins.Count; i < max; i++) {
            //  ボタン生成
            GameObject button = Instantiate(skinButton, buttonParent);
            //  ボタンのテキストを変更
            TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
            if(buttonText != null) buttonText.SetText(character.skins[i].skinName);
            //  ボタンのイベントに数値を追加
            int index = i;
            button.GetComponent<Button>().onClick.AddListener(() => OnChangeSkin(index));
        }
    }
    #endregion

    #region ローカルバナー選択UIの処理

    /// <summary>
    /// データの中身があるかどうかのチェック
    /// </summary>
    /// <returns></returns>
    private bool CheckBannerData() {
        if(bannerData == null) {
            Debug.LogError("BannerDataが空、または設定されていません。");
            return false;
        }
        return true;
    }

    /// <summary>
    /// 数値を増減する（数値が一周したら戻す）
    /// </summary>
    /// <param name="count"></param>
    /// <param name="num"></param>
    /// <returns></returns>
    private int CheckBannerCount(int count, int num) {
        //  増減
        count += num;
        //  最大値を保存
        int max = bannerData.bannerInfos.Count - 1;
        //  数値が最大値より大きくなったら0に戻す
        if (count > max) return 0;
        //  数値が0を下回ったら最大値にする
        if (count < 0) return max;
        //  何もなければそのまま返す
        return count;
    }

    private void ChangeBanner(int num) {
        if (!CheckBannerData()) return;
        //  数値を増減
        localBannerCount = CheckBannerCount(localBannerCount, num);
        //  バナーデータの○番目の数値を保存
        BannerData.BannerInfo bannerInfo = bannerData.bannerInfos[localBannerCount];
        //  バナー画像を変更させる
        bannerImage.sprite = bannerInfo.bannerImage;
    }

    #endregion

    #region 削除関数
    /// <summary>
    /// 指定の親オブジェクトの子オブジェクトを全部削除する
    /// </summary>
    /// <param name="parent"></param>
    private void DestroyAllChildren(Transform parent) {
        foreach (Transform child in parent) {
            Destroy(child.gameObject);
        }
    }
    #endregion

    #region ネットワーク同期
    /// <summary>
    /// 呼び出し元
    /// </summary>
    /// <param name="player"></param>
    public void ConfirmPlayerChange(GameObject player) {
        CmdPlayerChange(player, localCharacterCount, localSkinCount, localCanChange, localBannerCount);
        AppearanceChangeManager.instance.ChangeSkillUI(localCharacterCount);
    }

    //  クライアントからサーバーへ送信
    [Command(requiresAuthority = false)]
    public void CmdPlayerChange(
        GameObject player, int characterCount, int skinCount, bool canChange, int bannerCount) {
        //  サーバーが全員に通知
        RpcPlayerChange(player, characterCount, skinCount, canChange, bannerCount);
    }
    //  サーバーから全員へ同期
    [ClientRpc]
    public void RpcPlayerChange(
        GameObject player, int characterCount, int skinCount, bool canChange, int bannerCount) {
        //  ローカルスキン番号をネットワークスキン番号に反映
        networkSkinCount = skinCount;
        //  ローカルキャラクター番号をネットワークキャラクター番号に反映
        networkCharacterCount = characterCount;
        //  ローカルチェンジ判定をネットワークチェンジ判定に反映
        networkCanChange = canChange;
        //  一旦仮でバナーを変更できるかどうか
        player.GetComponent<CharacterBase>().bannerNum = bannerCount;

        //  自分自身のプレイヤー変更
        AppearanceChangeManager.instance.PlayerChange(player, networkCharacterCount, networkSkinCount, networkCanChange);
        //  他のスキンを同時に読み込む
        AppearanceSyncManager.instance.CmdRequestAllStates();
    }
    #endregion

}
