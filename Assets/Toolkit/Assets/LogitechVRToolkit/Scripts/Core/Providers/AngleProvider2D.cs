namespace Logitech.XRToolkit.Providers
{
    using Logitech.XRToolkit.Core;
    using Logitech.XRToolkit.Utils;
    using System;
    using UnityEngine;

    /// <summary>
    /// Provides the angle in one axis between two objects given a starting angle.
    /// </summary>
    [Serializable]
    public class AngleProvider2D : Provider<float>
    {
        public Transform FirstTransform;
        public Transform SecondTransform;
        public EAxis Axis = EAxis.X;

        [HideInInspector]
        public Vector3 StartAngle;

        public AngleProvider2D() { }

        public AngleProvider2D(Transform firstTransform, Transform secondTransform, EAxis axis)
        {
            FirstTransform = firstTransform;
            SecondTransform = secondTransform;
            Axis = axis;
        }

        public sealed override void Init()
        {
            StartAngle = FirstTransform.position - SecondTransform.position;
        }

        private float GetAngle()
        {
            Vector3 traveledVector = FirstTransform.position - SecondTransform.position;
            switch (Axis)
            {
                case EAxis.X:
                    return Vector2.SignedAngle(new Vector2(traveledVector.y, traveledVector.z), new Vector2(StartAngle.y, StartAngle.z));
                case EAxis.Y:
                    return Vector2.SignedAngle(new Vector2(traveledVector.x, traveledVector.z), new Vector2(StartAngle.x, StartAngle.z));
                case EAxis.Z:
                    return Vector2.SignedAngle(new Vector2(traveledVector.x, traveledVector.y), new Vector2(StartAngle.x, StartAngle.y));
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override float GetOutput()
        {
            return GetAngle();
        }
    }
}
