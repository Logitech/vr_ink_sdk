namespace Logitech.XRToolkit.Providers
{
    using Logitech.XRToolkit.Core;
    using System;

    /// <summary>
    /// Provides a generic function.
    /// </summary>
    /// <remarks>
    /// This may not be particularly useful until generic interfaces can be serialized to allow for inspector based
    /// dependency injection.
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    public class FunctionProvider<T> : Provider<T>
    {
        public Func<T> Function;

        public FunctionProvider(Func<T> function)
        {
            Function = function;
        }

        public override T GetOutput()
        {
            return Function();
        }
    }
}
