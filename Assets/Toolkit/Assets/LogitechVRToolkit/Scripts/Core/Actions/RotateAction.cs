namespace Logitech.XRToolkit.Actions
{
    using Action = Logitech.XRToolkit.Core.Action;
    using Logitech.XRToolkit.Utils;
    using System;
    using UnityEngine;

    /// <summary>
    /// Rotates a Transform about an axis.
    /// </summary>
    [Serializable]
    public class RotateAction : Action
    {
        [HideInInspector]
        public Transform ObjectToRotate;

        // Rotation Adjustment.
        [HideInInspector]
        public float RotationValue;
        public EAxis RotateAxis = EAxis.X;

        private Quaternion _targetObjectInitialRotation;

        protected override void OnTriggerValid()
        {
            if (ObjectToRotate == null)
            {
                throw new ArgumentNullException(ObjectToRotate.name);
            }

            _targetObjectInitialRotation = ObjectToRotate.rotation;
        }

        protected override void TriggerValid()
        {
            if (ObjectToRotate == null)
            {
                throw new ArgumentNullException(ObjectToRotate.name);
            }

            RotateObject();
        }

        private void RotateObject()
        {
            switch (RotateAxis)
            {
                case EAxis.X:
                    ObjectToRotate.rotation = Quaternion.Euler(new Vector3(
                        _targetObjectInitialRotation.eulerAngles.x + RotationValue,
                        _targetObjectInitialRotation.eulerAngles.y,
                        _targetObjectInitialRotation.eulerAngles.z));
                    break;
                case EAxis.Y:
                    ObjectToRotate.rotation = Quaternion.Euler(new Vector3(
                        _targetObjectInitialRotation.eulerAngles.x,
                        _targetObjectInitialRotation.eulerAngles.y + RotationValue,
                        _targetObjectInitialRotation.eulerAngles.z));
                    break;
                case EAxis.Z:
                    ObjectToRotate.rotation = Quaternion.Euler(new Vector3(
                        _targetObjectInitialRotation.eulerAngles.x,
                        _targetObjectInitialRotation.eulerAngles.y,
                        _targetObjectInitialRotation.eulerAngles.z + RotationValue));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
