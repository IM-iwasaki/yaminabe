using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SelectObjectManager : MonoBehaviour {
    //  値を変更させる定数
    private readonly int SUB_ONE_COUNT = -1;
    private readonly int ADD_ONE_COUNT = 1;

    [Header("親オブジェクト")]
    [SerializeField] private GameObject parent;

    [Header("キャラクターのステータスデータ")]
    [SerializeField] public CharacterStatus[] characterStatuses;

    [Header("キャラクターの見た目プレハブ")]
    [SerializeField] public GameObject[] prefabs;

    [Header("キャラクターステータステキスト")]
    [SerializeField] private TextMeshProUGUI statusText;

    //  数値を保存するカウンター
    private int count = 0;
    //  プレハブ化したオブジェクトを保存
    private GameObject obj;

    //  初期は登録してあるプレハブの一番目を生成しておく
    private void Start() {
        if (prefabs == null) return;

        count = 0;

        //  子オブジェクトとして生成
        obj = Instantiate(prefabs[count], parent.transform);
    }

    //  左右切り替えボタン
    public void OnChangeLeft() {
        ChangeCharacterObject(SUB_ONE_COUNT);
    }
    public void OnChangeRight() {
        ChangeCharacterObject(ADD_ONE_COUNT);
    }

    //  キャラクターを切り替える
    public void ChangeCharacterObject(int num) {
        //  数値を増減
        count = CheckCount(count, num);
        Debug.Log(count);
        //  先に生成されているものがあるなら消す
        if(obj != null) Destroy(obj);
        //  子オブジェクトとして生成
        obj = Instantiate(prefabs[count], parent.transform);

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

}
