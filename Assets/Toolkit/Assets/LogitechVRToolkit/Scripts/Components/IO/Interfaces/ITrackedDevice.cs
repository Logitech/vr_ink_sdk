namespace Logitech.XRToolkit.IO
{
    using Logitech.XRToolkit.Utils;
    using UnityEngine;

    /// <summary>
    /// Gets tracking info of a device.
    /// </summary>
    public interface ITrackedDevice
    {
        // Tracking.
        Handedness StylusHand { get; set; }
        Utils.DeviceType Type { get; set; }
        Pose GetDevicePose();

        // Cleanup.
        void OnDestroy();
        int DeviceIndex { get; }
    }
}
