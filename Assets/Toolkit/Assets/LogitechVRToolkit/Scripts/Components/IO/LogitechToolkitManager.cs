namespace Logitech.XRToolkit.IO
{
    using Logitech.XRToolkit.Utils;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// TODO There is probably a way to detect which SDK is loaded in the editor and change accordingly.
    /// </summary>
    public enum ETrackingAbstraction
    {
        NativeUnity,
        SteamVR
    }

    /// <summary>
    /// Singleton manager to hold application-wide information such as which tracking abstraction is being used.
    /// This information is used, for example, by instances of TrackedDevice to define which implementation
    /// they should use to track objects.
    /// </summary>
    public class LogitechToolkitManager : SingletonBehaviour<LogitechToolkitManager>
    {
        public Dictionary<Handedness, TrackedDevice> Devices = new Dictionary<Handedness, TrackedDevice>();
        [SerializeField]
        private ETrackingAbstraction _trackingAbstraction;
        [SerializeField, ShowIf("_trackingAbstraction", ETrackingAbstraction.SteamVR)]
        private LogitechStylusDetection _stylusDetection;

        /// <summary>
        /// Swaps the left and right hand controllers.
        /// </summary>
        public void SwapHandedness()
        {
            var primary = Devices[Handedness.Primary];
            Devices[Handedness.Primary] = Devices[Handedness.NonDominant];
            Devices[Handedness.NonDominant] = primary;
        }

        /// <summary>
        /// This should be set to the SDK that you are currently using in PlayerSettings->XR.
        /// </summary>
        /// <returns>
        /// Current tracking abstraction loaded by unity.
        /// </returns>
        public ETrackingAbstraction GetTrackingAbstraction()
        {
            return _trackingAbstraction;
        }

        public void AssignStylusToPrimary()
        {
#if STEAMVR_ENABLED
            if (_stylusDetection != null)
            {
                _stylusDetection.IsStylusConnectedAndAssigned();
            }
#endif
        }
    }
}
