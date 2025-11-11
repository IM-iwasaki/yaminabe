using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

// GeneralCharacterStatus専用のインスペクターをカスタマイズするクラス
[CustomEditor(typeof(GeneralCharacterStatus))]
public class GeneralCharacterStatusAssistance : Editor {

    //StatusBaseのパス
    readonly string StatusBasePath = "Assets/_TeamMember/Kira/_ScriptableObjects/StatusBase.asset"; 

    public override void OnInspectorGUI() {
        var obj = (GeneralCharacterStatus)target;
        serializedObject.Update();

        //BaseStatusを探して自動でアタッチ
        var BaseStatus = AssetDatabase.LoadAssetAtPath<StatusBase>(StatusBasePath);
        if (BaseStatus != null)  EditorUtility.SetDirty(this);
        else Debug.LogWarning($"BaseStatusが見つかりません: {StatusBasePath}");
        serializedObject.FindProperty("baseStatus").objectReferenceValue = BaseStatus;

        //プロパティを表示
        EditorGUILayout.PropertyField(serializedObject.FindProperty("baseStatus"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("displayName"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("chatacterType"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxHPCorrection"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("attackCorrection"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("speedCorrection"));

        EditorGUILayout.Space();
        //キャラクタータイプで分岐
        switch (obj.chatacterType) {
            case CharacterEnum.CharaterType.Melee:
                EditorGUILayout.LabelField("近接職専用ステータスはまだありません！", EditorStyles.boldLabel);
                break;

            case CharacterEnum.CharaterType.Wizard:
                EditorGUILayout.LabelField("魔法職専用ステータス", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("maxMPCorrection"));
                break;

            case CharacterEnum.CharaterType.Gunner:
                EditorGUILayout.LabelField("間接職専用ステータス", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("maxMagazine"));
                break;
        }

        EditorGUILayout.PropertyField(serializedObject.FindProperty("passives"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("skills"));

        //反映
        serializedObject.ApplyModifiedProperties();
    }
}
