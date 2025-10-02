using UnityEngine;
using System.Collections.Generic;

//インスペクターでSystemObjectを継承したManagerを登録すること


public class SystemManager : MonoBehaviour {
    [Header("SystemObject プレハブ一覧")]
    [SerializeField] private List<GameObject> systemObjectPrefabs;


    void Awake() {
        DontDestroyOnLoad(gameObject);

        if (systemObjectPrefabs == null) {
            Debug.LogError("SystemObject Prefabs がセットされていません！");
            return;
        }

        foreach (var prefab in systemObjectPrefabs) {
            if (prefab == null) {
                Debug.LogWarning("systemObjectPrefabs に null が含まれています");
                continue;
            }

            GameObject instance = Instantiate(prefab);
            DontDestroyOnLoad(instance);

            var monoBehaviours = instance.GetComponents<MonoBehaviour>();
            foreach (var mono in monoBehaviours) {
                if (mono == null) continue;

                var type = mono.GetType();
                var baseType = type.BaseType;

                while (baseType != null) {
                    if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(SystemObject<>)) {
                        var method = type.GetMethod("Initialize");
                        if (method != null) {
                            method.Invoke(mono, null);
                        }
                        else {
                            Debug.LogWarning(type.Name + " に Initialize メソッドがありません");
                        }
                        break;
                    }
                    baseType = baseType.BaseType;
                }
            }
        }
    }
}
