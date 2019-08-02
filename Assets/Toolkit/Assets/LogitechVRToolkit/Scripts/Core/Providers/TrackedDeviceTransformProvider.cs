namespace Logitech.XRToolkit.Providers
{
    using Logitech.XRToolkit.Core;
    using Logitech.XRToolkit.IO;
    using Logitech.XRToolkit.Utils;
    using System;
    using UnityEngine;

    /// <summary>
    /// Provides access to the TrackedDevice transform with matching Handedness.
    /// </summary>
    /// <remarks>
    /// The provided transform is the true position of the tracked device, and may not be the position of the model.
    /// For example, when it's snapped to a surface.
    /// </remarks>>
    [Serializable]
    public class TrackedDeviceTransformProvider : Provider<Transform>
    {
        public Handedness TrackedDevice;

        public TrackedDeviceTransformProvider() { }

        public TrackedDeviceTransformProvider(Handedness trackedDevice)
        {
            TrackedDevice = trackedDevice;
        }

        public override Transform GetOutput()
        {
            return LogitechToolkitManager.Instance.Devices[TrackedDevice].transform;
        }
    }
}
