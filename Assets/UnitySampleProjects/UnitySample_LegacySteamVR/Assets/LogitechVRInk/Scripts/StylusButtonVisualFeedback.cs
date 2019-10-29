/* Copyright (c) Logitech Corporation. All rights reserved. Licensed under the MIT License.*/

namespace Logitech.Scripts
{
    using UnityEngine;
    using Valve.VR;

    /// <summary>
    /// Update the colour of a Stylus button when it is pressed.
    /// </summary>
    public class StylusButtonVisualFeedback : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField]
        EVRButtonId _input;

        [Header("Materials")]
        [SerializeField]
        private Renderer _targetRenderer;
        [SerializeField]
        private Material _newMaterial;
        private Material _defaultMaterial;

        void Update()
        {
            if (PrimaryDeviceDetection.PrimaryIndex < 0)
            {
                return;
            }

            if (PrimaryDeviceDetection.GetPrimaryInput().GetPressDown(_input))
            {
                Material[] mats = _targetRenderer.sharedMaterials;
                _defaultMaterial = mats[0];
                mats[0] = _newMaterial;
                _targetRenderer.sharedMaterials = mats;
            }

            if (PrimaryDeviceDetection.GetPrimaryInput().GetPressUp(_input))
            {
                Material[] mats = _targetRenderer.sharedMaterials;
                mats[0] = _defaultMaterial;
                _targetRenderer.sharedMaterials = mats;
            }
        }
    }
}
