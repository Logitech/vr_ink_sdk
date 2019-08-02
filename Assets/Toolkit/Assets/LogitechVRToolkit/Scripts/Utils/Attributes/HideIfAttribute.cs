namespace Logitech.XRToolkit.Utils
{
    using System;
    using UnityEngine;
    
    /// <summary>
    /// Attribute for properties and fields that will hide the element in the inspector.
    /// </summary>
    /// <seealso cref="ShowIfAttribute"/>
    /// <seealso cref="DisableIfAttribute"/>
    /// <seealso cref="EnableIfAttribute"/>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class HideIfAttribute : PropertyAttribute
    {
        public string ComparedPropertyName;
        public object ComparedValue;
        
        /// <summary>
        /// Hides the property in the inspector, based on its boolean state.
        /// </summary>
        /// <param name="propertyName">Name of the property to compare the value of.</param>
        public HideIfAttribute(string propertyName)
        {
            ComparedPropertyName = propertyName;
            ComparedValue = true;
        }
        
        /// <summary>
        /// Hides the property in the inspector, based on its state compared to a defined value.
        /// </summary>
        /// <param name="propertyName">Name of the property to compare the value of.</param>
        /// <param name="comparedValue">The value to compare against.</param>
        public HideIfAttribute(string propertyName, object comparedValue)
        {
            ComparedPropertyName = propertyName;
            ComparedValue = comparedValue;
        }
    }
}
