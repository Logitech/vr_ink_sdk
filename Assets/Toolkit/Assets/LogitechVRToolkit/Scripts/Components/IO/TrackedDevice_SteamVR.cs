namespace Logitech.XRToolkit.IO
{
    using Logitech.XRToolkit.Utils;
    using System.Collections.Generic;
    using UnityEngine;

#if STEAMVR_ENABLED
    using Valve.VR;

    /// <summary>
    /// SteamVR abstraction of a <see cref="TrackedDevice"/> implementing <see cref="IIOTrackedDevice"/>.
    /// TODO Note that the touchstrip rejection portion of this class is legacy code and will likely be removed at a later date.
    /// </summary>
    public class TrackedDevice_SteamVR : IIOTrackedDevice
    {
        public Handedness StylusHand
        {
            get; set;
        }

        public Utils.DeviceType Type
        {
            get; set;
        }

        public SteamVR_Input_Sources InputSource;
        private Transform _steamVRPose;
        private SteamVR_Behaviour_Pose _steamVRBehaviourPose;
        public int DeviceIndex
        {
            get { return _steamVRBehaviourPose.GetDeviceIndex(); }
        }

        private SteamVR_Action_Vector2 _touchpadValue;
        private readonly Dictionary<StylusButton, SteamVR_Action_Boolean> _stylusButtonsToSteamVrMap;
        private readonly Dictionary<TrackedDeviceButton, SteamVR_Action_Boolean> _trackedDeviceButtonsToSteamVrMap;
        private readonly Dictionary<StylusAxisInput, SteamVR_Action_Single> _stylusAxisToSteamVrMap;
        private readonly Dictionary<TrackedDeviceAxisInput, SteamVR_Action_Single> _trackedDeviceAxisToSteamVrMap;


        // Debug touchstrip rejection.
        private EButtonRejectionState _rejectionState = EButtonRejectionState.Neither;
        private const float TSDelay = 0.3f;
        private const float TSAfterDelay = 1f;
        private const float TSLockDelay = 0.5f;
        private const float RestingValue = 0.001f;
        private float _delayTimer = 0f;
        private float _touchStripTimer = 0f;
        private float _delayLockTimer = 0f;
        private bool _isDelayStateEnbable = false;
        private bool _touchStripRejection = false;
        public bool TouchStripRejection
        {
            get { return _touchStripRejection; }
            set { _touchStripRejection = value; }
        }

        public TrackedDevice_SteamVR(Handedness hand, Transform transform, SteamVR_Behaviour_Pose SteamVRBehaviourPose)
        {
            _steamVRBehaviourPose = SteamVRBehaviourPose;
            InputSource = SteamVRBehaviourPose.inputSource;
            _steamVRPose = transform;
            StylusHand = hand;

            _touchpadValue = SteamVR_Actions.logitechToolkit_TrackpadValue;

            _stylusButtonsToSteamVrMap = new Dictionary<StylusButton, SteamVR_Action_Boolean>()
            {
                { StylusButton.Primary, SteamVR_Actions.logitechToolkit_PrimaryClick },
                { StylusButton.TouchstripTouch, SteamVR_Actions.logitechToolkit_TrackpadTouch },
                { StylusButton.TouchstripClick, SteamVR_Actions.logitechToolkit_TrackpadClick },
                { StylusButton.Menu, SteamVR_Actions.logitechToolkit_MenuClick },
                { StylusButton.Grip, SteamVR_Actions.logitechToolkit_GripClick },
                { StylusButton.Tip, SteamVR_Actions.logitechToolkit_TipClick }
            };

            _stylusAxisToSteamVrMap = new Dictionary<StylusAxisInput, SteamVR_Action_Single>()
            {
                { StylusAxisInput.Primary, SteamVR_Actions.logitechToolkit_PrimaryPressure },
                { StylusAxisInput.Grip, SteamVR_Actions.logitechToolkit_GripPressure },
                { StylusAxisInput.Tip, SteamVR_Actions.logitechToolkit_TipPressure }
            };

            _trackedDeviceButtonsToSteamVrMap = new Dictionary<TrackedDeviceButton, SteamVR_Action_Boolean>()
            {
                { TrackedDeviceButton.Trigger, SteamVR_Actions.logitechToolkit_PrimaryClick },
                { TrackedDeviceButton.TrackpadTouch, SteamVR_Actions.logitechToolkit_TrackpadTouch },
                { TrackedDeviceButton.TrackpadClick, SteamVR_Actions.logitechToolkit_TrackpadClick },
                { TrackedDeviceButton.Menu, SteamVR_Actions.logitechToolkit_MenuClick },
                { TrackedDeviceButton.Grab, SteamVR_Actions.logitechToolkit_GripClick },
            };

            _trackedDeviceAxisToSteamVrMap = new Dictionary<TrackedDeviceAxisInput, SteamVR_Action_Single>()
            {
                { TrackedDeviceAxisInput.Trigger, SteamVR_Actions.logitechToolkit_PrimaryPressure },
                { TrackedDeviceAxisInput.Grab, SteamVR_Actions.logitechToolkit_GripPressure },
            };
        }

        // Stylus.
        public float GetAxis(StylusAxisInput stylusAxisInput)
        {
            SteamVR_Action_Single actionSinglePrimary;
            SteamVR_Action_Single actionSingleNib;
            if (_touchStripRejection && _stylusAxisToSteamVrMap.TryGetValue(StylusAxisInput.Primary, out actionSinglePrimary) && _stylusAxisToSteamVrMap.TryGetValue(StylusAxisInput.Tip, out actionSingleNib))
            {
                UpdateStateMachine(actionSinglePrimary.GetAxis(InputSource), actionSingleNib.GetAxis(InputSource), _touchpadValue.GetAxis(InputSource).x, _touchpadValue.GetAxis(InputSource).y);
            }

            if (stylusAxisInput == StylusAxisInput.TrackpadX && GetRejectionStatus(stylusAxisInput))
            {
                return _touchpadValue.GetAxis(InputSource).x;
            }
            if (stylusAxisInput == StylusAxisInput.TrackpadY && GetRejectionStatus(stylusAxisInput))
            {
                return _touchpadValue.GetAxis(InputSource).y;
            }

            SteamVR_Action_Single actionSingle;
            if (_stylusAxisToSteamVrMap.TryGetValue(stylusAxisInput, out actionSingle) && GetRejectionStatus(stylusAxisInput))
            {
                return actionSingle.GetAxis(InputSource);
            }
            return 0;
        }

        public bool GetButtonDown(StylusButton stylusButton)
        {
            return GetBooleanActionFromMap(stylusButton).GetStateDown(InputSource);
        }

        public bool GetButton(StylusButton stylusButton)
        {
            if (stylusButton == StylusButton.TouchstripTouch)
            {
                return GetBooleanActionFromMap(stylusButton).GetState(InputSource) && GetRejectionStatus(StylusAxisInput.TrackpadY);
            }
            return GetBooleanActionFromMap(stylusButton).GetState(InputSource);
        }

        public bool GetButtonUp(StylusButton stylusButton)
        {
            return GetBooleanActionFromMap(stylusButton).GetStateUp(InputSource);
        }

        private SteamVR_Action_Boolean GetBooleanActionFromMap(StylusButton stylusButton)
        {
            SteamVR_Action_Boolean actionBoolean;
            if (_stylusButtonsToSteamVrMap.TryGetValue(stylusButton, out actionBoolean))
            {
                return actionBoolean;
            }

            Debug.LogErrorFormat("No SteamVR action could be found for button {0}", stylusButton);
            return null;
        }

        // Other TrackedDevice.
        public float GetAxis(TrackedDeviceAxisInput trackedDeviceAxisInput)
        {
            if (trackedDeviceAxisInput == TrackedDeviceAxisInput.TrackpadX)
            {
                return _touchpadValue.GetAxis(InputSource).x;
            }
            if (trackedDeviceAxisInput == TrackedDeviceAxisInput.TrackpadY)
            {
                return _touchpadValue.GetAxis(InputSource).y;
            }

            SteamVR_Action_Single actionSingle;
            if (_trackedDeviceAxisToSteamVrMap.TryGetValue(trackedDeviceAxisInput, out actionSingle))
            {
                return actionSingle.GetAxis(InputSource);
            }
            return 0;
        }

        public bool GetButtonDown(TrackedDeviceButton trackedDeviceButton)
        {
            return GetBooleanActionFromMap(trackedDeviceButton).GetStateDown(InputSource);
        }

        public bool GetButton(TrackedDeviceButton trackedDeviceButton)
        {
            return GetBooleanActionFromMap(trackedDeviceButton).GetState(InputSource);
        }

        public bool GetButtonUp(TrackedDeviceButton trackedDeviceButton)
        {
            return GetBooleanActionFromMap(trackedDeviceButton).GetStateUp(InputSource);
        }

        private SteamVR_Action_Boolean GetBooleanActionFromMap(TrackedDeviceButton trackedDeviceButton)
        {
            SteamVR_Action_Boolean actionBoolean;
            if (_trackedDeviceButtonsToSteamVrMap.TryGetValue(trackedDeviceButton, out actionBoolean))
            {
                return actionBoolean;
            }

            Debug.LogErrorFormat("No SteamVR action could be found for button {0}", trackedDeviceButton);
            return null;
        }

        public void SendHapticPulse(float delayInSeconds, float durationInSeconds, float frequency, float amplitude)
        {
            SteamVR_Actions.logitechToolkit_Haptics.Execute(delayInSeconds, durationInSeconds, frequency, amplitude, InputSource);
        }

        public Pose GetDevicePose()
        {
            return new Pose(
                _steamVRPose.position,
                _steamVRPose.rotation
            );
        }

        public void OnDestroy() { }

        // Debug touchstrip rejection.
        private bool GetRejectionStatus(StylusAxisInput axisType)
        {
            if (!_touchStripRejection)
                return true;
            if (axisType == StylusAxisInput.Grip)
                return true;
            switch (_rejectionState)
            {
                case EButtonRejectionState.Neither:
                    return false;
                case EButtonRejectionState.Primary_Analog_Only:
                    if (axisType == StylusAxisInput.Primary || axisType == StylusAxisInput.Tip)
                        return true;
                    return false;
                case EButtonRejectionState.TouchStrip_Delay:
                    return false;
                case EButtonRejectionState.TouchStrip_Only:
                    if (axisType == StylusAxisInput.TrackpadX || axisType == StylusAxisInput.TrackpadY)
                        return true;
                    return false;
                case EButtonRejectionState.TouchStrip_Only_Lock:
                    if (axisType == StylusAxisInput.TrackpadX || axisType == StylusAxisInput.TrackpadY)
                        return true;
                    return false;
                case EButtonRejectionState.TouchStrip_After_Delay:
                    return false;
            }
            return true;
        }

        private void UpdateStateMachine(float primaryAnalog, float nibAnalog, float touchStripX, float touchStripY)
        {
            switch (_rejectionState)
            {
                case EButtonRejectionState.Neither:
                {
                    if (primaryAnalog > RestingValue || nibAnalog > 0)
                    {
                        _rejectionState = EButtonRejectionState.Primary_Analog_Only;
                    }
                    else if (touchStripX != 0 || touchStripY != 0)
                    {
                        _rejectionState = EButtonRejectionState.TouchStrip_Delay;
                        _delayTimer = 0f;
                    }

                    break;
                }
                case EButtonRejectionState.Primary_Analog_Only:
                {
                    if (primaryAnalog <= 0 && nibAnalog <= 0)
                    {
                        _rejectionState = EButtonRejectionState.Neither;
                    }

                    break;
                }
                case EButtonRejectionState.TouchStrip_Delay:
                {
                    _delayTimer += Time.deltaTime;
                    if (primaryAnalog > 0 || nibAnalog > 0 || (touchStripX == 0 && touchStripY == 0))
                    {
                        _rejectionState = EButtonRejectionState.Neither;
                    }
                    else if (_delayTimer > TSDelay)
                    {
                        _touchStripTimer = 0f;
                        _rejectionState = EButtonRejectionState.TouchStrip_Only;
                    }

                    break;
                }
                case EButtonRejectionState.TouchStrip_Only:
                {
                    if (primaryAnalog > 0 && primaryAnalog <= RestingValue || (touchStripX == 0 && touchStripY == 0))
                    {
                        _rejectionState = EButtonRejectionState.Neither;
                    }
                    else if (primaryAnalog > RestingValue || nibAnalog > 0)
                    {
                        _rejectionState = EButtonRejectionState.Primary_Analog_Only;
                    }

                    _touchStripTimer += Time.deltaTime;
                    if (_touchStripTimer >= TSAfterDelay && _isDelayStateEnbable)
                    {
                        _rejectionState = EButtonRejectionState.TouchStrip_Only_Lock;
                    }

                    break;
                }
                case EButtonRejectionState.TouchStrip_Only_Lock:
                {
                    if (_isDelayStateEnbable)
                    {
                        _rejectionState = EButtonRejectionState.TouchStrip_Only;
                        break;
                    }

                    if (primaryAnalog > 0 || nibAnalog > 0 || (touchStripX == 0 && touchStripY == 0))
                    {
                        _delayLockTimer = 0;
                        _rejectionState = EButtonRejectionState.TouchStrip_After_Delay;
                    }

                    break;
                }
                case EButtonRejectionState.TouchStrip_After_Delay:
                {
                    if (_isDelayStateEnbable)
                    {
                        _rejectionState = EButtonRejectionState.TouchStrip_Only;
                        break;
                    }

                    if (primaryAnalog > RestingValue || nibAnalog > 0)
                    {
                        _delayLockTimer += Time.deltaTime;
                        if (_delayLockTimer > TSLockDelay)
                        {
                            _rejectionState = EButtonRejectionState.Primary_Analog_Only;
                        }
                    }
                    else if (primaryAnalog > 0 && primaryAnalog <= RestingValue)
                    {
                        _delayLockTimer += Time.deltaTime;
                        if (_delayLockTimer > TSLockDelay)
                        {
                            _rejectionState = EButtonRejectionState.Neither;
                        }
                    }
                    else if (touchStripX != 0 || touchStripY != 0)
                    {
                        _rejectionState = EButtonRejectionState.TouchStrip_Only_Lock;
                    }

                    break;
                }
            }
        }
    }
#endif
}
