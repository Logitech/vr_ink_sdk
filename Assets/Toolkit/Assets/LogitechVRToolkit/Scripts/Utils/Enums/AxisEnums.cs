namespace Logitech.XRToolkit.Utils
{
    using System;

    /// <summary>
    /// Axis flags for specifying axes in the inspector.
    /// </summary>
    [Flags]
    public enum EAxis
    {
        X  = (1 << 0),
        Y  = (1 << 1),
        Z  = (1 << 2)
    }
}
