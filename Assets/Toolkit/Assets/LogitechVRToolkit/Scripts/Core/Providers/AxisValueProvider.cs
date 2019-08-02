namespace Logitech.XRToolkit.Providers
{
    using Logitech.XRToolkit.Core;
    using Logitech.XRToolkit.Utils;
    using System;

    /// <summary>
    /// Provides the analog value of a TrackedDevice axis input.
    /// </summary>
    [Serializable]
    public class AxisValueProvider : Provider<float>
    {
        public TrackedDeviceProvider TrackedDeviceProvider;
        public StylusAxisInput StylusAxisInput;

        public AxisValueProvider() { }

        public AxisValueProvider(TrackedDeviceProvider trackedDeviceProvider, StylusAxisInput stylusAxisInput)
        {
            TrackedDeviceProvider = trackedDeviceProvider;
            StylusAxisInput = stylusAxisInput;
        }

        public override float GetOutput()
        {
            return TrackedDeviceProvider.GetOutput().GetAxis(StylusAxisInput);
        }
    }
}
