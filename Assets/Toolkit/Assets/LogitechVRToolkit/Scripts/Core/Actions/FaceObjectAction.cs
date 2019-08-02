namespace Logitech.XRToolkit.Actions
{
    using Action = Logitech.XRToolkit.Core.Action;
    using Logitech.XRToolkit.Utils;
    using System;
    using UnityEngine;

    /// <summary>
    /// Makes a source object look towards a target one, optionally specifying a direction other than 'forward' (Z axis),
    /// a lerp factor for smooth transition, and whether the axis should face away from the target object.
    /// </summary>
    [Serializable]
    public class FaceObjectAction : Action
    {
        // TODO: Change this whole mechanism so that one may specify a Vector3 as forward, local to source.
        [SerializeField, Tooltip("The object to be rotated")]
        private Transform _source;
        [SerializeField, Tooltip("The object to follow")]
        private Transform _target;

        [Tooltip("How responsive the rotation should be (1 = instantaneous)")]
        public float LerpFactor;

        [SerializeField, Tooltip("The axis on source that will follow target")]
        private EAxis _forwardAxis;
        [SerializeField, Tooltip("The axis on target which source will have 'up' aligned with")]
        private EAxis _upAxis;

        [SerializeField, Tooltip("Should the specified forward axis be reversed")]
        private bool _negativeForward;
        [SerializeField, Tooltip("Should the specified up axis be reversed")]
        private bool _negativeUp;

        /// <summary>
        /// Acton that will make source face target along the given axes.
        /// </summary>
        protected override void TriggerValid()
        {
            // Note: this could be made much simpler by using proper quaternions (rotate the up and forward vectors so
            // that z and y face directions such that the desired local directions face up and towards the target).

            // Direction to look towards.
            var forward = _target.position - _source.position;
            Vector3 up;
            if (_upAxis == EAxis.X)
            {
                up = _target.transform.right;
            }
            else if (_upAxis == EAxis.Y)
            {
                up = _target.transform.up;
            }
            else
            {
                up = _target.transform.forward;
            }

            // Invert that direction if needed.
            if (_negativeForward)
            {
                forward = -forward;
            }
            if (_negativeUp)
            {
                up = -up;
            }

            // Compute base look rotation with Z axis.
            Quaternion rotation = Quaternion.LookRotation(forward, up);

            // Make the look rotation face the right axis if needed.
            if (_forwardAxis == EAxis.X)
            {
                rotation *= Quaternion.FromToRotation(Vector3.right, Vector3.forward);
            }
            else if (_forwardAxis == EAxis.Y)
            {
                rotation *= Quaternion.FromToRotation(Vector3.up, Vector3.forward);
            }

            // Apply the rotation.
            _source.rotation = Quaternion.Lerp(_source.rotation, rotation, LerpFactor);
        }
    }
}
