using System.Linq;
using UnityEditor;
using UnityEngine;

namespace T2D.Editor
{
    [CustomPropertyDrawer(typeof(SortingLayer))]
    public class SortingLayerDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            position.height = EditorGUIUtility.singleLineHeight;

            EditorGUI.BeginChangeCheck();
            int id = property.FindPropertyRelative("Id").intValue;
            id = EditorGUI.IntPopup(position, EditorGUIUtility.TrTempContent("Sorting Layer"), 
                id, UnityEngine.SortingLayer.layers.Select(l => new GUIContent(l.name)).ToArray(), UnityEngine.SortingLayer.layers.Select(l => l.id).ToArray());
            if (EditorGUI.EndChangeCheck())
                property.FindPropertyRelative("Id").intValue = id;

            position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("Order"), EditorGUIUtility.TrTempContent("Sorting Order"));
            
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) * 2 + EditorGUIUtility.standardVerticalSpacing;
        }
    }
}

