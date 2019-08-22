/* Copyright (c) Logitech Corporation. All rights reserved. Licensed under the MIT License.*/

namespace LogitechStylus.Scripts
{
    using UnityEngine;
    using Valve.VR;

    /// <summary>
    /// Updates a MeshRenderer that has a circle shader material to indicate pressure on the primary button of a Stylus.
    /// </summary>
    public class StylusPrimaryVisualFeedback : MonoBehaviour
    {
        [Header("Input")]
        public bool UseStylusDetection = true;
        [Tooltip("If not using UseStylusDetection, set the SteamVR input source manually")]
        public SteamVR_Input_Sources SteamVRInputSource;
        [SerializeField]
        private SteamVR_Action_Single _input;

        [SerializeField]
        private MeshRenderer _circleShaderMaterialMeshRenderer;
        [SerializeField]
        private AnimationCurve _mappedCurve;

        private void Update()
        {
            SteamVR_Input_Sources inputSource = UseStylusDetection ? LogitechStylusDetection.Instance.VRInkInputSource : SteamVRInputSource;

            float mappedPressureValue = _input.GetAxis(inputSource);
            if (mappedPressureValue > 0)
            {
                mappedPressureValue = _mappedCurve.Evaluate(mappedPressureValue);
            }
            _circleShaderMaterialMeshRenderer.sharedMaterial.SetFloat("_BackgroundCutoff", 1 - mappedPressureValue);
        }
    }
}
