namespace Logitech.XRToolkit.Editor
{
    using Logitech.XRToolkit.Utils;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Draws the GUI for properties that have a <see cref="HideIfAttribute"/>.
    /// </summary>
    /// <seealso cref="ShowIfDrawer"/>
    /// <seealso cref="DisableIfDrawer"/>
    /// <seealso cref="EnableIfDrawer"/>
    [CustomPropertyDrawer(typeof(HideIfAttribute))]
    public class HideIfDrawer : PropertyDrawer
    {
        // Reference to the attribute on the property.
        private HideIfAttribute _hideIf;

        // Reference to the property that is being compared.
        private SerializedProperty _comparedProperty;

        /// <summary>
        /// Overrides the PropertyDrawer base class OnGUI() method.
        /// </summary>
        /// <param name="position">Rectangle on the screen to use for the property GUI.</param>
        /// <param name="property">The SerializedProperty to make the custom GUI for.</param>
        /// <param name="label">The label of this property.</param>
        /// <remarks>
        /// <para>This will either draw the element normally if the comparison fails</para>
        /// <para>GetPropertyHeight will determine if the element will be hidden.</para>
        /// </remarks>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!CompareProperty(property))
            {
                EditorGUI.PropertyField(position, property, true);
            }
        }

        /// <summary>
        /// Overrides the PropertyDrawer base class method for getting the property height.
        /// </summary>
        /// <param name="property">The SerializedProperty to make the custom GUI for.</param>
        /// <param name="label">The label of this property.</param>
        /// <returns>
        /// The height of the property.
        /// </returns>
        /// <remarks>
        /// This hides the property if the comparison passes.
        /// </remarks>>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (CompareProperty(property))
            {
                return -2f;
            }

            // Leave the height of the property as default
            return EditorGUI.GetPropertyHeight(property, label);
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
                Debug.LogError("Cannot find property with name: " + _hideIf.ComparedPropertyName);
                return false;
            }

            // Based on the property type, compare its value. Add more supported types here.
            switch (_comparedProperty.type)
            {
                case "bool":
                    if (_hideIf.ComparedValue.GetType() == typeof(bool))
                    {
                        return _comparedProperty.boolValue.Equals(_hideIf.ComparedValue);
                    }

                    Debug.LogError("Error: " + _comparedProperty.type + " and " + _hideIf.ComparedValue.GetType() + " Are not the same type!");
                    return false;

                case "Enum":
                    if (_hideIf.ComparedValue.GetType().IsEnum)
                    {
                        return _comparedProperty.enumValueIndex.Equals((int) _hideIf.ComparedValue);
                    }

                    Debug.LogError("Error: " + _comparedProperty.type + " and " + _hideIf.ComparedValue.GetType() + " Are not the same type!");
                    return false;

                default:
                    Debug.LogError("Error: " + _comparedProperty.type + " is not supported. Supported types are: bool, enum");
                    return false;
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
            _hideIf = (HideIfAttribute) attribute;

            string path = property.propertyPath.Contains(".") ? System.IO.Path.ChangeExtension(property.propertyPath, _hideIf.ComparedPropertyName) : _hideIf.ComparedPropertyName;
            _comparedProperty = property.serializedObject.FindProperty(path);
        }
    }
}
