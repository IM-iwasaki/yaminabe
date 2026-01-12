using UnityEngine;
using UnityEngine.InputSystem;   // F9トグル用（新InputSystem）

/// <summary>
/// CharacterBase.TeamID に合わせて、プレイヤーをチームカラーで発光させるコンポーネント。
/// ・TeamID = redTeamId / blueTeamId のときだけ光る
/// ・TeamID が該当しない（例：-1）は光らない
/// ・hideSelfGlow が true のとき、このクライアントのローカルプレイヤーは光らない
/// ・キャラ/スキン切り替えに対応するため、Renderer を自動で取り直すオプション付き
/// </summary>
[RequireComponent(typeof(CharacterBase))]
public class TeamGlowSimple : MonoBehaviour {
    [Header("光らせる Renderer 群")]
    [Tooltip("空なら自動で子階層から Renderer を拾う")]
    public Renderer[] targetRenderers;

    [Header("Renderer 自動更新")]
    [Tooltip("true にすると毎フレーム Renderer を取り直してスキン切り替えに追従する")]
    public bool autoRefreshRenderers = true;

    [Header("TeamID 設定")]
    [Tooltip("赤チームの TeamID（例：0）")]
    public int redTeamId = 0;

    [Tooltip("青チームの TeamID（例：1）")]
    public int blueTeamId = 1;

    [Header("チームごとの発光色")]
    public Color redTeamColor = Color.red;
    public Color blueTeamColor = Color.blue;

    [Header("発光の強さ")]
    [Tooltip("Emission の強さ。数字を上げるほど眩しくなる")]
    public float emissionIntensity = 2.0f;

    [Header("オプション")]
    [Tooltip("true のとき、このクライアント視点で自キャラは光らない")]
    public bool hideSelfGlow = true;

    // 内部
    private CharacterBase character;
    private int lastTeamId = int.MinValue;

    void Awake() {
        character = GetComponent<CharacterBase>();

        // 最初の一回、Renderer を拾っておく
        if (targetRenderers == null || targetRenderers.Length == 0) {
            RefreshRenderers();
        }
    }

    void Start() {
        ApplyGlow(true);
    }

    void Update() {
        // デバッグ用：F9 で「自分も光る / 光らない」を切り替え
        if (Keyboard.current != null && Keyboard.current.f9Key.wasPressedThisFrame) {
            hideSelfGlow = !hideSelfGlow;
            ApplyGlow(true);    // 即反映
            Debug.Log($"[TeamGlowSimple] hideSelfGlow = {hideSelfGlow}");
        }
    }

    void LateUpdate() {
        if (character == null) return;

        // ■ ここが重要：スキン/キャラ切り替えに追従するための更新
        if (autoRefreshRenderers) {
            RefreshRenderers();
        }
        else {
            // キャッシュモードのとき、配列が空 or null なら保険で再取得
            if (targetRenderers == null || targetRenderers.Length == 0) {
                RefreshRenderers();
            }
        }

        if (character.parameter.TeamID != lastTeamId) {
            ApplyGlow(true);
        }
        else {
            ApplyGlow(false);
        }
    }

    /// <summary>
    /// 今のキャラ階層から Renderer を取り直す
    /// （キャラクター切り替え・スキン切り替え後も呼べば追従できる）
    /// </summary>
    public void RefreshRenderers() {
        targetRenderers = GetComponentsInChildren<Renderer>();
    }

    /// <summary>
    /// 発光色と「自分だけ非表示」を反映する
    /// </summary>
    void ApplyGlow(bool updateColor) {
        if (targetRenderers == null || targetRenderers.Length == 0) return;
        if (character == null) return;

        bool isSelf = character.isLocalPlayer;

        // TeamID → ベース色
        Color teamColor = GetColorByTeamId(character.parameter.TeamID);

        // 未所属 or (自キャラかつ hideSelfGlow が true) → 発光なし
        Color emissionColor;
        if (teamColor == Color.black || (isSelf && hideSelfGlow)) {
            emissionColor = Color.black;
        }
        else {
            emissionColor = teamColor * emissionIntensity;
        }

        lastTeamId = character.parameter.TeamID;

        foreach (var r in targetRenderers) {
            if (r == null) continue;

            var mat = r.material;
            if (mat == null) continue;

            mat.SetColor("_EmissionColor", emissionColor);

            if (emissionColor == Color.black) {
                mat.DisableKeyword("_EMISSION");
            }
            else {
                mat.EnableKeyword("_EMISSION");
            }
        }
    }

    /// <summary>
    /// TeamID → チームカラー
    /// </summary>
    Color GetColorByTeamId(int id) {
        if (id == redTeamId) return redTeamColor;
        if (id == blueTeamId) return blueTeamColor;
        return Color.black; // 未所属は光らない
    }
}
