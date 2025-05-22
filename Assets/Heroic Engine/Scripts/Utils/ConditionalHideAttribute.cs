using UnityEngine;
using System;
using System.Collections.Generic;

namespace HeroicEngine.Utils.Editor
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property |
        AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
    public class ConditionalHideAttribute : PropertyAttribute
    {
        //The name of the bool field that will be in control
        public readonly string _conditionalSourceField;
        //TRUE = Hide in inspector / FALSE = Disable in inspector 
        public readonly bool _hideInInspector;
        public readonly List<object> _neededFieldValues;

        public ConditionalHideAttribute(string conditionalSourceField, object neededFieldValue)
        {
            _conditionalSourceField = conditionalSourceField;
            _hideInInspector = false;
            _neededFieldValues = new List<object> { neededFieldValue };
        }

        public ConditionalHideAttribute(string conditionalSourceField, bool hideInInspector, object neededFieldValue)
        {
            _conditionalSourceField = conditionalSourceField;
            _hideInInspector = hideInInspector;
            _neededFieldValues = new List<object> { neededFieldValue };
        }

        public ConditionalHideAttribute(string conditionalSourceField, bool hideInInspector, params object[] neededFieldValues)
        {
            _conditionalSourceField = conditionalSourceField;
            _hideInInspector = hideInInspector;
            _neededFieldValues = new List<object>();
            _neededFieldValues.AddRange(neededFieldValues);
        }
    }
}