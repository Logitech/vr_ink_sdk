/* Copyright (c) Logitech Corporation. All rights reserved. Licensed under the MIT License.*/

namespace LogitechStylus.Scripts
{
    using UnityEngine;
    using System.Collections;
    using Valve.VR;

    public class LogitechStylusButtonAnimation : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField]
        private string _modelName;
        [SerializeField]
        EVRButtonId _input;
        private SteamVR_TrackedController _trackedObject;
        private SteamVR_Controller.Device _device;

        [Header("Materials")]
        [SerializeField] private Renderer _targerRenderer;
        [SerializeField] private Material _newMaterial;
        private Material _defaultMaterial;

        /// <summary>
        ///     This is a coroutine as the SteamVR api takes a couple of frames to get the proper device index for
        ///     a defined SteamVR_TrackedController
        /// </summary>
        private IEnumerator Start()
        {
            yield return new WaitForSeconds(0.5f);

            int deviceIndex = LogitechStylusDetection.GetLogitechStylusIndex(_modelName);
            _device = SteamVR_Controller.Input(deviceIndex);

            yield return null;
        }

        void Update()
        {
            if (_device == null)
            {
                return;
            }

            if (_device.GetPressDown(_input))
            {
                Material[] mats = _targerRenderer.sharedMaterials;
                _defaultMaterial = mats[0];
                mats[0] = _newMaterial;
                _targerRenderer.sharedMaterials = mats;
            }

            if (_device.GetPressUp(_input))
            {
                Material[] mats = _targerRenderer.sharedMaterials;
                mats[0] = _defaultMaterial;
                _targerRenderer.sharedMaterials = mats;
            }
        }
    }
}
