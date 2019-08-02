namespace Logitech.XRToolkit.IO
{
    using Logitech.XRToolkit.Utils;

    /// <summary>
    /// Gets axes and buttons events from a device and send haptic events to a device.
    /// </summary>
    public interface IIOHandler
    {
        bool GetButtonDown(StylusButton stylusButton);
        bool GetButton(StylusButton stylusButton);
        bool GetButtonUp(StylusButton stylusButton);
        float GetAxis(StylusAxisInput stylusAxisInput);
        void SendHapticPulse(float delayInSeconds, float durationInSeconds, float frequency, float amplitude);
    }
}
