namespace Logitech.XRToolkit.Editor
{
    using Logitech.XRToolkit.Utils;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Draws the GUI for properties that have a <see cref="LabelAttribute"/>.
    /// </summary>
    [CustomPropertyDrawer(typeof(LabelAttribute))]
    public class LabelDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            LabelAttribute labelAttribute = (LabelAttribute)attribute;
            EditorGUI.PropertyField(position, property, new GUIContent(labelAttribute.Name));
        }
    }
}

