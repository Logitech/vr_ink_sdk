/* Copyright (c) Logitech Corporation. All rights reserved. Licensed under the MIT License.*/

namespace LogitechStylus.Scripts
{
    using UnityEngine;
    using Valve.VR;

    /// <summary>
    /// Visually shows the touch position of a TrackedDevice trackpad.
    /// </summary>
    public class TouchPositionFeedback : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField]
        private SteamVR_Action_Boolean _touchInput;
        [SerializeField]
        private SteamVR_Action_Vector2 _touchPosition;

        [SerializeField]
        private Transform _touchRepresentation;

        private void Update()
        {
            if (_touchInput.GetStateDown(LogitechStylusDetection.Instance.VRInkInputSource))
            {
                _touchRepresentation.gameObject.SetActive(true);
            }

            if (_touchInput.GetState(LogitechStylusDetection.Instance.VRInkInputSource))
            {
                Vector2 touchPosition = _touchPosition.axis / 2;
                _touchRepresentation.localPosition = touchPosition;
            }

            if (_touchInput.GetStateUp(LogitechStylusDetection.Instance.VRInkInputSource))
            {
                _touchRepresentation.gameObject.SetActive(false);
            }
        }
    }
}
