namespace Logitech.XRToolkit.Providers
{
    using Logitech.XRToolkit.Core;
    using Logitech.XRToolkit.Components;
    using System;
    using UnityEngine;

    /// <summary>
    /// Provides access to the Stylus model transform (snapped to surfaces, not absolute tracking).
    /// </summary>
    [Serializable]
    public class StylusModelTransformProvider : Provider<Transform>
    {
        public override Transform GetOutput()
        {
            return LogitechStylus.Instance.transform;
        }
    }
}
