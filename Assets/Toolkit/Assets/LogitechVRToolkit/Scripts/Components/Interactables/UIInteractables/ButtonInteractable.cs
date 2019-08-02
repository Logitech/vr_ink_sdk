namespace Logitech.XRToolkit.Components
{
    using Logitech.XRToolkit.Actions;
    using Logitech.XRToolkit.Utils;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Attached to a collider of a Button. Receives hover and button events which allows physical and raycast button
    /// interaction.
    /// </summary>
    public class ButtonInteractable : UIInteractable
    {
        [Space(10)]
        [SerializeField]
        private Button _button;

        [Header("Haptics"), SerializeField]
        private bool _enableHaptics = true;
        [SerializeField, ShowIf("_enableHaptics")]
        private HapticAction _vibrationOnActivation;

        void Reset()
        {
            _button = GetComponent<Button>();
            Tags = EInteractable.UIInteractable;
        }

        private void Awake()
        {
            UIElement = _button;
            OnPhysicalPressDown += PhysicallyPressButton;
        }

        private void PhysicallyPressButton()
        {
            if (_button != null)
            {
                _button.onClick.Invoke();
                if (_enableHaptics)
                {
                    _vibrationOnActivation.TriggerOnce();
                }
            }
        }

        protected override void OnButtonDownEvent()
        {
            if (_button != null)
            {
                _button.onClick.Invoke();
                if (_enableHaptics)
                {
                    _vibrationOnActivation.TriggerOnce();
                }
            }
        }
    }
}
