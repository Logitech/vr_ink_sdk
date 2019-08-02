namespace Logitech.XRToolkit.Core
{
    using System;
    using Triggers;

    /// <summary>
    /// Triggers may be used to execute actions if certain (sometimes complex) conditions are met.
    /// <seealso cref="Action"/>
    /// <seealso cref="Provider"/>
    /// <seealso cref="InputTrigger"/>
    /// </summary>
    [Serializable]
    public abstract class Trigger
    {
        /// <summary>
        /// Gets the validity of the trigger.
        /// </summary>
        /// <returns>
        /// If the trigger is true or false.
        /// </returns>
        public abstract bool IsValid();
    }
}
