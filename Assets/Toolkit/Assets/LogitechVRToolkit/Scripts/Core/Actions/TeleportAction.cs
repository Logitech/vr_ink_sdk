namespace Logitech.XRToolkit.Actions
{
    using System;

    using UnityEngine;

    using Action = Logitech.XRToolkit.Core.Action;

    /// <summary>
    /// Teleport the parent GameObject of the camera. Any gameObject that is physically linked to the player should be
    /// children of this GameObject.
    /// </summary
    [Serializable]
    public class TeleportAction : Action
    {
        public Transform CameraParentTransform;

        // TODO: Implement usage
        private bool _isTeleportValid;

        private Vector3 _landingLocation = Vector3.zero;

        /// <summary>
        /// Update the teleport area and beam
        /// </summary>
        protected override void TriggerValid()
        {
            Teleport();
        }

        private void Teleport()
        {
            var mainCamera = CameraParentTransform.GetComponentInChildren<Camera>();
            Debug.Assert(mainCamera != null, "No camera is child of " + CameraParentTransform.name);

            Vector3 offset = CameraParentTransform.position - mainCamera.transform.position;

            CameraParentTransform.position = new Vector3(_landingLocation.x + offset.x, CameraParentTransform.position.y, _landingLocation.z + offset.z);
        }

        public void UpdateLocation(Vector3 teleportLocation)
        {
            _landingLocation = teleportLocation;
        }
    }
}
