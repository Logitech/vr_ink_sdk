namespace Logitech.XRToolkit.Utils
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Attribute for properties and fields that will make the element readonly in the inspector.
    /// </summary>
    /// <seealso cref="EnableIfAttribute"/>
    /// <seealso cref="HideIfAttribute"/>
    /// <seealso cref="ShowIfAttribute"/>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class DisableIfAttribute : PropertyAttribute
    {
        public string ComparedPropertyName;
        public object ComparedValue;

        /// <summary>
        /// Disables the property in the inspector, based on its boolean state.
        /// </summary>
        /// <param name="propertyName">Name of the property to compare the value of.</param>
        public DisableIfAttribute(string propertyName)
        {
            ComparedPropertyName = propertyName;
            ComparedValue = true;
        }

        /// <summary>
        /// Disables the property in the inspector, based on its state compared to a defined value.
        /// </summary>
        /// <param name="propertyName">Name of the property to compare the value of.</param>
        /// <param name="comparedValue">The value to compare against.</param>
        public DisableIfAttribute(string propertyName, object comparedValue)
        {
            ComparedPropertyName = propertyName;
            ComparedValue = comparedValue;
        }
    }
}
