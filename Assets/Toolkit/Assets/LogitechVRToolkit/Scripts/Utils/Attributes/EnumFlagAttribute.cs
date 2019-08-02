namespace Logitech.XRToolkit.Utils
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Attribute for enums to allow BitFlag functionality in the inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class EnumFlagAttribute : PropertyAttribute
    {
        public EnumFlagAttribute() { }
    }
}
