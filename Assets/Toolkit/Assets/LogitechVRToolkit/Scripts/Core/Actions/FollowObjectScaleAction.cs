namespace Logitech.XRToolkit.Actions
{
    using Action = Logitech.XRToolkit.Core.Action;
    using Logitech.XRToolkit.Utils;
    using System;
    using UnityEngine;

    /// <summary>
    /// Allows an object to follow another object's scale.
    /// </summary>
    [Serializable]
    public class FollowObjectScaleAction : Action
    {
        private bool IsFollowingScale
        {
            get
            {
                return FollowTargetScale.HasFlag(EAxis.X) || FollowTargetScale.HasFlag(EAxis.Y) ||
                       FollowTargetScale.HasFlag(EAxis.Z);
            }
        }

        [HideInInspector]
        public Transform Follower;
        [HideInInspector]
        public Transform TargetToFollow;
        private Transform _previousTargetToFollow;

        [EnumFlag]
        public EAxis FollowTargetScale = default(EAxis);
        public bool ScaleIsOffset;
        [EnumFlag]
        public EAxis InvertScale = default(EAxis);

        private Vector3 _targetScalePreviousFrame;

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

            if (IsFollowingScale)
            {
                UpdateScale();
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

        private void UpdateScale()
        {
            Vector3 scale;

            if (ScaleIsOffset)
            {
                scale = TargetToFollow.localScale - _targetScalePreviousFrame;
                scale = InvertAxes(scale, InvertScale);
                scale += Follower.localScale;
            }
            else
            {
                scale = TargetToFollow.localScale;
            }

            scale = UpdateFollowingAxes(scale, FollowTargetScale);
            Follower.localScale = scale;
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
            _targetScalePreviousFrame = TargetToFollow.localScale;
        }
    }
}
