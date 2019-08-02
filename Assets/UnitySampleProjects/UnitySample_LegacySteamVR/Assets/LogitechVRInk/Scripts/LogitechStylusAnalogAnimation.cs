/* Copyright (c) Logitech Corporation. All rights reserved. Licensed under the MIT License.*/

namespace LogitechStylus.Scripts
{
    using UnityEngine;
    using System.Collections;
    using Valve.VR;

    public class LogitechStylusAnalogAnimation : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField]
        private string _modelName;
        [SerializeField]
        EVRButtonId _input = EVRButtonId.k_EButton_SteamVR_Trigger;
        private SteamVR_TrackedController _trackedObject;
        private SteamVR_Controller.Device _device;

        [Header("Visual Feedback")]
        [SerializeField]
        private MeshRenderer _circleIndicatorRenderer;

        /// <summary>
        ///     This is a coroutine as the SteamVR api takes a couple of frames to get the proper device index for
        ///     a defined SteamVR_TrackedController
        /// </summary>
        private IEnumerator Start()
        {
            yield return new WaitForSeconds(0.5f);

            int deviceIndex = LogitechStylusDetection.GetLogitechStylusIndex(_modelName, false);
            _device = SteamVR_Controller.Input(deviceIndex);

            yield return null;
        }

        void Update()
        {
            if (_device == null)
            {
                return;
            }
            _circleIndicatorRenderer.material.SetFloat("_BackgroundCutoff", 1 - _device.GetAxis(_input).x);
        }
    }
}
