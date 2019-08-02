namespace Logitech.XRToolkit.Editor
{
    using Logitech.XRToolkit.IO;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Draws the GUI for TouchZones in a <see cref="TrackedDevice_Trackpad"/>.
    /// </summary>
    [CustomPropertyDrawer(typeof(TrackedDevice_Trackpad.TouchZone))]
    public class TouchZoneDrawer : PropertyDrawer
    {
        // Draw the property inside the given rect.
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            // Don't make child fields be indented
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Calculate rects
            var zoneRect = new Rect(position.x, position.y, position.width * 0.2f, position.height);
            var centerLabelRect = new Rect(position.x + position.width * 0.2f, position.y, position.width * 0.1f,
                position.height);
            var centerRect = new Rect(position.x + position.width * 0.3f, position.y, position.width * 0.3f,
                position.height);
            var sizeLabelRect = new Rect(position.x + position.width * 0.6f, position.y, position.width * 0.1f,
                position.height);
            var sizeRect = new Rect(position.x + position.width * 0.7f, position.y, position.width * 0.3f,
                position.height);

            var style = new GUIStyle(GUI.skin.label) {alignment = TextAnchor.MiddleRight};

            // Draw fields.
            EditorGUI.PropertyField(zoneRect, property.FindPropertyRelative("Zone"), GUIContent.none);
            EditorGUI.LabelField(centerLabelRect, "Center:", style);
            EditorGUI.PropertyField(centerRect, property.FindPropertyRelative("Center"), GUIContent.none);
            EditorGUI.LabelField(sizeLabelRect, "Size:", style);
            EditorGUI.PropertyField(sizeRect, property.FindPropertyRelative("Size"), GUIContent.none);

            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
    }
}
