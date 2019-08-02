namespace Logitech.XRToolkit.IO
{
    using Logitech.XRToolkit.Utils;
    using UnityEngine;

    /// <summary>
    /// Gets trackpad touch events from a device.
    /// </summary>
    public interface ITouchEvents
    {
        bool OnTap();
        bool OnDoubleTap();
        bool IsInZone(ETouchZone touchZoneType);
        bool TouchRemainedInZone(bool requireAllTrue, params ETouchZone[] zones);
        bool OnSwipe(ESwipeDirection eSwipeDirection);
        ESwipeDirection OnSwipe();
        Vector2 Scroll();
    }
}
