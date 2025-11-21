using UnityEngine;

/// <summary>
/// CharacterBase.TeamID に合わせて、プレイヤーをチームカラーで発光させるコンポーネント。
/// ・TeamID = redTeamId / blueTeamId のときだけ光る
/// ・TeamID = -1 など、それ以外は光らない
/// ・自分のキャラは（オプションで）光らせない
/// </summary>
[RequireComponent(typeof(CharacterBase))]
public class TeamGlowSimple : MonoBehaviour {
    [Header("光らせる Renderer 群")]
    [Tooltip("空なら自動で子階層から Renderer を全部拾う")]
    public Renderer[] targetRenderers;

    [Header("TeamID 設定")]
    [Tooltip("赤チームの TeamID（例：0）")]
    public int redTeamId = 0;

    [Tooltip("青チームの TeamID（例：1）")]
    public int blueTeamId = 1;

    [Header("チームごとの発光色")]
    [Tooltip("赤チーム用の発光色")]
    public Color redTeamColor = Color.red;

    [Tooltip("青チーム用の発光色")]
    public Color blueTeamColor = Color.blue;

    [Header("発光の強さ")]
    [Tooltip("Emission の強さ。数字を上げるほど眩しくなる")]
    public float emissionIntensity = 2.0f;

    [Header("オプション")]
    [Tooltip("true にすると、自分のキャラだけ光らせない")]
    public bool hideSelfGlow = true;

    // 内部用
    CharacterBase character;
    int lastTeamId = int.MinValue;

    void Awake() {
        // 対象キャラ取得
        character = GetComponent<CharacterBase>();

        // 対象 Renderer 自動取得（Inspector で指定していればそちら優先）
        if (targetRenderers == null || targetRenderers.Length == 0) {
            targetRenderers = GetComponentsInChildren<Renderer>();
        }
    }

    void Start() {
        // 初回反映
        ApplyGlow(true);
    }

    void LateUpdate() {
        if (character == null) return;

        // TeamID が変わったら色を更新
        if (character.TeamID != lastTeamId) {
            ApplyGlow(true);
        }
        else {
            // TeamID は同じでも、「自分だけ非表示」設定は毎フレームチェックしておく
            ApplyGlow(false);
        }
    }

    /// <summary>
    /// 発光色と「自分だけ非表示」を反映する
    /// </summary>
    /// <param name="updateColor">true のときは TeamID 変化も考慮して色を再計算</param>
    void ApplyGlow(bool updateColor) {
        if (targetRenderers == null || targetRenderers.Length == 0) return;
        if (character == null) return;

        // 「自分のキャラを光らせない」設定
        bool visible = !(hideSelfGlow && character.isLocalPlayer);

        // TeamID からベースカラー取得
        Color teamColor = GetColorByTeamId(character.TeamID);

        // 未所属（= Color.black） or 自分を隠す → 発光なし
        Color emissionColor;
        if (!visible || teamColor == Color.black) {
            emissionColor = Color.black;
        }
        else {
            // 強さを乗算して発光させる
            emissionColor = teamColor * emissionIntensity;
        }

        lastTeamId = character.TeamID;

        // 全ての Renderer に対して Emission を設定
        foreach (var r in targetRenderers) {
            if (r == null) continue;

            // インスタンス側のマテリアルを取得
            var mat = r.material;
            if (mat == null) continue;

            mat.SetColor("_EmissionColor", emissionColor);

            if (emissionColor == Color.black) {
                // 真っ黒なら発光キーワードを切っておく（完全に非発光）
                mat.DisableKeyword("_EMISSION");
            }
            else {
                // 光らせるときはキーワードを ON
                mat.EnableKeyword("_EMISSION");
            }
        }
    }

    /// <summary>
    /// TeamID → チームカラー
    /// ・redTeamId / blueTeamId 以外は Color.black（＝光らない）
    /// </summary>
    Color GetColorByTeamId(int id) {
        if (id == redTeamId) {
            // 赤チーム
            return redTeamColor;
        }
        if (id == blueTeamId) {
            // 青チーム
            return blueTeamColor;
        }

        // それ以外（-1 や未設定）は光らせない
        return Color.black;
    }
}
