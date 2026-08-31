#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(BuildableDatabase))]
public class MyDataAssetEditor : Editor
{
    private ReorderableList _list;

    private void OnEnable()
    {
        var prop = serializedObject.FindProperty("_buildables");
        _list = new ReorderableList(serializedObject, prop, true, true, true, true);

        _list.drawHeaderCallback = rect =>
            EditorGUI.LabelField(rect, "_buildables");

        _list.elementHeightCallback = index =>
            EditorGUI.GetPropertyHeight(prop.GetArrayElementAtIndex(index), true) + 4;

        _list.drawElementCallback = (rect, index, active, focused) =>
        {
            rect.y += 2;
            EditorGUI.PropertyField(
                new Rect(rect.x, rect.y, rect.width, rect.height - 4),
                prop.GetArrayElementAtIndex(index), true);
        };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        _list.DoLayoutList();
        serializedObject.ApplyModifiedProperties();
    }
}
#endif