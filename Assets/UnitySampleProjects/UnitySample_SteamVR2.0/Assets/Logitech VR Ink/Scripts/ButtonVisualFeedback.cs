/* Copyright (c) Logitech Corporation. All rights reserved. Licensed under the MIT License.*/

namespace Logitech.Scripts
{
    using UnityEngine;
    using Valve.VR;

    /// <summary>
    /// Update the colour of a Stylus button when it is pressed.
    /// </summary>
    public class ButtonVisualFeedback : MonoBehaviour
    {
        [Header("Input")]
        public bool GetInputSourceFromStylusDetection = true;
        [Tooltip("If not using UseStylusDetection, set the SteamVR input source manually")]
        public SteamVR_Input_Sources ManualSteamVRInputSource;
        [SerializeField]
        private SteamVR_Action_Boolean _input;

        [Header("Materials")]
        [SerializeField]
        private Renderer _targetRenderer;
        [SerializeField]
        private Material _newMaterial;
        private Material _defaultMaterial;

        private void Update()
        {
            SteamVR_Input_Sources inputSource = GetInputSourceFromStylusDetection
                ? PrimaryDeviceDetection.PrimaryDeviceBehaviourPose.inputSource
                : ManualSteamVRInputSource;

            if (_input.GetStateDown(inputSource))
            {
                var mats = _targetRenderer.sharedMaterials;
                _defaultMaterial = mats[0];
                mats[0] = _newMaterial;
                _targetRenderer.sharedMaterials = mats;
            }

            if (_input.GetStateUp(inputSource))
            {
                var mats = _targetRenderer.sharedMaterials;
                mats[0] = _defaultMaterial;
                _targetRenderer.sharedMaterials = mats;
            }
        }
    }
}
