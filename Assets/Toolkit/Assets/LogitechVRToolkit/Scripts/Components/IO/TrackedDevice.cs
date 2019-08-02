namespace Logitech.XRToolkit.IO
{
    using Logitech.XRToolkit.Utils;
    using UnityEngine;
#if STEAMVR_ENABLED
    using Valve.VR;
#endif

    /// <summary>
    /// Abstracts a device, using an implementation of <see cref="IIOTrackedDevice"/> to position the GameObject it is
    /// on according to the pose of its target tracked device.
    /// </summary>
    /// <seealso cref="TrackedDevice_Unity"/>
    /// <seealso cref="TrackedDevice_SteamVR"/>
    /// <seealso cref="TrackedDevice_Trackpad"/>
    [RequireComponent(typeof(TrackedDevice_Trackpad))]
    public class TrackedDevice : MonoBehaviour, IIOHandler, ITouchEvents
    {
        public Handedness Handedness;
        private TrackedDevice_Trackpad _trackpad;

        public IIOTrackedDevice Controller { get; private set; }

        public float GetAxis(StylusAxisInput stylusAxisInput)
        {
            return Controller.GetAxis(stylusAxisInput);
        }

        public bool GetButton(StylusButton stylusButton)
        {
            return Controller.GetButton(stylusButton);
        }

        public bool GetButtonDown(StylusButton stylusButton)
        {
            return Controller.GetButtonDown(stylusButton);
        }

        public bool GetButtonUp(StylusButton stylusButton)
        {
            return Controller.GetButtonUp(stylusButton);
        }

        public void SendHapticPulse(float delayInSeconds, float durationInSeconds, float frequency, float amplitude)
        {
            Controller.SendHapticPulse(delayInSeconds, durationInSeconds, frequency, amplitude);
        }

#region TouchInput

        public bool OnTap()
        {
            return _trackpad.OnTap();
        }

        public bool OnDoubleTap()
        {
            return _trackpad.OnDoubleTap();
        }

        public bool IsInZone(ETouchZone touchZone)
        {
            return _trackpad.IsInZone(touchZone);
        }

        public bool TouchRemainedInZone(bool requireAllTrue, params ETouchZone[] zones)
        {
            return _trackpad.TouchRemainedInZone(requireAllTrue, zones);
        }

        public bool OnSwipe(ESwipeDirection swipeDirection)
        {
            return _trackpad.OnSwipe(swipeDirection);
        }

        public ESwipeDirection OnSwipe()
        {
            return _trackpad.OnSwipe();
        }

        public Vector2 Scroll()
        {
            return _trackpad.Scroll();
        }

#endregion

        private void Awake()
        {
            var trackingAbstraction = LogitechToolkitManager.Instance.GetTrackingAbstraction();
            switch (trackingAbstraction)
            {
                case ETrackingAbstraction.NativeUnity:
                    Controller = new TrackedDevice_Unity();
                    Controller.StylusHand = Handedness;
                    break;
                case ETrackingAbstraction.SteamVR:
#if STEAMVR_ENABLED
                    SteamVR_Behaviour_Pose steamComponent = GetComponent<SteamVR_Behaviour_Pose>();
                    if (steamComponent == null)
                    {
                        Debug.LogErrorFormat("For SteamVR abstraction please place this script on Controller (left) and Controller (right) alongside SteamVR_Behaviour_Pose.", trackingAbstraction);
                    }
                    Controller = new TrackedDevice_SteamVR(Handedness, transform, steamComponent);

                    // Initialize device index.
                    SetDeviceIndex();
#endif
                    break;
                default:
                    Debug.LogErrorFormat("The requested tracking abstraction {0} is not supported.", trackingAbstraction);
                    break;
            }

            LogitechToolkitManager.Instance.Devices[Handedness] = this;
            _trackpad = GetComponent<TrackedDevice_Trackpad>();
        }

        private void LateUpdate()
        {
            if (LogitechToolkitManager.Instance.GetTrackingAbstraction() == ETrackingAbstraction.SteamVR)
            {
                return;
            }

            Debug.Assert(Controller != null);
            var devicePose = Controller.GetDevicePose();
            transform.localPosition = devicePose.position;
            transform.localRotation = devicePose.rotation;
        }

        private void OnDestroy()
        {
            Controller.OnDestroy();
        }

        // SetDeviceIndex is a listener of a broadcast message sent by SteamVR_Behaviour_Pose when the Device ID changes.
        public void SetDeviceIndex()
        {
            LogitechToolkitManager.Instance.AssignStylusToPrimary();
        }
    }
}
