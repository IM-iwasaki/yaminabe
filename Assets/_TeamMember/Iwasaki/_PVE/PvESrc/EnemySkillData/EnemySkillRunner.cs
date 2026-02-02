using UnityEngine;

/// <summary>
/// EnemySkillRunner
/// 
/// 【役割】
/// ・Enemy のスキル実行を「管理」するためのクラス
/// ・スキルそのものの処理は書かない
/// ・Coroutine を実行するための受け皿になる
///
/// 【想定される使い方】
/// EnemyAI
///   ↓（実行命令）
/// EnemySkillRunner
///   ↓（Coroutine 実行）
/// EnemySkill（IEnumerator）
///
/// 【このクラスに今は書かなくていいもの】
/// ・攻撃処理
/// ・ダメージ計算
/// ・エフェクト生成
///
/// </summary>
public class EnemySkillRunner : MonoBehaviour {

}
