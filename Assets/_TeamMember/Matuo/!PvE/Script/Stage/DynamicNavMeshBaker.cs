using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class DynamicNavMeshBaker : MonoBehaviour {
    public void BakeStage(GameObject stageRoot) {
        // まず 1つの Surface を stageRoot に作る
        var surface = stageRoot.GetComponent<NavMeshSurface>();
        if (surface == null) {
            surface = stageRoot.AddComponent<NavMeshSurface>();
        }

        surface.collectObjects = CollectObjects.Children;  // 子オブジェクトの全てをまとめる
        surface.useGeometry = NavMeshCollectGeometry.RenderMeshes;

        // Bake
        surface.BuildNavMesh();

        Debug.Log($"DynamicNavMeshBaker: Stage全体の NavMesh をBakeしました");
    }
}