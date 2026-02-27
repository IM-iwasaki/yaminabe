using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Explane")]
public class ExplaneScentences : ScriptableObject
{
    [TextArea(3, 6)] public List<string> explanes;
}
