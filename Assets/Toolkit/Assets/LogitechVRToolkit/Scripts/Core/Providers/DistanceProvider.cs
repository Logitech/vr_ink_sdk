namespace Logitech.XRToolkit.Providers
{
    using Logitech.XRToolkit.Core;
    using System;
    using UnityEngine;

    /// <summary>
    /// Provides the distance between two objects.
    /// </summary>
    [Serializable]
    public class DistanceProvider : Provider<float>
    {
        public Transform FirstTransform;
        public Transform SecondTransform;

        public DistanceProvider() { }

        public DistanceProvider(Transform firstTransform, Transform secondTransform)
        {
            FirstTransform = firstTransform;
            SecondTransform = secondTransform;
        }

        public override float GetOutput()
        {
            return Vector3.Distance(FirstTransform.position, SecondTransform.position);
        }
    }
}
