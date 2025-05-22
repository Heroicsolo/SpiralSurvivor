using System;
using UnityEditor;
using UnityEngine;

namespace HeroicEngine.Utils
{
    public static class ScriptableObjectsHelper
    {
#if UNITY_EDITOR
        public static object GetValue(this SerializedProperty property)
        {
            var parentType = property.serializedObject.targetObject.GetType();
            var fi = parentType.GetField(property.propertyPath);
            return fi.GetValue(property.serializedObject.targetObject);
        }

        public static void SetValue(this SerializedProperty property, object value)
        {
            var parentType = property.serializedObject.targetObject.GetType();
            var fi = parentType.GetField(property.propertyPath); //this FieldInfo contains the type.
            fi.SetValue(property.serializedObject.targetObject, value);
        }

        public static Type GetType(SerializedProperty property)
        {
            var parentType = property.serializedObject.targetObject.GetType();
            var fi = parentType.GetField(property.propertyPath);
            return fi.FieldType;
        }

        public static object GetPropertyValue(this SerializedProperty prop)
        {
            switch (prop.propertyType)
            {
                case SerializedPropertyType.Integer:
                    return prop.intValue;
                case SerializedPropertyType.Boolean:
                    return prop.boolValue;
                case SerializedPropertyType.Float:
                    return prop.floatValue;
                case SerializedPropertyType.String:
                    return prop.stringValue;
                case SerializedPropertyType.Color:
                    return prop.colorValue;
                case SerializedPropertyType.ObjectReference:
                    return prop.objectReferenceValue;
                case SerializedPropertyType.LayerMask:
                    return (LayerMask)prop.intValue;
                case SerializedPropertyType.Enum:
                    return prop.enumNames.Length == 0 ? "undefined" : prop.enumNames[prop.enumValueIndex];
                case SerializedPropertyType.Vector2:
                    return prop.vector2Value;
                case SerializedPropertyType.Vector3:
                    return prop.vector3Value;
                case SerializedPropertyType.Vector4:
                    return prop.vector4Value;
                case SerializedPropertyType.Rect:
                    return prop.rectValue;
                case SerializedPropertyType.ArraySize:
                    return prop.arraySize;
                case SerializedPropertyType.Character:
                    return (char)prop.intValue;
                case SerializedPropertyType.AnimationCurve:
                    return prop.animationCurveValue;
                case SerializedPropertyType.Bounds:
                    return prop.boundsValue;
                case SerializedPropertyType.Gradient:
                    return null;
            }

            return null;
        }
#endif
    }

    public class ScriptableObjectIdAttribute : PropertyAttribute
    {
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ScriptableObjectIdAttribute))]
    public class ScriptableObjectIdDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false;
            if (string.IsNullOrEmpty(property.stringValue))
            {
                property.stringValue = Guid.NewGuid().ToString();
            }
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
    }
#endif
}
