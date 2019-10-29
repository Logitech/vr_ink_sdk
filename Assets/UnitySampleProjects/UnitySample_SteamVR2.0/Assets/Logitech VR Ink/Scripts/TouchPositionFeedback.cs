/* Copyright (c) Logitech Corporation. All rights reserved. Licensed under the MIT License.*/

namespace Logitech.Scripts
{
    using UnityEngine;
    using Valve.VR;

    /// <summary>
    /// Visually shows the touch position of a TrackedDevice trackpad.
    /// </summary>
    public class TouchPositionFeedback : MonoBehaviour
    {
        [Header("Input")]
        public bool GetInputSourceFromStylusDetection = true;
        [Tooltip("If not using UseStylusDetection, set the SteamVR input source manually")]
        public SteamVR_Input_Sources ManualSteamVRInputSource;
        [SerializeField]
        private SteamVR_Action_Boolean _touchInput;
        [SerializeField]
        private SteamVR_Action_Vector2 _touchPosition;

        [SerializeField]
        private Transform _touchRepresentation;

        private void Update()
        {
            SteamVR_Input_Sources inputSource = GetInputSourceFromStylusDetection
                ? PrimaryDeviceDetection.PrimaryDeviceBehaviourPose.inputSource
                : ManualSteamVRInputSource;

            if (_touchInput.GetStateDown(inputSource))
            {
                _touchRepresentation.gameObject.SetActive(true);
            }

            if (_touchInput.GetState(inputSource))
            {
                Vector2 touchPosition = _touchPosition.axis / 2;
                _touchRepresentation.localPosition = touchPosition;
            }

            if (_touchInput.GetStateUp(inputSource))
            {
                _touchRepresentation.gameObject.SetActive(false);
            }
        }
    }
}
