using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// UnityのEditorでバイナリファイルの中身を変更する用
/// </summary>
public class SaveDataEditorWindow : EditorWindow {
    private PlayerData data;
    private Vector2 scrollPos;

    [MenuItem("Tools/SaveData Editor")]
    public static void OpenWindow() {
        GetWindow<SaveDataEditorWindow>("SaveData Editor");
    }

    private void OnEnable() {
        LoadData();
    }

    private void LoadData() {
        data = PlayerSaveData.Load();

        if (data.items == null)
            data.items = new List<string>();

        if (string.IsNullOrEmpty(data.playerName))
            data.playerName = "default";
    }

    private void SaveData() {
        PlayerSaveData.Save(data);
        Debug.Log("Save Completed!");
    }

    private void OnGUI() {
        if (data == null) {
            if (GUILayout.Button("Load Data"))
                LoadData();
            return;
        }

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        EditorGUILayout.LabelField("Player Save Data", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Player Name
        data.playerName = EditorGUILayout.TextField("Player Name", data.playerName);

        // Money
        data.currentMoney = EditorGUILayout.IntField("Current Money", data.currentMoney);

        // Rate
        data.currentRate = EditorGUILayout.IntField("Current Rate", data.currentRate);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Items", EditorStyles.boldLabel);

        // アイテムリスト表示
        for (int i = 0; i < data.items.Count; i++) {
            EditorGUILayout.BeginHorizontal();

            data.items[i] = EditorGUILayout.TextField($"Item {i}", data.items[i]);

            if (GUILayout.Button("X", GUILayout.Width(25))) {
                data.items.RemoveAt(i);
                i--;
            }

            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("Add Item")) {
            data.items.Add("");
        }

        EditorGUILayout.Space(20);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Reload")) {
            LoadData();
        }

        if (GUILayout.Button("Save")) {
            SaveData();
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndScrollView();
    }
}