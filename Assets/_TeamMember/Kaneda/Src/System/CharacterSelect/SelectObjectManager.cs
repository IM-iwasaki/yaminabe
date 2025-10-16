using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using TMPro;
using UnityEngine;

public class SelectObjectManager : MonoBehaviour {
    //  値を変更させる定数
    private readonly int SUB_ONE_COUNT = -1;
    private readonly int ADD_ONE_COUNT = 1;

    [Header("キャラクターのステータスデータ")]
    [SerializeField] public CharacterStatus[] characterStatuses;

    [Header("キャラクターの見た目プレハブ")]
    [SerializeField] public GameObject[] prefabs;

    [Header("キャラクターステータステキスト")]
    [SerializeField] private TextMeshProUGUI statusText;

    //  親オブジェクトを保存
    private GameObject parent;
    //  数値を保存するカウンター
    private int count = 0;
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

        if (prefabs == null) return;

        count = 0;

        //  子オブジェクトとして生成
        ChangeCharacterObject(count);
        //  テキストを書き換える
        ChangeStatusText(count);
    }

    //  左右切り替えボタン
    public void OnChangeLeft() {
        ChangeObject(SUB_ONE_COUNT);
    }
    public void OnChangeRight() {
        ChangeObject(ADD_ONE_COUNT);
    }

    //  キャラクター選択時のメイン処理
    private void ChangeObject(int num) {
        //  数値を増減
        count = CheckCount(count, num);
        Debug.Log(count);
        //  キャラクターを切り替える
        ChangeCharacterObject(count);
        //  ステータステキストを切り替える
        ChangeStatusText(count);
    }

    //  数値を増減する（数値が一周したら戻す）
    private int CheckCount(int count, int num) {
        //  増減
        count += num;
        //  最大値を保存
        int max = prefabs.Length -1;
        //  数値が最大値より大きくなったら0に戻す
        if (count > max) return 0;
        //  数値が0を下回ったら最大値にする
        if (count < 0) return max;
        //  何もなければそのまま返す
        return count;
    }

    //  キャラクターを切り替える
    private void ChangeCharacterObject(int count) {
        //  先に生成されているものがあるなら消す
        if(obj != null) Destroy(obj);
        //  子オブジェクトとして生成
        obj = Instantiate(prefabs[count], parent.transform);
    }

    //  ステータステキストを切り替える
    private void ChangeStatusText(int count) {
        SetStatusText(count);
        //  テキストに変換
        statusText.SetText("HP : " + HP + "\n"
                           + "ATK : " + ATK + "\n"
                           + "SPD : " + SPD + "\n");
    }

    //  ステータステキストにデータを代入
    private void SetStatusText(int count) {
        //  ステータスデータがなければ全て0にする
        if (characterStatuses[count] == null) {
            HP = ATK = SPD = 0;
            return;
        }
        //  ステータスデータを代入する
        HP = characterStatuses[count].MaxHP;
        ATK = characterStatuses[count].Attack;
        SPD = characterStatuses[count].MoveSpeed;
    }

}
