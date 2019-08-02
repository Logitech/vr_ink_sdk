namespace Logitech.XRToolkit.Inking
{
    using System;
    using Utils;
    using UnityEngine;

    public enum EToolState
    {
        AirDrawing,
        SurfaceDrawing
    }

    public enum EBrushMode
    {
        Airbrush,
        Inking,
        Shading,
        Splatter
    }

    [Serializable]
    public class DrawingVariables : SingletonBehaviour<DrawingVariables>
    {
        /// <summary>
        /// Helps maintain consistency between lines drawn on planes and in the air. If a line drawn on a surface has
        /// width W, a line in the air of width AIR_WIDTH_MODIFIER * W will have the same apparent width for most people.
        /// </summary>
        public const float AirWidthModifier = 1f;

        public Color Colour = Color.black;
        public float LineMaxWidth = 0.01f;

        [HideInInspector]
        public EToolState PenToolState = EToolState.AirDrawing;

        public EBrushMode BrushMode;
        public DrawingModificationProperties AirbrushMode;
        public DrawingModificationProperties InkingMode;
        public DrawingModificationProperties ShadingMode;
        public DrawingModificationProperties SplatterMode;
    }
}
