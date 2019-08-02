namespace Logitech.XRToolkit.Editor
{
    using Logitech.XRToolkit.Utils;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Draws the GUI for properties that have a <see cref="EnableIfAttribute"/>.
    /// </summary>
    /// <seealso cref="DisableIfDrawer"/>
    /// <seealso cref="ShowIfDrawer"/>
    /// <seealso cref="HideIfDrawer"/>
    [CustomPropertyDrawer(typeof(EnableIfAttribute))]
    public class EnableIfDrawer : PropertyDrawer
    {
        // Reference to the attribute on the property.
        private EnableIfAttribute _enableIf;

        // Reference to the property that is being compared.
        private SerializedProperty _comparedProperty;

        /// <summary>
        /// Overrides the PropertyDrawer base class OnGUI() method.
        /// </summary>
        /// <param name="position">Rectangle on the screen to use for the property GUI.</param>
        /// <param name="property">The SerializedProperty to make the custom GUI for.</param>
        /// <param name="label">The label of this property.</param>
        /// <remarks>
        /// <para>This will either draw the element normally or draw the element as readonly.</para>
        /// </remarks>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // If the values are equal, draw the property normally.
            if (CompareProperty(property))
            {
                EditorGUI.PropertyField(position, property, true);
            }
            else
            {
                GUI.enabled = false;
                EditorGUI.PropertyField(position, property, true);
                GUI.enabled = true;
            }
        }

        /// <summary>
        /// Compares the property value with a defined value.
        /// </summary>
        /// <param name="property">The SerializedProperty to make the custom GUI for.</param>
        /// <returns>
        /// If the comparison passed or failed.
        /// </returns>
        /// <remarks>
        /// Unsupported or incompatible types return false by default.
        /// TODO: Move this code somewhere else as it applies to many attributes.
        /// </remarks>
        private bool CompareProperty(SerializedProperty property)
        {
            UpdateDrawerReferences(property);

            if (_comparedProperty == null)
            {
                Debug.LogError("Cannot find property with name: " + _enableIf.ComparedPropertyName);
                return true;
            }

            // Based on the property type, compare its value. Add more supported types here.
            switch (_comparedProperty.type)
            {
                case "bool":
                    if (_enableIf.ComparedValue.GetType() == typeof(bool))
                    {
                        return _comparedProperty.boolValue.Equals(_enableIf.ComparedValue);
                    }

                    Debug.LogError("Error: " + _comparedProperty.type + " and " + _enableIf.ComparedValue.GetType() + " Are not the same type!");
                    return true;

                case "Enum":
                    if (_enableIf.ComparedValue.GetType().IsEnum)
                    {
                        return _comparedProperty.enumValueIndex.Equals((int) _enableIf.ComparedValue);
                    }

                    Debug.LogError("Error: " + _comparedProperty.type + " and " + _enableIf.ComparedValue.GetType() + " Are not the same type!");
                    return true;

                default:
                    Debug.LogError("Error: " + _comparedProperty.type + " is not supported. Supported types are: bool, enum");
                    return true;
            }
        }

        /// <summary>
        /// Updates the current attribute and compared property references.
        /// </summary>
        /// <param name="property">The SerializedProperty to make the custom GUI for.</param>
        /// <remarks>
        /// TODO: Move this code somewhere else as it applies to many attributes.
        /// </remarks>
        private void UpdateDrawerReferences(SerializedProperty property)
        {
            _enableIf = (EnableIfAttribute) attribute;

            string path = property.propertyPath.Contains(".") ? System.IO.Path.ChangeExtension(property.propertyPath, _enableIf.ComparedPropertyName) : _enableIf.ComparedPropertyName;
            _comparedProperty = property.serializedObject.FindProperty(path);
        }
    }
}
