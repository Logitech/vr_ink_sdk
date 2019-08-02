namespace Logitech.XRToolkit.Utils
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Attribute for properties and fields to set the element label in the inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class LabelAttribute : PropertyAttribute
    {
        public string Name;

        public LabelAttribute(string name)
        {
            Name = name;
        }
    }
}
