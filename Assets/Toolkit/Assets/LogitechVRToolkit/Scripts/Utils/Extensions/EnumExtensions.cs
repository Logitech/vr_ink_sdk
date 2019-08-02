namespace Logitech.XRToolkit.Utils
{
    using System;

    /// <summary>
    /// Extension methods for enums.
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// A .Net 3.5 way to mimic the .Net 4 "HasFlag" method.
        /// </summary>
        /// <param name="variable">The tested enum.</param>
        /// <param name="flag">The flag to test against.</param>
        /// <returns>True if the flag is set, otherwise false.</returns>
        public static bool HasFlag(this Enum variable, Enum flag)
        {
            if (variable.GetType() != flag.GetType())
            {
                throw new ArgumentException(variable.GetType().Name + " and " + flag.GetType().Name + " are not the same type!");
            }
            return (Convert.ToInt32(variable) & Convert.ToInt32(flag)) == Convert.ToInt32(flag);
        }
    }
}
