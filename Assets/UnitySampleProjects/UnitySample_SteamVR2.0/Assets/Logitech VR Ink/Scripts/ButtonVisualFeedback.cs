/* Copyright (c) Logitech Corporation. All rights reserved. Licensed under the MIT License.*/

namespace LogitechStylus.Scripts
{
    using UnityEngine;
    using Valve.VR;

    /// <summary>
    /// Update the colour of a Stylus button when it is pressed.
    /// </summary>
    public class ButtonVisualFeedback : MonoBehaviour
    {
        [Header("Input")]
        public bool UseStylusDetection = true;
        [Tooltip("If not using UseStylusDetection, set the SteamVR input source manually")]
        public SteamVR_Input_Sources SteamVRInputSource;
        [SerializeField]
        private SteamVR_Action_Boolean _input;

        [Header("Materials")]
        [SerializeField]
        private Renderer _targerRenderer;
        [SerializeField]
        private Material _newMaterial;
        private Material _defaultMaterial;

        private void Update()
        {
            SteamVR_Input_Sources inputSource = UseStylusDetection ? LogitechStylusDetection.Instance.VRInkInputSource : SteamVRInputSource;

            if (_input.GetStateDown(inputSource))
            {
                var mats = _targerRenderer.sharedMaterials;
                _defaultMaterial = mats[0];
                mats[0] = _newMaterial;
                _targerRenderer.sharedMaterials = mats;
            }

            if (_input.GetStateUp(inputSource))
            {
                var mats = _targerRenderer.sharedMaterials;
                mats[0] = _defaultMaterial;
                _targerRenderer.sharedMaterials = mats;
            }
        }
    }
}
