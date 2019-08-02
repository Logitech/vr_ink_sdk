namespace Logitech.XRToolkit.Actions
{
    using Action = Logitech.XRToolkit.Core.Action;
    using Logitech.XRToolkit.Utils;
    using System;
    using UnityEngine;

    /// <summary>
    /// Allows an object to follow another object's rotation.
    /// </summary>
    [Serializable]
    public class FollowObjectRotationAction : Action
    {
        private bool IsFollowingRotation
        {
            get
            {
                return FollowTargetRotation.HasFlag(EAxis.X) || FollowTargetRotation.HasFlag(EAxis.Y) ||
                       FollowTargetRotation.HasFlag(EAxis.Z);
            }
        }

        private bool IsPivoting
        {
            get
            {
                return PivotAroundTarget.HasFlag(EAxis.X) || PivotAroundTarget.HasFlag(EAxis.Y) ||
                       PivotAroundTarget.HasFlag(EAxis.Z);
            }
        }

        [HideInInspector]
        public Transform Follower;
        [HideInInspector]
        public Transform TargetToFollow;
        private Transform _previousTargetToFollow;

        [EnumFlag]
        public EAxis FollowTargetRotation = default(EAxis);
        public bool RotationIsOffset;
        [EnumFlag]
        public EAxis InvertRotation = default(EAxis);

        [Space(10)]
        [EnumFlag]
        public EAxis PivotAroundTarget = default(EAxis);
        [EnumFlag]
        public EAxis InvertPivot = default(EAxis);

        private Quaternion _targetRotationPreviousFrame;

        protected override void TriggerValid()
        {
            if (Follower == null)
            {
                Debug.LogError("Follower is not set!");
                return;
            }

            if (TargetToFollow == null)
            {
                Debug.LogError("TargetToFollow is not set!");
                return;
            }

            if (TargetToFollow != _previousTargetToFollow)
            {
                _previousTargetToFollow = TargetToFollow;
                if (TargetToFollow != null)
                {
                    UpdateTargetPreviousFrameValues();
                }
            }

            if (IsFollowingRotation)
            {
                UpdateRotation();
            }

            if (IsPivoting)
            {
                UpdatePivot();
            }

            if (TargetToFollow != null)
            {
                UpdateTargetPreviousFrameValues();
            }
        }

        protected override void TriggerInvalid()
        {
            if (TargetToFollow == null)
            {
                return;
            }

            _previousTargetToFollow = TargetToFollow;
            UpdateTargetPreviousFrameValues();
        }

        private void UpdateRotation()
        {
            Quaternion rotation = Quaternion.Euler(UpdateFollowingAxes(TargetToFollow.rotation.eulerAngles, FollowTargetRotation));

            if (RotationIsOffset)
            {
                rotation *= Quaternion.Inverse(Quaternion.Euler(UpdateFollowingAxes(_targetRotationPreviousFrame.eulerAngles, FollowTargetRotation)));
                rotation.eulerAngles = InvertAxes(rotation.eulerAngles, InvertRotation);
                rotation *= Follower.rotation;
            }
            else
            {
                rotation.eulerAngles = InvertAxes(rotation.eulerAngles, InvertRotation);
            }

            Follower.rotation = rotation;
        }

        private void UpdatePivot()
        {
            Vector3 rotationAngles = (TargetToFollow.rotation * Quaternion.Inverse(_targetRotationPreviousFrame)).eulerAngles;

            rotationAngles = UpdateFollowingAxes(rotationAngles, FollowTargetRotation);
            rotationAngles = InvertAxes(rotationAngles, InvertPivot);
            Follower.position = Follower.position.RotatePointAroundPivot(TargetToFollow.position, rotationAngles);
        }

        private Vector3 UpdateFollowingAxes(Vector3 value, EAxis axis)
        {
            if (!axis.HasFlag(EAxis.X))
            {
                value = new Vector3(0, value.y, value.z);
            }
            if (!axis.HasFlag(EAxis.Y))
            {
                value = new Vector3(value.x, 0, value.z);
            }
            if (!axis.HasFlag(EAxis.Z))
            {
                value = new Vector3(value.x, value.y, 0);
            }

            return value;
        }

        private Vector3 InvertAxes(Vector3 value, EAxis axis)
        {
            if (axis.HasFlag(EAxis.X))
            {
                value = new Vector3(-value.x, value.y, value.z);
            }
            if (axis.HasFlag(EAxis.Y))
            {
                value = new Vector3(value.x, -value.y, value.z);
            }
            if (axis.HasFlag(EAxis.Z))
            {
                value = new Vector3(value.x, value.y, -value.z);
            }

            return value;
        }

        private void UpdateTargetPreviousFrameValues()
        {
            _targetRotationPreviousFrame = TargetToFollow.rotation;
        }
    }
}
