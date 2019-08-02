namespace Logitech.XRToolkit.Interactions
{
    using Logitech.XRToolkit.Providers;
    using Logitech.XRToolkit.Utils;
    using UnityEngine;

    /// <summary>
    /// Visually shows the touch position of a TrackedDevice trackpad.
    /// </summary>
    public class TouchPositionFeedback : MonoBehaviour
    {
        [SerializeField]
        private TrackedDeviceProvider _trackedDeviceProvider;
        [SerializeField]
        private Transform _touchRepresentation;

        private void Update()
        {
            if (_trackedDeviceProvider.GetOutput().GetButtonDown(StylusButton.TouchstripTouch))
            {
                _touchRepresentation.gameObject.SetActive(true);
            }

            if (_trackedDeviceProvider.GetOutput().GetButton(StylusButton.TouchstripTouch))
            {
                Vector2 touchPosition;
                touchPosition.x = _trackedDeviceProvider.GetOutput().GetAxis(StylusAxisInput.TrackpadX) / 2;
                touchPosition.y = _trackedDeviceProvider.GetOutput().GetAxis(StylusAxisInput.TrackpadY) / 2;
                _touchRepresentation.localPosition = touchPosition;
            }

            if (_trackedDeviceProvider.GetOutput().GetButtonUp(StylusButton.TouchstripTouch))
            {
                _touchRepresentation.gameObject.SetActive(false);
            }
        }
    }
}
