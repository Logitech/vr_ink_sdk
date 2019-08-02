namespace Logitech.XRToolkit.UIHelpers
{
    using Logitech.XRToolkit.Inking;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Set the Stylus stroke width.
    /// </summary>
    [RequireComponent(typeof(Slider))]
    public class StrokeSizeSetter : MonoBehaviour
    {
        private enum SliderSettings
        {
            SetLineMaxWidthToSliderValue,
            SetSliderValueToLineMaxWidth
        }

        [SerializeField, Header("On Enable")]
        private SliderSettings _onEnableSliderSetting;

        public void SetStrokeSize(float value)
        {
            DrawingVariables.Instance.LineMaxWidth = value;
        }

        private void OnEnable()
        {
            if (_onEnableSliderSetting == SliderSettings.SetLineMaxWidthToSliderValue)
            {
                DrawingVariables.Instance.LineMaxWidth = GetComponent<Slider>().value;
            }
            else // _sliderSetting == SliderSettings.SetSliderValueToLineMaxWidth
            {
                GetComponent<Slider>().value = DrawingVariables.Instance.LineMaxWidth;
            }
        }
    }
}
