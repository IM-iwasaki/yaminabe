using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectObjectManager : MonoBehaviour {
    //  値を変更させる定数
    private readonly int SUB_ONE_COUNT = -1;
    private readonly int ADD_ONE_COUNT = 1;
    private readonly int DEFAULT_SKIN_COUNT = 0;

    [Header("キャラクターデータ")]
    [SerializeField] private CharacterDatabase data;

    [Header("解放していないキャラクターを代用で表示させる")]
    [SerializeField] private GameObject unuseObject;
    [Header("解放していない場合に表示するUI")]
    [SerializeField] private GameObject unuseUI;

    [Header("キャラクターステータステキスト")]
    [SerializeField] private TextMeshProUGUI statusText;

    [Header("スキン選択ボタンを生成させる")]
    [SerializeField] private GameObject skinButton;
    [SerializeField] private Transform buttonParent;

    //  親オブジェクトを保存
    private GameObject parent;

    //  キャラクターデータ格納
    private CharacterDatabase.CharacterInfo character;

    //  数値を保存するカウンター
    private int characterCount = 0;

    //  プレハブ化したオブジェクトを保存
    private GameObject obj;

    //  表示する用のステータスデータ格納
    private int HP = 0;
    private int ATK = 0;
    private int SPD = 0;

    //  初期は登録してあるプレハブの一番目を生成しておく
    private void Start() {
        //  親オブジェクトを自身にする
        parent = gameObject;
        //  UIを非表示
        unuseUI.SetActive(false);

        characterCount = 0;

        //  子オブジェクトとして生成
        //  テキストを書き換える
        ChangeObject(characterCount);
    }

    //  左右切り替えボタン
    public void OnChangeLeft() {
        ChangeObject(SUB_ONE_COUNT);
    }
    public void OnChangeRight() {
        ChangeObject(ADD_ONE_COUNT);
    }

    //  スキン切り替えボタン
    public void OnChangeSkin(int skinCount) {
        ChangeCharacterObject(skinCount);
    }

    //  データの中身があるかどうかのチェック
    private bool CheckData() {
        if(data == null || data.characters == null || data.characters.Count == 0) {
            Debug.LogError("CharacterDatabaseが空、または設定されていません。");
            return false;
        }

        return true;
    }

    //  キャラクター選択時のメイン処理
    private void ChangeObject(int num) {
        if (!CheckData()) return;
        //  数値を増減
        characterCount = CheckCount(characterCount, num);
        //  characterCount番目のキャラクターを取得して格納
        character = data.characters[characterCount];
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

    //  数値を増減する（数値が一周したら戻す）
    private int CheckCount(int count, int num) {
        //  増減
        count += num;
        //  最大値を保存
        int max = data.characters.Count - 1;
        //  数値が最大値より大きくなったら0に戻す
        if (count > max) return 0;
        //  数値が0を下回ったら最大値にする
        if (count < 0) return max;
        //  何もなければそのまま返す
        return count;
    }

    //  使用不可能キャラクターなら
    private void UnuseCharacter() {
        //  UIを表示
        unuseUI.SetActive(true);
        //  先に生成されているものがあるなら消す
        if (obj != null) Destroy(obj);
        //  使用不可能専用オブジェクトに切り替える
        obj = Instantiate(unuseObject, parent.transform);
        //  テキストを全て？にする
        statusText.SetText("HP : ?" + "\n"
                   + "ATK : ?" + "\n"
                   + "SPD : ?" + "\n");
    }

    //  使用可能キャラクターなら
    private void UseCharacter() {
        //  UIを非表示
        unuseUI.SetActive(false);
        //  キャラクターを切り替える
        ChangeCharacterObject(DEFAULT_SKIN_COUNT);
        //  ステータステキストを切り替える
        ChangeStatusText();
    }

    //  キャラクターを切り替える
    private void ChangeCharacterObject(int skinCount) {
        //  nullチェック、インデクスの範囲外防止
        if (character.skins == null || character.skins.Count == 0) return;
        skinCount = Mathf.Clamp(skinCount, 0, character.skins.Count - 1);
        //  スキンがまだ取得されていない場合
        if (!PlayerItemManager.Instance.HasSkin(character.characterName, character.skins[skinCount].skinName)) {
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

    //  ステータステキストを切り替える
    private void ChangeStatusText() {
        SetStatusText();
        //  テキストに変換
        statusText.SetText("HP : " + HP + "\n"
                           + "ATK : " + ATK + "\n"
                           + "SPD : " + SPD + "\n");
    }

    //  ステータステキストにデータを代入
    private void SetStatusText() {
        //  ステータスデータ取得
        CharacterStatus characterStatuses = character.statusData;

        //  ステータスデータがなければ全て0にする
        if (characterStatuses == null) {
            HP = ATK = SPD = 0;
            return;
        }

        //  ステータスデータを代入する
        HP = characterStatuses.MaxHP;
        ATK = characterStatuses.Attack;
        SPD = characterStatuses.MoveSpeed;
    }

    //  ボタンをキャラごとに生成
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

    //  指定の親オブジェクトの子オブジェクトを全部削除する
    private void DestroyAllChildren(Transform parent) {
        foreach (Transform child in parent) {
            Destroy(child.gameObject);
        }
    }

}
