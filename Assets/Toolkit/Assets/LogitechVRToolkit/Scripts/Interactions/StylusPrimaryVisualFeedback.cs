namespace Logitech.XRToolkit.Interactions
{
    using Logitech.XRToolkit.Providers;
    using UnityEngine;

    /// <summary>
    /// Updates a MeshRenderer that has a circle shader material to indicate pressure on the primary button of a Stylus.
    /// </summary>
    public class StylusPrimaryVisualFeedback : MonoBehaviour
    {
        [SerializeField]
        private AxisValueProvider _axisValueProvider;
        [SerializeField]
        private MeshRenderer _circleShaderMaterialMeshRenderer;
        [SerializeField]
        private AnimationCurve _mappedCurve;

        private void Update()
        {
            float mappedPressureValue = _axisValueProvider.GetOutput();
            if (mappedPressureValue > 0)
            {
                mappedPressureValue = _mappedCurve.Evaluate(_axisValueProvider.GetOutput());
            }
            _circleShaderMaterialMeshRenderer.sharedMaterial.SetFloat("_BackgroundCutoff", 1 - mappedPressureValue);
        }
    }
}
