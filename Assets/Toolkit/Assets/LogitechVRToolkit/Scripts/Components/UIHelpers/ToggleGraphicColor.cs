namespace Logitech.XRToolkit.UIHelpers
{
    using Logitech.XRToolkit.Utils;
    using System.Collections;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Toggle the colour of a Graphic between two colours.
    /// </summary>
    [RequireComponent(typeof(Toggle))]
    public class ToggleGraphicColor : MonoBehaviour
    {
        private Toggle _toggle;

        [SerializeField]
        private Graphic _graphic;

        [SerializeField]
        private Color _onColor;

        [SerializeField]
        private Color _offColor;

        [SerializeField]
        private bool _fade;

        [SerializeField, ShowIf("_fade")]
        private float _animationSpeed;

        private void Start()
        {
            Toggle();
        }

        public void Toggle()
        {
            if (_toggle == null)
            {
                _toggle = GetComponent<Toggle>();
            }
            Color toColor = _toggle.isOn ? _onColor : _offColor;

            if (_fade)
            {
                StopAllCoroutines();
                StartCoroutine(Animate(_graphic.color, toColor, _animationSpeed));
            }
            else
            {
                _graphic.color = toColor;
            }
        }

        private IEnumerator Animate(Color fromColor, Color toColor, float speed)
        {
            while (_graphic.color != toColor)
            {
                _graphic.color = Color.Lerp(fromColor, toColor, Time.deltaTime * speed);
                yield return null;
            }
        }
    }
}
