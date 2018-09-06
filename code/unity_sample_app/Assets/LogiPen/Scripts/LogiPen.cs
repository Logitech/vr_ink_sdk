/* Copyright (c) Logitech Corporation. All rights reserved. Licensed under the MIT License.*/

namespace LogiPen.Scripts
{
    using UnityEngine;

    /// <summary>
    /// Hold intrinsic information about the pen that can be  used to drive the 3d model
    /// and interactions within the app
    /// </summary>
    public class LogiPen : Singleton<LogiPen> {

        public const float LineMaximumWidth = 0.02f;
        public const float LineMinimumWidth = 0.001f;
        
        [Header("Pen Setup")]
        [SerializeField]private SteamVR_TrackedController _controller;
        public SteamVR_TrackedController Controller
        {
            get { return _controller; }
            set { _controller = value; }
        }

        public Renderer PenColorRenderer;
    

        [Header("Pen Attributes")]
        [SerializeField]private float _lineWidth = 0.005f;
        [SerializeField]private Material _penColorMaterial;

        private void Start()
        {
            _penColorMaterial = PenColorRenderer.material;
        }

        /// <summary>
        /// Get color from the pen. You could also get the Renderer troughh the singleton if needed
        /// </summary>
        /// <returns>Pen Color</returns>
        public Color GetPenColor()
        {
            return _penColorMaterial.color;
        }
    
        /// <summary>
        /// Set the pen model to follow the SteamVR trackked controller
        /// </summary>
        private void LateUpdate()
        {
            transform.rotation = _controller.transform.rotation;
            transform.position = _controller.transform.position;
        }
      
    }
}
