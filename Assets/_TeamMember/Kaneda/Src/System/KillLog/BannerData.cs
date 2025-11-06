using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BannerData", menuName = "Banner/BannerData")]
public class BannerData : ScriptableObject
{
    [Header("登録されているバナー一覧")]
    public List<BannerInfo> bannerInfos = new();

    [System.Serializable]
    public class BannerInfo {
        [Header("バナー名")]
        public string bannerName;
        [Header("バナー画像")]
        public Sprite bannerImage;
    }

}
