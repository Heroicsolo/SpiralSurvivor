using UnityEngine;
using UnityEditor;

namespace HeroicEngine.Utils.Editor
{
    [CustomPropertyDrawer(typeof(ConditionalHideAttribute))]
    public class ConditionalHidePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var condHAtt = (ConditionalHideAttribute)attribute;
            var enabled = GetConditionalHideAttributeResult(condHAtt, property);

            var wasEnabled = GUI.enabled;
            GUI.enabled = enabled;
            if (!condHAtt._hideInInspector || enabled)
            {
                EditorGUI.PropertyField(position, property, label, true);
            }

            GUI.enabled = wasEnabled;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var condHAtt = (ConditionalHideAttribute)attribute;
            var enabled = GetConditionalHideAttributeResult(condHAtt, property);

            if (!condHAtt._hideInInspector || enabled)
            {
                return EditorGUI.GetPropertyHeight(property, label);
            }
            else
            {
                return -EditorGUIUtility.standardVerticalSpacing;
            }
        }

        private bool GetConditionalHideAttributeResult(ConditionalHideAttribute condHAtt, SerializedProperty property)
        {
            var enabled = true;
            var propertyPath = property.propertyPath; //returns the property path of the property we want to apply the attribute to
            var conditionPath = propertyPath.Replace(property.name, condHAtt._conditionalSourceField); //changes the path to the conditionalsource property path
            var sourcePropertyValue = property.serializedObject.FindProperty(conditionPath);

            if (sourcePropertyValue != null)
            {
                var fieldValue = sourcePropertyValue.GetPropertyValue();

                var comparingValue = "";

                condHAtt._neededFieldValues.ForEach(val => comparingValue += val + ",");

                var fieldValueString = fieldValue.ToString();

                enabled = comparingValue.Contains(fieldValueString);
            }
            else
            {
                Debug.LogWarning("Attempting to use a ConditionalHideAttribute but no matching SourcePropertyValue found in object: " + condHAtt._conditionalSourceField);
            }

            return enabled;
        }
    }
}
