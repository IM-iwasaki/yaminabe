using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PveBossSearchController : NetworkBehaviour
{
    //  ターゲット候補リスト
    private List<Transform> targets = new List<Transform>();

    //  離脱リスト+タイマー管理
    private Dictionary<Transform, float> exitTimers = new Dictionary<Transform, float>();

    [Header("離脱猶予時間")]
    [SerializeField] private float leaveGraceTime = 3.0f;
    [Header("ボスの索敵範囲")]
    [SerializeField] private float searchRadius = 3.0f;
    [Header("索敵コライダー")]
    [SerializeField] private SphereCollider col;

    private PveBossHpBarController hpBar;

    private void Awake() {
        col = col.GetComponent<SphereCollider>();
        col.radius = searchRadius;

        hpBar = GetComponent<PveBossHpBarController>();
    }

    //  現在のターゲット候補を取得
    public List<Transform> GetTargets() {
        //  削除リスト
        List<Transform> removeList = new List<Transform>();

        foreach (var t in targets) {
            //  ターゲット自体がNullなら削除リストに追加
            if (t == null) {
                removeList.Add(t);
                continue;
            }
            //  キャラクターの死亡判定を取得して削除リストに追加
            CharacterBase character = t.GetComponent<CharacterBase>();
            if (character == null || character.parameter == null || character.parameter.isDead) {
                removeList.Add(t);
            }
        }
        //  削除リストの中にあるものを全て削除
        foreach (var t in removeList) {
            targets.Remove(t);
            exitTimers.Remove(t);
        }

        return targets;
    }

    private void Update() {
        //  server以外で処理しない
        if (!isServer) return;
        //  誰も離脱していない場合はスキップ
        if (exitTimers.Count == 0) return;
        //  一度KeyをListに避難させる
        List<Transform> keys = new List<Transform>(exitTimers.Keys);
        //  削除予定リスト
        List<Transform> removeList = new List<Transform>();
        //  何秒離れているか見る
        foreach (var key in keys) {
            exitTimers[key] += Time.deltaTime;

            if (exitTimers[key] >= leaveGraceTime)
                removeList.Add(key);
        }
        //  削除リストの中身を消す
        foreach (var t in removeList) {
            exitTimers.Remove(t);
            targets.Remove(t);
            hpBar.HideBossUI();
        }
    }

    /// <summary>
    /// 索敵範囲内に入ったらターゲット候補に追加
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other) {
        //  server以外で処理しない
        if (!isServer) return;
        //  プレイヤーを取得し格納
        CharacterBase player = other.GetComponent<CharacterBase>();
        if(player == null) return;
        Transform t = player.transform;
        //  ボスのHPBarを表示させる
        if (hpBar != null) hpBar.ShowBossUI();

        //  プレイヤーをターゲット候補に追加
        if (!targets.Contains(t)) {
            targets.Add(t);
        }
        //  離脱中なら離脱をキャンセル
        if (exitTimers.ContainsKey(t)) {
            exitTimers.Remove(t);
        }
    }

    /// <summary>
    /// 索敵範囲外に行ったら離脱リストに追加
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerExit(Collider other) {
        //  server以外で処理しない
        if (!isServer) return;
        //  プレイヤーを取得し格納
        CharacterBase player = other.GetComponent<CharacterBase>();
        if (player == null) return;
        Transform t = player.transform;
        //  ターゲット候補に未登録ならスルー
        if(!targets.Contains(t)) return;
        //  離脱リストに追加
        if (!exitTimers.ContainsKey(t)) {
            exitTimers.Add(t, 0f);
        }
    }

}
