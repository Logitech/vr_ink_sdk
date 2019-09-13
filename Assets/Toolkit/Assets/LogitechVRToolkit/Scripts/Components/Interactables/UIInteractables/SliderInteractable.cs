namespace Logitech.XRToolkit.Components
{
    using Logitech.XRToolkit.Actions;
    using Logitech.XRToolkit.Utils;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Attached to a child collider of a slider that acts as the selectable bounds. Receives hover and button events
    /// which allows physical and raycast button interaction.
    /// TODO Make it possible to place this beside the slider component itself.
    /// </summary>
    public class SliderInteractable : UIInteractable
    {
        [Space(10)]
        [SerializeField]
        private Slider _slider;

        [Header("Haptics"), SerializeField]
        private bool _enableHaptics = true;
        [SerializeField, ShowIf("_enableHaptics")]
        private HapticAction _vibrationOnActivation;
        [Tooltip("The rate at which haptic ticks will occur, from every 0% - every 100%.")]
        [SerializeField, Range(0, 100), ShowIf("_enableHaptics")]
        private float _hapticTickRate;

        private float _minTickValue;
        private float _maxTickValue;
        private bool _firstPress = true;

        void Reset()
        {
            _slider = GetComponentInParent<Slider>();
            Tags = EInteractable.UIInteractable;
        }

        private void Awake()
        {
            UIElement = _slider;
            OnPhysicalPressDown += () => _vibrationOnActivation.TriggerOnce();
        }

        protected override void OnHoverEvent(RaycastHit hit, Transform pushPoint, bool isCollding)
        {
            if (!OnPhysicalPress)
            {
                return;
            }

            if (_slider != null)
            {
                SliderMove(hit.textureCoord);
            }
        }

        protected override void OnButtonDownEvent()
        {
            if (_enableHaptics)
            {
                if (_firstPress)
                {
                    SetCurrentHapticTick();
                    _firstPress = false;
                }
                _vibrationOnActivation.TriggerOnce();
            }
        }

        protected override void OnButtonEvent(RaycastHit hit)
        {
            if (_slider != null)
            {
                SliderMove(hit.textureCoord);
            }
        }

        private void SetCurrentHapticTick()
        {
            Debug.Assert(_hapticTickRate <= 100, "Haptic tick rate is larger than 100% and therefore may never trigger!");

            var tickValue = _hapticTickRate * (_slider.maxValue / 100);
            var currentTickIndex = _slider.value / tickValue;

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

        private void SliderMove(Vector2 hitCoord)
        {
            // Adjust received coordinate to slider direction.
            float normValue;
            switch (_slider.direction)
            {
                case Slider.Direction.BottomToTop:
                    normValue = hitCoord.y;
                    break;
                case Slider.Direction.LeftToRight:
                    normValue = hitCoord.x;
                    break;
                case Slider.Direction.RightToLeft:
                    normValue = 1f - hitCoord.x;
                    break;
                case Slider.Direction.TopToBottom:
                    normValue = 1f - hitCoord.y;
                    break;
                default:
                    return;
            }

            // Update the slider value.
            _slider.value = _slider.minValue + normValue * (_slider.maxValue - _slider.minValue);

            if (_enableHaptics)
            {
                if (_slider.value >= _maxTickValue || _slider.value <= _minTickValue)
                {
                    SetCurrentHapticTick();
                    _vibrationOnActivation.TriggerOnce();
                }
            }
        }

        private void OnValidate()
        {
            SetCurrentHapticTick();
        }
    }
}
