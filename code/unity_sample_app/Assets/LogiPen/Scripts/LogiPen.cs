/* Copyright (c) Logitech Corporation. All rights reserved. Licensed under the MIT License.*/

namespace LogiPen.Scripts
{
    using UnityEngine;

    /// <summary>
    ///     Hold intrinsic information about the pen that can be  used to drive the 3d model
    ///     and interactions within the app
    /// </summary>
    public class LogiPen : Singleton<LogiPen>
    {
        public const float LineMaximumWidth = 0.02f;
        public const float LineMinimumWidth = 0.001f;

        [Header("Pen Setup")] [SerializeField] private SteamVR_TrackedObject _trackedDevice;

        [Header("Pen Attributes")] [SerializeField] private float _lineWidth = 0.005f;

        [SerializeField] private Material _penColorMaterial;

        public Renderer PenColorRenderer;

        public SteamVR_TrackedController Controller { get; set; }

        private void Start()
        {
            _penColorMaterial = PenColorRenderer.material;

            if (_trackedDevice == null)
            {
                Debug.LogError("Please select a steamVR tracked object that you want the pen to follow");
                return;
            }

            if (_trackedDevice.GetComponent<SteamVR_TrackedController>() == null)
            {
                _trackedDevice.gameObject.AddComponent<SteamVR_TrackedController>();
                Controller = _trackedDevice.GetComponent<SteamVR_TrackedController>();
            }
            else
            {
                Controller = _trackedDevice.GetComponent<SteamVR_TrackedController>();
            }
        }

        /// <summary>
        ///     Get color from the pen. You could also get the Renderer trough the singleton if needed
        /// </summary>
        /// <returns>Pen Color</returns>
        public Color GetPenColor()
        {
            return _penColorMaterial.color;
        }

        /// <summary>
        ///     Set the pen model to follow the SteamVR tracked controller
        /// </summary>
        private void LateUpdate()
        {
            if (_trackedDevice == null)
            {
                Debug.LogError("Please select a steamVR tracked object that you want the pen to follow");
                return;
            }

            transform.rotation = Controller.transform.rotation;
            transform.position = Controller.transform.position;
        }
    }
}