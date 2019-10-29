/* Copyright (c) Logitech Corporation. All rights reserved. Licensed under the MIT License.*/

namespace Logitech.Scripts
{
    using UnityEngine;
    using Valve.VR;

    /// <summary>
    /// Updates a MeshRenderer that has a circle shader material to indicate pressure on the primary button of a Stylus.
    /// </summary>
    public class StylusPrimaryVisualFeedback : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField]
        EVRButtonId _input = EVRButtonId.k_EButton_SteamVR_Trigger;

        [Header("Visual Feedback")]
        [SerializeField]
        private MeshRenderer _circleIndicatorRenderer;
        [SerializeField]
        private AnimationCurve _mappedCurve;

        void Update()
        {
            if (PrimaryDeviceDetection.PrimaryIndex < 0)
            {
                return;
            }

            float mappedPressureValue = PrimaryDeviceDetection.GetPrimaryInput().GetAxis(_input).x;
            if (mappedPressureValue > 0)
            {
                mappedPressureValue = _mappedCurve.Evaluate(mappedPressureValue);
            }
            _circleIndicatorRenderer.sharedMaterial.SetFloat("_BackgroundCutoff", 1 - mappedPressureValue);
        }
    }
}
