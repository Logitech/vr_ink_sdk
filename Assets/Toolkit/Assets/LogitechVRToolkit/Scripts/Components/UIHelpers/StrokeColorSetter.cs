namespace Logitech.XRToolkit.UIHelpers
{
    using Logitech.XRToolkit.Inking;
    using UnityEngine;

    /// <summary>
    /// Change the Stylus inking colour.
    /// </summary>
    public class StrokeColorSetter : MonoBehaviour
    {
        [SerializeField]
        private Color _colour;
        private Renderer[] _penColourIndicators;

        public void SetColour()
        {
            _colour.a = 1f;
            DrawingVariables.Instance.Colour = _colour;
        }
    }
}
