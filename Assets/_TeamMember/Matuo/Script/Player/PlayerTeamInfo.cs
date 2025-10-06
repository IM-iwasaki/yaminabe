using Mirror;
using UnityEngine;

/// <summary>
/// プレイヤーのチーム情報(仮)
/// </summary>
public class PlayerTeamInfo : NetworkBehaviour {
    [SyncVar] public int teamId = 0;  // チームID
}