using HeroicEngine.Components;
using UnityEditor;
using UnityEngine;

namespace HeroicEngine.Utils.Editor
{
    [CustomPropertyDrawer(typeof(ObjectSaveMode))]
    public class ObjectSaveModeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            property.intValue = EditorGUI.MaskField(position, label, property.intValue, property.enumDisplayNames);
        }
    }
}