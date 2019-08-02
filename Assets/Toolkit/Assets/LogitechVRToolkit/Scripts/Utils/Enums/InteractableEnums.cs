namespace Logitech.XRToolkit.Utils
{
    using System;

    /// <summary>
    /// Tags for Interactables.
    /// </summary>
    [Flags]
    public enum EInteractable
    {
        Untagged = (1 << 0),
        UIInteractable = (1 << 1),
        WritableSurface = (1 << 2),
        AirDrawingPreventionZone = (1 << 3),
        Snappable = (1 << 4),
        Grabbable = (1 << 5),
        Highlight = (1 << 6),
        Scalable = (1 << 7),
        ScriptGenerated = (1 << 8),
        Haptics = (1 << 9),
        Stylus
    }
}
