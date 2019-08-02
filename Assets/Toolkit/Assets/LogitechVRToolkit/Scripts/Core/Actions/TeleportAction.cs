namespace Logitech.XRToolkit.Actions
{
    using Action = Logitech.XRToolkit.Core.Action;
    using Logitech.XRToolkit.Components;
    using System;
    using UnityEngine;

    /// <summary>
    /// Teleport the parent GameObject of the camera. Any gameObject that is physically linked to the player should be
    /// children of this GameObject.
    /// </summary
    [Serializable]
    public class TeleportAction : Action
    {
        public Transform CameraParentTransform;

        [HideInInspector]
        public TeleportBeam TeleportBeam;

        // TODO: Implement usage
        private bool _isTeleportValid;

        /// <summary>
        /// Update the teleport area and beam
        /// </summary>
        protected override void TriggerValid()
        {
            Teleport();
        }

        private void Teleport()
        {
            var teleportPosition = TeleportBeam.GetBeamHitPoint();
            var mainCamera = CameraParentTransform.GetComponentInChildren<Camera>();
            Debug.Assert(mainCamera != null, "No camera is child of " + CameraParentTransform.name);

            Vector3 offset = CameraParentTransform.position - mainCamera.transform.position;

            CameraParentTransform.position = new Vector3(teleportPosition.x + offset.x, CameraParentTransform.position.y, teleportPosition.z + offset.z);
        }
    }
}
