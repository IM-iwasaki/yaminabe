using System.Collections.Generic;
using UnityEngine;

public class GachaSystem : MonoBehaviour {
    public GachaData data;

    public GachaItem PullSingle() {
        int totalWeight = 0;
        foreach (var item in data.items)
            totalWeight += item.rate;

        int randomValue = Random.Range(0, totalWeight);
        int current = 0;

        foreach (var item in data.items) {
            current += item.rate;
            if (randomValue < current)
                return item;
        }

        return null; // –œˆê
    }

    public List<GachaItem> PullMultiple(int count) {
        List<GachaItem> result = new List<GachaItem>();
        for (int i = 0; i < count; i++) {
            result.Add(PullSingle());
        }
        return result;
    }
}