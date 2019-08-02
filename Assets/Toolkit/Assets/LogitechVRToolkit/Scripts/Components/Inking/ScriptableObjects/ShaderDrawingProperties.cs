namespace Logitech.XRToolkit.Inking
{
    using UnityEngine;

    /// <summary>
    /// Holds information used by a ShaderDrawingAction.
    /// </summary>
    [CreateAssetMenu(fileName = "ShaderDrawingProperties")]
    public class ShaderDrawingProperties : ScriptableObject
    {
        public float Offset = 0.001f;
        public float Spacing = 1f;
        public float MinWidth;
        public ComputeShader ComputeShaderRef;
        public Vector2 TextureDimension = new Vector2(7680 / 2f, 4320 / 2f);
        public int UndoHistorySize = 24;
    }
}
