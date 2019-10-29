/* Copyright (c) Logitech Corporation. All rights reserved. Licensed under the MIT License.*/

namespace Logitech.Scripts
{
    using UnityEngine;
    using Valve.VR;

    /// <summary>
    /// Visually shows the touch position of a TrackedDevice trackpad.
    /// </summary>
    public class TouchPositionVisualFeedback : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField]
        EVRButtonId _touchInput = EVRButtonId.k_EButton_SteamVR_Touchpad;

        [SerializeField]
        private Transform _touchRepresentation;

        private void Update()
        {
            if (PrimaryDeviceDetection.PrimaryIndex < 0)
            {
                return;
            }

            if (PrimaryDeviceDetection.GetPrimaryInput().GetTouchDown(EVRButtonId.k_EButton_SteamVR_Touchpad))
            {
                _touchRepresentation.gameObject.SetActive(true);
            }

            if (PrimaryDeviceDetection.GetPrimaryInput().GetTouch(EVRButtonId.k_EButton_SteamVR_Touchpad))
            {
                Vector2 touchPosition = PrimaryDeviceDetection.GetPrimaryInput().GetAxis(_touchInput) / 2;
                _touchRepresentation.localPosition = touchPosition;
            }

            if (PrimaryDeviceDetection.GetPrimaryInput().GetTouchUp(EVRButtonId.k_EButton_SteamVR_Touchpad))
            {
                _touchRepresentation.gameObject.SetActive(false);
            }
        }
    }
}
