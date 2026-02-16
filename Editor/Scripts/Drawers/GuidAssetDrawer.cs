using System.IO;
using Raccoons.Identifiers.Guids;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(GuidAsset), true)]
public class GuidAssetDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Calculate height of single line
        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = 2f;

        // Rect for the object field
        Rect fieldRect = new Rect(position.x, position.y, position.width, lineHeight);
        EditorGUI.PropertyField(fieldRect, property, label);

        Rect buttonRect = new Rect(position.x, position.y + lineHeight + spacing, position.width, lineHeight);
        if (GUI.Button(buttonRect, "Create new guid"))
        {
            var newAsset = ScriptableObject.CreateInstance<GuidAsset>();
            StringInputPopup.Show(
                "New Guid Name",
                property.serializedObject.targetObject.name,
                result =>
                {
                    if (!string.IsNullOrEmpty(result))
                    {
                        EnsureFolderExists();

                        AssetDatabase.CreateAsset(newAsset, Path.Combine("Assets", "Resources", "Guids", result + ".asset"));
                        AssetDatabase.SaveAssets();
                        property.objectReferenceValue = newAsset;
                        property.serializedObject.ApplyModifiedProperties();
                    }
                });
        }

    }

    private static void EnsureFolderExists()
    {
        string resourcesPath = Path.Combine("Assets", "Resources");
        if (!AssetDatabase.IsValidFolder(resourcesPath))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }
        string guidsPath = Path.Combine(resourcesPath, "Guids");
        if (!AssetDatabase.IsValidFolder(guidsPath))
        {
            AssetDatabase.CreateFolder(resourcesPath, "Guids");
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = 2f;

        return lineHeight * 2 + spacing;
    }

}