namespace Logitech.XRToolkit.Providers
{
    using Logitech.XRToolkit.Core;
    using System;

    /// <summary>
    /// Provides a constant value for an object of type T (Float, Transform, etc.).
    /// </summary>
    /// <typeparam name="T">The type of object provided.</typeparam>
    [Serializable]
    public class ConstantProvider<T> : Provider<T>
    {
        public T Target;

        public ConstantProvider() { }

        public ConstantProvider(T target)
        {
            Target = target;
        }

        public override T GetOutput()
        {
            return Target;
        }
    }
}
