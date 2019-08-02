namespace Logitech.XRToolkit.UIHelpers
{
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Set a text element based on the value of a slider.
    /// </summary>
    [RequireComponent(typeof(Slider))]
    public class SetTextFromSlider : MonoBehaviour
    {
        [SerializeField]
        private bool _inverse;
        [SerializeField]
        private Text _text;
        [SerializeField]
        private string _prefix;
        [SerializeField]
        private string _postfix;
        private Slider _slider;

        private void Start()
        {
            _slider = GetComponent<Slider>();
            UpdateText();
        }

        public void UpdateText()
        {
            var value = _inverse ? 100 - _slider.value : _slider.value;
            _text.text = _prefix + value + _postfix;
        }
    }
}
