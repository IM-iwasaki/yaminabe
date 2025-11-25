using UnityEngine;

/// <summary>
/// シーン内 / ゲーム中に生成された CharacterBase 全員に
/// TeamGlowSimple を設定するマネージャー。
/// </summary>
public class TeamGlowManager : MonoBehaviour {
    public static TeamGlowManager Instance { get; private set; }

    [Header("共通 TeamID 設定")]
    [Tooltip("赤チームの TeamID（例：0）")]
    public int redTeamId = 0;

    [Tooltip("青チームの TeamID（例：1）")]
    public int blueTeamId = 1;

    [Header("共通カラー設定")]
    public Color redTeamColor = Color.red;
    public Color blueTeamColor = Color.blue;

    [Header("発光強度")]
    public float emissionIntensity = 2.0f;

    [Header("オプション")]
    [Tooltip("true にすると自分のキャラだけ光らせない")]
    public bool hideSelfGlow = true;

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 既にいるプレイヤーに一括適用したいとき用
    /// </summary>
    public void ApplyGlowToAllPlayers() {
        var players = FindObjectsOfType<CharacterBase>();
        foreach (var p in players) {
            RegisterPlayer(p);
        }
    }

    /// <summary>
    /// 新しくスポーンした CharacterBase から呼ぶ想定
    /// </summary>
    public void RegisterPlayer(CharacterBase player) {
        if (player == null) return;

        // すでに付いていれば再利用
        var glow = player.GetComponent<TeamGlowSimple>();
        if (glow == null) {
            glow = player.gameObject.AddComponent<TeamGlowSimple>();
        }

        // 光らせる Renderer をまとめて取る（必要なら手で絞る）
        if (glow.targetRenderers == null || glow.targetRenderers.Length == 0) {
            glow.targetRenderers = player.GetComponentsInChildren<Renderer>();
        }

        // 共通設定を流し込む
        glow.redTeamId = redTeamId;
        glow.blueTeamId = blueTeamId;
        glow.redTeamColor = redTeamColor;
        glow.blueTeamColor = blueTeamColor;
        glow.emissionIntensity = emissionIntensity;
        glow.hideSelfGlow = hideSelfGlow;
    }
}
