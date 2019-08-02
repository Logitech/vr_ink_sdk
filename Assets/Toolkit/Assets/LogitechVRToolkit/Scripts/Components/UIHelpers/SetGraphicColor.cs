namespace Logitech.XRToolkit.UIHelpers
{
    using Logitech.XRToolkit.Utils;
    using System.Collections;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Set the colour of a Graphic.
    /// </summary>
    [RequireComponent(typeof(Toggle))]
    public class SetGraphicColor : MonoBehaviour
    {
        [SerializeField]
        private Graphic _graphic;
        [SerializeField]
        private Color _targetColor;
        [SerializeField]
        private bool _fade;
        [SerializeField, ShowIf("_fade")]
        private float _animationSpeed;

        public void SetColor()
        {
            if (_fade)
            {
                StopAllCoroutines();
                StartCoroutine(Animate(_graphic.color, _targetColor, _animationSpeed));
            }
            else
            {
                _graphic.color = _targetColor;
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
