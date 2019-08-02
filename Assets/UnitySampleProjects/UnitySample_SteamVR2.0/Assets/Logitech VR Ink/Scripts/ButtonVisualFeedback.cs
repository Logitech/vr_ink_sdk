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
            if (_input.GetStateDown(LogitechStylusDetection.Instance.VRInkInputSource))
            {
                var mats = _targerRenderer.sharedMaterials;
                _defaultMaterial = mats[0];
                mats[0] = _newMaterial;
                _targerRenderer.sharedMaterials = mats;
            }

            if (_input.GetStateUp(LogitechStylusDetection.Instance.VRInkInputSource))
            {
                var mats = _targerRenderer.sharedMaterials;
                mats[0] = _defaultMaterial;
                _targerRenderer.sharedMaterials = mats;
            }
        }
    }
}
