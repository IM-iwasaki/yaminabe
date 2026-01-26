using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 見た目を変更させる実行
/// </summary>
public class AppearanceChangeManager : MonoBehaviour {
    //  インスタンス化
    public static AppearanceChangeManager instance;

    private GameObject currentSkin = null;

    [Header("キャラクターデータ")]
    [SerializeField] private CharacterDatabase data;

    //  ここでインスタンス化
    private void Awake() {
        instance = this;
    }

    /// <summary>
    /// 指定の親オブジェクトのタグ付き子オブジェクトを削除する
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="tag"></param>
    private void DestroyChildrenWithTag(Transform parent, string tag) {
        if (tag == null) return;

        foreach (Transform child in parent) {
            if (child.CompareTag(tag)) {
                child.gameObject.SetActive(false);
                Destroy(child.gameObject);
            }
        }
    }

    /// <summary>
    /// プレイヤーに現在のキャラクターを反映させる
    /// </summary>
    /// <param name="player"></param>
    public void PlayerChange(GameObject player, int characterCount, int skinCount, bool canChange) {
        //  チェンジ不可なら処理しない
        if (!canChange) return;
        //  タグを取り破棄する
        DestroyChildrenWithTag(player.transform, "Skin");
        //  親の位置を保存
        Transform parent = player.transform;
        //  生成位置を取る
        Vector3 spawnPos = parent.position + parent.TransformDirection(Vector3.down);
        //  スキンの番号を同期させる
        GameObject prefab = data.characters[characterCount].skins[skinCount].skinPrefab;
        //  プレイヤーの子オブジェクトに生成
        var characterSkin = Instantiate(prefab, spawnPos, parent.rotation, parent);
        //  プレイヤーのステータスを置き換える
        player.GetComponent<CharacterBase>().parameter.StatusInport(data.characters[characterCount].statusData);
        //  プレイヤーのIDを取得・格納
        uint netId = player.GetComponent<NetworkIdentity>().netId;
        //  変更したデータを保存する
        AppearanceSyncManager.instance.RecordAppearance(netId, characterCount, skinCount);

        // ここから下変えましたbyまつお

        // CharacterAnimationController を取得
        var animController = player.GetComponent<CharacterAnimationController>();

        // 生成した Skin から Animator を取得
        var animator = characterSkin.GetComponent<Animator>();

        // Animator を直接差し替える（NetworkAnimator 不使用）
        animController.anim = animator;
        StartCoroutine(animController.AddNetworkAnimator(characterSkin));

    }

    public void ChangeSkillUI(int characterCount) {
        SkillBase skill = data.characters[characterCount].statusData.skills[0];
        PassiveBase passive = data.characters[characterCount].statusData.passives[0];

        SkillDisplayer.Instance.SetSkillUI(
            skill.skillName, skill.skillDescription,
            passive.passiveName, passive.passiveDescription
            );
    }
}
