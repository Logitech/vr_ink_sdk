namespace Logitech.XRToolkit.Core
{
    using Providers;

    /// <summary>
    /// Providers may be used by triggers and actions to get access to an object of type T. This is especially useful
    /// when that value T might change over the lifetime of the app.
    /// <seealso cref="Trigger"/>
    /// <seealso cref="Action"/>
    /// </summary>
    /// <typeparam name="T">The type of object provided.</typeparam>
    public abstract class Provider<T> : Provider
    {
        /// <summary>
        /// Use this implicit cast very sparingly, as it is not very easy to read afterwards. Prefer using
        /// ConstantProvider constructor where readability is not impeded.
        /// </summary>
        public static implicit operator Provider<T>(T x)
        {
            return new ConstantProvider<T>(x);
        }

        /// <summary>
        /// Initializes any data that may need initialization.
        /// </summary>
        public virtual void Init() { }

        /// <summary>
        /// Gets an output of type T from the provider.
        /// </summary>
        /// <returns>
        /// The provided output of the provider.
        /// </returns>
        public abstract T GetOutput();
    }

    /// <summary>
    /// Container class for drawers & serialization.
    /// <seealso cref="Provider{T}"/>
    /// </summary>
    public abstract class Provider { }
}
