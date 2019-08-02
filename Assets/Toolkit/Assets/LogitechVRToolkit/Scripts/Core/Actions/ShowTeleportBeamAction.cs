﻿namespace Logitech.XRToolkit.Actions
{
    using Action = Logitech.XRToolkit.Core.Action;
    using Logitech.XRToolkit.Components;
    using Logitech.XRToolkit.Providers;
    using System;
    using UnityEngine;

    /// <summary>
    /// Create and show a teleportation beam.
    /// </summary
    [Serializable]
    public class ShowTeleportBeamAction : Action
    {
        [HideInInspector]
        public TrackedDeviceProvider TrackedDevice;
        [SerializeField]
        private TeleportBeamType _teleportBeamType;
        [SerializeField]
        private float _beamLength;

        [HideInInspector]
        public TeleportBeam TeleportBeam;

        // TODO: Implement usage
        private bool _isTeleportValid;

        /// <summary>
        /// Create and display the teleport area and beam.
        /// </summary>
        protected override void OnTriggerValid()
        {
            if (TeleportBeam == null)
            {
                TeleportBeam = TrackedDevice.GetOutput().gameObject.AddComponent<TeleportBeam>();
            }

            TeleportBeam.TeleportEnabled = true;
        }

        protected override void TriggerValid()
        {
            UpdateTeleportBeam();
        }

        /// <summary>
        /// Hide the teleport beam.
        /// </summary>
        protected override void OnTriggerInvalid()
        {
            TeleportBeam.TeleportEnabled = false;
        }

        /// <summary>
        /// Update the teleport beam values.
        /// </summary>
        private void UpdateTeleportBeam()
        {
            TeleportBeam.TeleportBeamType = _teleportBeamType;
            TeleportBeam.ArcRadius = _beamLength;
        }
    }
}
