namespace Logitech.XRToolkit.Providers
{
    using Logitech.XRToolkit.Core;
    using Logitech.XRToolkit.IO;
    using Logitech.XRToolkit.Utils;
    using System;

    /// <summary>
    /// Provides access to the TrackedDevice with matching Handedness
    /// </summary>
    /// <seealso cref="IO.TrackedDevice"/>
    [Serializable]
    public class TrackedDeviceProvider : Provider<TrackedDevice>
    {
        public Handedness TrackedDevice;

        public TrackedDeviceProvider() { }

        public TrackedDeviceProvider(Handedness trackedDevice)
        {
            TrackedDevice = trackedDevice;
        }

        public override TrackedDevice GetOutput()
        {
            return LogitechToolkitManager.Instance.Devices[TrackedDevice];
        }
    }
}
