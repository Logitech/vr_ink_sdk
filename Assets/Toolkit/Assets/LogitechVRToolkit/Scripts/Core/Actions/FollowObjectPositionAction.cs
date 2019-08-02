namespace Logitech.XRToolkit.Actions
{
    using Action = Logitech.XRToolkit.Core.Action;
    using Logitech.XRToolkit.Utils;
    using System;
    using UnityEngine;

    /// <summary>
    /// Allows an object to follow another object's position.
    /// </summary>
    [Serializable]
    public class FollowObjectPositionAction : Action
    {
        private bool IsFollowingPosition
        {
            get
            {
                return FollowTargetPosition.HasFlag(EAxis.X) || FollowTargetPosition.HasFlag(EAxis.Y) ||
                       FollowTargetPosition.HasFlag(EAxis.Z);
            }
        }

        [HideInInspector]
        public Transform Follower;
        [HideInInspector]
        public Transform TargetToFollow;
        private Transform _previousTargetToFollow;

        [EnumFlag]
        public EAxis FollowTargetPosition = default(EAxis);
        public bool PositionIsOffset;

        private Vector3 _targetPositionPreviousFrame;

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

            if (IsFollowingPosition)
            {
                UpdatePosition();
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

        private void UpdatePosition()
        {
            Vector3 position = UpdateFollowingAxes(TargetToFollow.position, FollowTargetPosition);

            if (PositionIsOffset)
            {
                position = position - UpdateFollowingAxes(_targetPositionPreviousFrame, FollowTargetPosition) + Follower.position;
            }

            Follower.position = position;
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

        private void UpdateTargetPreviousFrameValues()
        {
            _targetPositionPreviousFrame = TargetToFollow.position;
        }
    }
}
