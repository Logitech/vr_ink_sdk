namespace Logitech.XRToolkit.Utils
{
    /// <summary>
    /// Stylus Touchstrip rejection states.
    /// </summary>
    public enum EButtonRejectionState
    {
        Neither,
        TouchStrip_Delay,
        TouchStrip_Only,
        Primary_Analog_Only,
        TouchStrip_Only_Lock,
        TouchStrip_After_Delay
    }

    /// <summary>
    /// Stylus handedness.
    /// </summary>
    public enum Handedness
    {
        Primary,
        NonDominant
    }

    /// <summary>
    /// Default means that it is a platform type of device. This might change how the behaviour of the ITrackedDevice
    /// interface is implemented. Not currently used.
    /// </summary>
    public enum DeviceType
    {
        Stylus,
        Keyboard,
        Default
    }

    /// <summary>
    /// Binary Stylus inputs.
    /// </summary>
    public enum StylusButton
    {
        Primary,
        Tip,
        Grip,
        Menu,
        TouchstripTouch,
        TouchstripClick,
    }

    /// <summary>
    /// Analog Stylus inputs.
    /// </summary>
    public enum StylusAxisInput
    {
        Primary,
        Tip,
        Grip,
        TrackpadX,
        TrackpadY
    }

    /// <summary>
    /// Binary TrackedDevice inputs.
    /// </summary>
    public enum TrackedDeviceButton
    {
        Trigger,
        Grab,
        Menu,
        TrackpadTouch,
        TrackpadClick,
    }

    /// <summary>
    /// Analog TrackedDevice inputs.
    /// </summary>
    public enum TrackedDeviceAxisInput
    {
        Trigger,
        Grab,
        TrackpadX,
        TrackpadY
    }
}
