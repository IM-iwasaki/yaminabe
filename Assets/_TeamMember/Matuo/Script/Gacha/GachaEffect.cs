using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ガチャ中に表示する演出
/// </summary>
public class GachaEffect : MonoBehaviour {

    [Header("集中線設定")]
    [SerializeField] private int lineCount = 32;
    [SerializeField] private float lineLength = 5f;
    [SerializeField] private float lineWidth = 0.05f;
    [SerializeField] private float rotateSpeed = 60f;

    [Header("キラキラ設定")]
    [SerializeField] private int sparkleCount = 80;

    private readonly List<GameObject> lineObjects = new();
    private ParticleSystem sparkle;
    private bool isPlaying;

    #region Public API

    public void Play(Rarity rarity) {
        if (isPlaying) return;
        isPlaying = true;

        Color color = GetRarityColor(rarity);

        CreateLines(color);
        CreateSparkle(color, rarity);

        StartCoroutine(RotateLines());
    }

    public void Stop() {
        isPlaying = false;

        // 集中線だけ消す
        foreach (var obj in lineObjects) {
            if (obj != null)
                Destroy(obj);
        }
        lineObjects.Clear();

        // キラキラだけ消す
        if (sparkle != null)
            Destroy(sparkle.gameObject);

        StopAllCoroutines();
    }

    #endregion

    #region 集中線

    private void CreateLines(Color color) {
        for (int i = 0; i < lineCount; i++) {
            GameObject obj = new GameObject($"Line_{i}");
            obj.transform.SetParent(transform, false);

            float angle = (360f / lineCount) * i;
            obj.transform.localRotation = Quaternion.Euler(0, 0, angle);

            LineRenderer lr = obj.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.useWorldSpace = false;
            lr.startWidth = lineWidth;
            lr.endWidth = lineWidth;
            lr.material = new Material(Shader.Find("Unlit/Color"));
            lr.material.color = color;

            lr.SetPosition(0, Vector3.zero);
            lr.SetPosition(1, Vector3.up * lineLength);

            lineObjects.Add(obj);
        }
    }

    private IEnumerator RotateLines() {
        while (isPlaying) {
            transform.Rotate(Vector3.forward, rotateSpeed * Time.deltaTime);
            yield return null;
        }
    }

    #endregion

    #region キラキラ

    private void CreateSparkle(Color color, Rarity rarity) {
        GameObject obj = new GameObject("Sparkle");
        obj.transform.SetParent(transform, false);

        sparkle = obj.AddComponent<ParticleSystem>();

        // Particle 設定
        var main = sparkle.main;
        main.startColor = color;
        main.startLifetime = 0.6f;
        main.startSpeed = rarity >= Rarity.Legendary ? 3f : 1.5f;
        main.startSize = rarity >= Rarity.Legendary ? 0.25f : 0.15f;
        main.maxParticles = sparkleCount;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;

        var emission = sparkle.emission;
        emission.rateOverTime = sparkleCount;

        var shape = sparkle.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 1.5f;

        // Renderer
        var renderer = sparkle.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(
            Shader.Find("Particles/Standard Unlit")
        );
        renderer.renderMode = ParticleSystemRenderMode.Billboard;

        sparkle.Play();
    }

    #endregion

    #region Utility

    private Color GetRarityColor(Rarity rarity) {
        return rarity switch {
            Rarity.Common => Color.white,
            Rarity.Rare => new Color(0.2f, 0.6f, 1f),
            Rarity.Epic => new Color(0.8f, 0.3f, 1f),
            Rarity.Legendary => new Color(1f, 0.85f, 0.2f),
            _ => Color.white
        };
    }

    #endregion
}