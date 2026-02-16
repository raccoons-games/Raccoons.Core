#if UNITY_EDITOR
using Raccoons.Builds.Adapters.SRDebuggerAdapter;
using UnityEditor;
using UnityEngine;

namespace Raccoons.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(BaseBuildAdapterSettings), true)] // true = include derived classes
    public class BaseBuildAdapterSettingsDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            
            float y = position.y;
            
            string adapterName = GetAdapterName();
            
            var headerRect = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(headerRect, $"{adapterName} Settings", EditorStyles.boldLabel);
            y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            
            var lineRect = new Rect(position.x, y, position.width, 1);
            EditorGUI.DrawRect(lineRect, new Color(0.5f, 0.5f, 0.5f, 0.3f));
            y += 3 + EditorGUIUtility.standardVerticalSpacing;
            
            EditorGUI.indentLevel++;
            
            SerializedProperty iterator = property.Copy();
            SerializedProperty endProperty = property.GetEndProperty();
            
            if (iterator.NextVisible(true))
            {
                do
                {
                    if (SerializedProperty.EqualContents(iterator, endProperty))
                        break;
                    
                    var rect = new Rect(position.x, y, position.width, EditorGUI.GetPropertyHeight(iterator, true));
                    EditorGUI.PropertyField(rect, iterator, true);
                    y += EditorGUI.GetPropertyHeight(iterator, true) + EditorGUIUtility.standardVerticalSpacing;
                    
                } while (iterator.NextVisible(false));
            }
            
            EditorGUI.indentLevel--;
            
            EditorGUI.EndProperty();
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight; // Header
            height += EditorGUIUtility.standardVerticalSpacing;
            height += 4;
            
            SerializedProperty iterator = property.Copy();
            SerializedProperty endProperty = property.GetEndProperty();
            
            if (iterator.NextVisible(true))
            {
                do
                {
                    if (SerializedProperty.EqualContents(iterator, endProperty))
                        break;
                    
                    height += EditorGUI.GetPropertyHeight(iterator, true);
                    height += EditorGUIUtility.standardVerticalSpacing;
                    
                } while (iterator.NextVisible(false));
            }
            
            return height;
        }
        
        private string GetAdapterName()
        {
            if (fieldInfo != null)
            {
                string typeName = fieldInfo.FieldType.Name;
                typeName = typeName.Replace("Settings", "").Replace("BuildAdapter", "");
                
                return System.Text.RegularExpressions.Regex.Replace(typeName, "(\\B[A-Z])", " $1");
            }
            
            return "Adapter";
        }
    }
}
#endif