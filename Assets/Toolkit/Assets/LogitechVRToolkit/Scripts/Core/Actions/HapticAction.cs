namespace Logitech.XRToolkit.Actions
{
    using Action = Logitech.XRToolkit.Core.Action;
    using Logitech.XRToolkit.Providers;
    using System;
    using UnityEngine;

    /// <summary>
    /// Triggers a TrackedDevice haptic pulse.
    /// </summary>
    [Serializable]
    public class HapticAction : Action
    {
        [SerializeField]
        private float _durationInSeconds = 0.1f;
        [SerializeField, Range(0, 320)]
        private float _frequency = 10;
        [SerializeField, Range(0, 1)]
        private float _amplitude = 0.3f;
        [SerializeField]
        private TrackedDeviceProvider _trackedDeviceProvider;

        protected override void TriggerValid()
        {
            _trackedDeviceProvider.GetOutput().SendHapticPulse(0, _durationInSeconds, _frequency, _amplitude);
        }
    }
}
