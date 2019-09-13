namespace Logitech.XRToolkit.Interactions
{
    using Logitech.XRToolkit.Inking;
    using Logitech.XRToolkit.Providers;
    using Logitech.XRToolkit.Triggers;
    using UnityEngine;

    /// <summary>
    /// Lets one write on a surface. Should be placed directly on a flat quad that will receive the writing.
    /// </summary>
    [RequireComponent(typeof(MeshCollider))]
    public class ShaderDrawing : MonoBehaviour
    {
        [Header("Drawing")]
        [SerializeField]
        private AxisValueProvider _drawingTrigger;
        [SerializeField]
        private ShaderDrawingAction _drawingAction;

        /// <summary>
        /// The Shader Drawing Action is only able to undo the last 24 stroks, by saving the initial renderTexture, we are able to undo all properly here.
        /// </summary>
        [SerializeField, Header("ClearAll")]
        private InputTrigger _clearAllTrigger;

        private RenderTexture _drawingTexture;



        void Start()
        {
            Vector2 textureDimension = _drawingAction.ShaderDrawingProperties.TextureDimension;
            _drawingTexture =
                   new RenderTexture((int) textureDimension.x, (int) textureDimension.y, 1)
                   {
                       enableRandomWrite = true,
                       filterMode = FilterMode.Trilinear,
                   };
            _drawingAction.Init(_drawingTexture);
            DepthCue depthCue = GetComponentInChildren<DepthCue>();
            if (depthCue != null)
            {
                depthCue.SetRaycastSource(new StylusModelTransformProvider().GetOutput());
            }
        }

        void LateUpdate()
        {
            // Properly erase all even with the 24 limits from the shader.
            if (_clearAllTrigger.IsValid())
            {
                _drawingAction.EraseAll();
            }
                _drawingAction.Update(_drawingTrigger.GetOutput() > 0);
        }
    }
}
