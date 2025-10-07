using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// サーバー上で生成処理を統一させる為のコード
/// </summary>

public interface ISystem {
    /// <summary>初期化順 (小さいほど先に初期化)</summary>
    int InitializationOrder { get; }
    /// <summary>初期化処理</summary>
    void Initialize();
}

