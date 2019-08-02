namespace Logitech.XRToolkit.Editor
{
    using Logitech.XRToolkit.Triggers;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Draws the GUI for a <see cref="PropertyStateTrigger"/>.
    /// </summary>
    [CustomPropertyDrawer(typeof(PropertyStateTrigger))]
    public class PropertyStateTriggerEditor : PropertyDrawer
    {
        private PropertyStateTrigger _propertyStateTrigger;
        private float _extraHeight;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            EditorGUI.PropertyField(position, property, label, true);

            var obj = fieldInfo.GetValue(property.serializedObject.targetObject);
            if (obj == null)
            {
                return;
            }

            if (obj.GetType().IsArray)
            {
                if (((PropertyStateTrigger[]) obj).Length > 0)
                {
                    int index = Convert.ToInt32(new string(property.propertyPath.Where(c => char.IsDigit(c)).ToArray()));
                    _propertyStateTrigger = ((PropertyStateTrigger[]) obj)[index];
                }
                else
                {
                    return;
                }
            }
            else
            {
                _propertyStateTrigger = (PropertyStateTrigger) obj;
            }

            if (_propertyStateTrigger.ComponentWithPublicProperties == null || _propertyStateTrigger.Condition == PropertyStateTrigger.BoolEnablingType.None)
            {
                _extraHeight = 0;
                foreach (var item in ActiveEditorTracker.sharedTracker.activeEditors)
                {
                    if (item.serializedObject == property.serializedObject)
                    {
                        item.Repaint();
                    }
                }
                return;
            }

            PopulateChoicesList();

            if (_propertyStateTrigger.Choices != null && _propertyStateTrigger.Choices.Length > 0)
            {
                if (_extraHeight != EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing)
                {
                    foreach (var item in ActiveEditorTracker.sharedTracker.activeEditors)
                    {
                        if (item.serializedObject == property.serializedObject)
                        {
                            item.Repaint();
                        }
                    }
                }

                Rect nextPosition = position;
                nextPosition.y += (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * (property.isExpanded ? 3 : 1);
                nextPosition.height = _extraHeight = property.isExpanded ? EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing : 0;

                EditorGUI.BeginChangeCheck();
                if (property.isExpanded)
                {
                    _propertyStateTrigger.ChoiceIndex = EditorGUI.Popup(nextPosition, _propertyStateTrigger.ChoiceIndex,
                        _propertyStateTrigger.Choices);
                }

                if (EditorGUI.EndChangeCheck())
                {
                    UpdateBlockingField();
                }
            }
            else
            {
                _propertyStateTrigger.ChoiceIndex = 0;
                UpdateBlockingField();

                if (_extraHeight != (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 2)
                {
                    foreach (var item in ActiveEditorTracker.sharedTracker.activeEditors)
                    {
                        if (item.serializedObject == property.serializedObject)
                        {
                            item.Repaint();
                        }
                    }
                }

                Rect nextPosition = position;
                nextPosition.y += (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 3;
                nextPosition.height = _extraHeight = (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 2;

                EditorGUI.HelpBox(nextPosition, _propertyStateTrigger.ComponentWithPublicProperties.GetType().Name + " does not have any public boolean fields or properties!", MessageType.Warning);
            }

            EditorGUI.EndProperty();
        }

        private void PopulateChoicesList()
        {
            List<string> choicesList = new List<string>();
            var fields = _propertyStateTrigger.ComponentWithPublicProperties.GetType().GetFields();
            var properties = _propertyStateTrigger.ComponentWithPublicProperties.GetType().GetProperties();
            foreach (FieldInfo property in fields)
            {
                if (property.FieldType == typeof(bool))
                {
                    choicesList.Add(property.Name);
                }
            }

            foreach (PropertyInfo property in properties)
            {
                if (property.PropertyType == typeof(bool))
                {
                    choicesList.Add(property.Name);
                }
            }

            if (_propertyStateTrigger.Choices.SequenceEqual(choicesList.ToArray()))
            {
                return;
            }

            _propertyStateTrigger.ChoiceIndex = 0;
            _propertyStateTrigger.Choices = choicesList.ToArray();
        }

        private void UpdateBlockingField()
        {
            if (_propertyStateTrigger.Choices.Length > 0 && _propertyStateTrigger.Condition != PropertyStateTrigger.BoolEnablingType.None)
            {
                _propertyStateTrigger.BlockingElement = _propertyStateTrigger.ComponentWithPublicProperties.GetType().GetMember(_propertyStateTrigger.Choices[_propertyStateTrigger.ChoiceIndex])[0];
            }
            else
            {
                _propertyStateTrigger.BlockingElement = null;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label) + _extraHeight;
        }
    }
}
