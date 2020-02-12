namespace Logitech.XRToolkit.Components
{
    using Logitech.XRToolkit.Utils;

    using UnityEngine;
    using UnityEngine.Events;

    public class MarkerMenuInteractable : MarkerMenuItem
    {
        public UnityEvent OnMarkerMenuRelease;

        [SerializeField]
        private bool _isResizeableOnHighlight = false;

        [SerializeField, ShowIf("_isResizeableOnHighlight")]
        private float _sizeFactor = 2;

        private Vector3 _scaleFactor = Vector3.one;
        private bool _isHilighted = false;

        private void Start()
        {
            if (OnMarkerMenuRelease == null)
            {
                OnMarkerMenuRelease = new UnityEvent();
            }
        }

        private void Update()
        {
            UIContainer.transform.position = Vector3.Lerp(UIContainer.transform.position, transform.position, Time.deltaTime * UISpeedFactor);
            if (_isResizeableOnHighlight && _isHilighted)
            {
                UIContainer.transform.localScale = Vector3.Lerp(UIContainer.transform.localScale, _scaleFactor, Time.deltaTime * UISpeedFactor);
            }
            else if (_isResizeableOnHighlight && !_isHilighted)
            {
                UIContainer.transform.localScale = Vector3.Lerp(UIContainer.transform.localScale, Vector3.one, Time.deltaTime * UISpeedFactor);
            }
        }

        private void OnEnable()
        {
            ResetItemToDefaultState();
            _scaleFactor = new Vector3(_sizeFactor, _sizeFactor, 1);
        }

        public override void ResetItemToDefaultState()
        {
            UIContainer.GetComponent<Renderer>().material.color = InitialColor;
            _isHilighted = false;
        }

        public override void SetVisualStartingPoint(Vector3 startingPoint)
        {
            UIContainer.transform.position = startingPoint;
        }

        public void SetAsActiveItem()
        {
            _isHilighted = true;
        }
    }
}
