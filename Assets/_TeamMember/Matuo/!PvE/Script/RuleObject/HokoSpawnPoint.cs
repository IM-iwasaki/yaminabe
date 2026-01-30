using UnityEngine;

/// <summary>
/// PVE用ホコのスポーンポイント
/// ・StageManager が参照してホコを生成する
/// </summary>
public class HokoSpawnPoint : MonoBehaviour {

    [Header("このポイントから生成するホコの数")]
    public int spawnCount = 1;
}