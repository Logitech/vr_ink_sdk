namespace Logitech.XRToolkit.Interactions
{
    using Logitech.XRToolkit.Inking;
    using Logitech.XRToolkit.Providers;
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

        void Start()
        {
            _drawingAction.Init();
            var depthCue = GetComponentInChildren<DepthCue>();
            if (depthCue != null) depthCue.SetRaycastSource(new StylusModelTransformProvider().GetOutput());
        }

        void LateUpdate()
        {
            _drawingAction.Update(_drawingTrigger.GetOutput() > 0);
        }
    }
}
