namespace Logitech.XRToolkit.Components
{
    using Logitech.XRToolkit.Actions;
    using Logitech.XRToolkit.Utils;

    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.UI;

    public class MarkerMenuSliderInteractable : MarkerMenuItem
    {
        public enum ESliderOrientation
        {
            SliderHorizontal,
            SliderVertical
        }

        public UnityEvent OnMarkerMenuRelease;

        [SerializeField]
        private bool _isResizeableOnHighlight = false;

        [SerializeField, ShowIf("_isResizeableOnHighlight")]
        private float _sizeFactor = 2;

        [Header("Slider info")]
        private Vector3 _initialUIScale;

        private Vector3 _scaleFactor = Vector3.one;
        private bool _isHilighted = false;

        [SerializeField]
        private Slider _endItemSlider;

        [SerializeField]
        private float _minimumSliderValue = 0.0f;

        [SerializeField]
        private float _maximumSliderValue = 1.0f;

        [SerializeField]
        private float _initialSliderValue = 0.5f;

        [HideInInspector]
        public float _sliderValue;

        [SerializeField]
        private float _sliderSensitivityFactor = 10.0f;

        private Vector3 _hitPlanCoordinate;

        [Header("Haptics"), SerializeField]
        private bool _enableHaptics = true;

        [SerializeField, ShowIf("_enableHaptics")]
        private HapticAction _vibrationOnActivation;

        [Tooltip("The rate at which haptic ticks will occur, from every 0% - every 100%")]
        [SerializeField, Range(0, 100), ShowIf("_enableHaptics")]
        private float _hapticTickRate;

        private float _minTickValue;
        private float _maxTickValue;

        private void Start()
        {
            _sliderValue = _initialSliderValue;
            _initialUIScale = UIContainer.transform.localScale;
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
                UIContainer.transform.localScale = Vector3.Lerp(UIContainer.transform.localScale, _initialUIScale, Time.deltaTime * UISpeedFactor);
            }
            if (_isHilighted)
            {
                float distFromSliderCenter = transform.localPosition.x - _hitPlanCoordinate.x;
                if (distFromSliderCenter == 0)
                {
                    _endItemSlider.value = (_minimumSliderValue + _maximumSliderValue) / 2;
                }
                else if (distFromSliderCenter < 0)
                {
                    _endItemSlider.value = ((_minimumSliderValue + _maximumSliderValue) / 2) - (distFromSliderCenter * ((_minimumSliderValue + _maximumSliderValue) / 2) * _sliderSensitivityFactor);
                }
                else if (distFromSliderCenter > 0)
                {
                    _endItemSlider.value = ((_minimumSliderValue + _maximumSliderValue) / 2) + (distFromSliderCenter * ((_minimumSliderValue + _maximumSliderValue) / 2) * -_sliderSensitivityFactor);
                }
                _sliderValue = _endItemSlider.value;

                if (_endItemSlider != null)
                {
                    SliderMove(_sliderValue);
                }
            }
        }

        private void OnEnable()
        {
            ResetItemToDefaultState();
            _scaleFactor = new Vector3(_initialUIScale.x * _sizeFactor, _initialUIScale.y * _sizeFactor, _initialUIScale.z);
            _endItemSlider.maxValue = _maximumSliderValue;
            _endItemSlider.minValue = _minimumSliderValue;
            SetCurrentHapticTick();
            if (_sliderValue != _endItemSlider.value)
            {
                _sliderValue = _endItemSlider.value;
            }
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

        public void SetHitPlanCoordinate(Vector3 hitPlanCoordinate)
        {
            _hitPlanCoordinate = transform.InverseTransformPoint(hitPlanCoordinate);
        }

        private void SetCurrentHapticTick()
        {
            Debug.Assert(_hapticTickRate <= 100, "Haptic tick rate is larger than 100% and therefore will never trigger!");

            var tickValue = _hapticTickRate * (_endItemSlider.maxValue / 100);
            var currentTickIndex = _endItemSlider.value / tickValue;

            float minTickNumber;
            float maxTickNumber;
            if (currentTickIndex % 1 == 0)
            {
                minTickNumber = currentTickIndex - 1;
                maxTickNumber = currentTickIndex + 1;
            }
            else
            {
                minTickNumber = Mathf.Floor(currentTickIndex);
                maxTickNumber = minTickNumber + 1;
            }

            _minTickValue = tickValue * minTickNumber;
            _maxTickValue = tickValue * maxTickNumber;
        }

        private void SliderMove(float normValue)
        {
            if (_enableHaptics)
            {
                if (_endItemSlider.value >= _maxTickValue || _endItemSlider.value <= _minTickValue)
                {
                    SetCurrentHapticTick();
                    _vibrationOnActivation.TriggerOnce();
                }
            }
        }
    }
}
