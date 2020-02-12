namespace Logitech.XRToolkit.UIHelpers
{
    using Logitech.XRToolkit.Utils;
    using System.Collections;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Toggle the colour of a Renderer material between two colours.
    /// </summary>
    [RequireComponent(typeof(Toggle))]
    public class ToggleObjectColor : MonoBehaviour
    {
        private Toggle _toggle;

        [SerializeField]
        private Renderer _renderer;
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
            _toggle = GetComponent<Toggle>();
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
                StartCoroutine(Animate(_renderer.material.color, toColor, _animationSpeed));
            }
            else
            {
                _renderer.material.color = toColor;
            }
        }

        private IEnumerator Animate(Color fromColor, Color toColor, float speed)
        {
            while (_renderer.material.color != toColor)
            {
                _renderer.material.color = Color.Lerp(fromColor, toColor, Time.deltaTime * speed);
                yield return null;
            }
        }
    }
}
