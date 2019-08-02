namespace Logitech.XRToolkit.Interactions
{
    using Logitech.XRToolkit.Providers;
    using Logitech.XRToolkit.Triggers;
    using Logitech.XRToolkit.Utils;
    using UnityEngine;

    /// <summary>
    /// Update the colour of a Stylus button when it is pressed.
    /// </summary>
    public class ButtonVisualFeedback : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField]
        private TrackedDeviceProvider _trackedDeviceProvider;
        [SerializeField]
        private StylusButton _stylusButton;
        private InputTrigger _buttonTrigger;
        private bool _pressedState;

        [Header("Materials")]
        [SerializeField]
        private Renderer _targerRenderer;
        [SerializeField]
        private Material _newMaterial;
        private Material _defaultMaterial;

        private void Start()
        {
            _buttonTrigger = new InputTrigger(_trackedDeviceProvider, _stylusButton, EButtonEvent.OnButton);
        }

        private void Update()
        {
            if (_buttonTrigger.IsValid() && !_pressedState)
            {
                var mats = _targerRenderer.sharedMaterials;
                _defaultMaterial = mats[0];
                mats[0] = _newMaterial;
                _targerRenderer.sharedMaterials = mats;
                _pressedState = true;
            }

            if (!_buttonTrigger.IsValid() && _pressedState)
            {
                var mats = _targerRenderer.sharedMaterials;
                mats[0] = _defaultMaterial;
                _targerRenderer.sharedMaterials = mats;
                _pressedState = false;
            }
        }

        private void OnValidate()
        {
            _buttonTrigger = new InputTrigger(_trackedDeviceProvider, _stylusButton, EButtonEvent.OnButton);
        }
    }
}
