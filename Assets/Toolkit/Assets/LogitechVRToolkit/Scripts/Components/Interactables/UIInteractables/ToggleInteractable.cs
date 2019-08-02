namespace Logitech.XRToolkit.Components
{
    using Logitech.XRToolkit.Actions;
    using Logitech.XRToolkit.Utils;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Attached to a collider of a Toggle. Receives hover and button events which allows physical and raycast button
    /// interaction.
    /// </summary>
    public class ToggleInteractable : UIInteractable
    {
        [Space(10)]
        [SerializeField]
        private Toggle _toggle;

        [Header("Haptics"), SerializeField]
        private bool _enableHaptics = true;
        [SerializeField, ShowIf("_enableHaptics")]
        private HapticAction _vibrationOnActivation;

        void Reset()
        {
            _toggle = GetComponent<Toggle>();
            Tags = EInteractable.UIInteractable;
        }

        private void Awake()
        {
            UIElement = _toggle;
            OnPhysicalPressDown += PhysicallyPressButton;
        }

        private void PhysicallyPressButton()
        {
            if (_toggle != null)
            {
                _toggle.isOn = !_toggle.isOn;
                if (_enableHaptics)
                {
                    _vibrationOnActivation.TriggerOnce();
                }
            }
        }

        protected override void OnButtonDownEvent()
        {
            if (_toggle != null)
            {
                _toggle.isOn = !_toggle.isOn;
                if (_enableHaptics)
                {
                    _vibrationOnActivation.TriggerOnce();
                }
            }
        }
    }
}
