namespace Logitech.XRToolkit.Actions
{
    using Action = Logitech.XRToolkit.Core.Action;
    using Logitech.XRToolkit.Utils;
    using System;
    using UnityEngine;

    /// <summary>
    /// Allows an object to follow the midpoint between two objects.
    /// </summary>
    [Serializable]
    public class FollowTwoObjectsPositionAction : Action
    {
        private enum ETargetToFollow
        {
            FirstTarget,
            SecondTarget
        }

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
        public Transform FirstTargetToFollow;
        [HideInInspector]
        public Transform SecondTargetToFollow;
        private Transform _previousFirstTargetToFollow;
        private Transform _previousSecondTargetToFollow;

        [EnumFlag]
        public EAxis FollowTargetPosition = default(EAxis);
        public bool PositionIsOffset;

        private Vector3 _firstTargetPositionPreviousFrame;
        private Vector3 _secondTargetPositionPreviousFrame;

        protected override void TriggerValid()
        {
            if (Follower == null)
            {
                Debug.LogError("Follower is not set!");
                return;
            }
            if (FirstTargetToFollow == null)
            {
                Debug.LogError("FirstTargetToFollow is not set!");
                return;
            }
            if (SecondTargetToFollow == null)
            {
                Debug.LogError("TargetToFollow is not set!");
                return;
            }
            if (FirstTargetToFollow != _previousFirstTargetToFollow)
            {
                _previousFirstTargetToFollow = FirstTargetToFollow;
                if (FirstTargetToFollow != null)
                {
                    UpdateTargetPreviousFrameValues(ETargetToFollow.FirstTarget);
                }
                if (SecondTargetToFollow != _previousSecondTargetToFollow)
                {
                    _previousSecondTargetToFollow = SecondTargetToFollow;
                    if (SecondTargetToFollow != null)
                    {
                        UpdateTargetPreviousFrameValues(ETargetToFollow.SecondTarget);
                    }
                }
            }
            if (IsFollowingPosition)
            {
                UpdatePosition();
            }

            if (FirstTargetToFollow != null)
            {
                UpdateTargetPreviousFrameValues(ETargetToFollow.FirstTarget);
            }
            if (SecondTargetToFollow != null)
            {
                UpdateTargetPreviousFrameValues(ETargetToFollow.SecondTarget);
            }

        }

        protected override void TriggerInvalid()
        {
            if (FirstTargetToFollow == null || SecondTargetToFollow == null)
            {
                return;
            }

            _previousFirstTargetToFollow = FirstTargetToFollow;
            UpdateTargetPreviousFrameValues(ETargetToFollow.FirstTarget);
            _previousSecondTargetToFollow = SecondTargetToFollow;
            UpdateTargetPreviousFrameValues(ETargetToFollow.SecondTarget);
        }

        private void UpdatePosition()
        {
            Vector3 position = UpdateFollowingAxes(FirstTargetToFollow.position,SecondTargetToFollow.position, FollowTargetPosition);

            if (PositionIsOffset)
            {
                position = position - UpdateFollowingAxes(_firstTargetPositionPreviousFrame, _secondTargetPositionPreviousFrame, FollowTargetPosition) + Follower.position;
            }

            Follower.position = position;
        }

        private Vector3 UpdateFollowingAxes(Vector3 firstValue, Vector3 secondValue, EAxis axis)
        {
            Vector3 value = (firstValue + secondValue) / 2;
            if (!axis.HasFlag(EAxis.X))
            {
                value = new Vector3(0, (firstValue.y + secondValue.y) / 2, (firstValue.z + secondValue.z) / 2);
            }
            if (!axis.HasFlag(EAxis.Y))
            {
                value = new Vector3((firstValue.x + secondValue.x) / 2, 0, (firstValue.x + secondValue.x) / 2);
            }
            if (!axis.HasFlag(EAxis.Z))
            {
                value = new Vector3((firstValue.x + secondValue.x) / 2, (firstValue.y + secondValue.y) / 2, 0);
            }

            return value;
        }

        private void UpdateTargetPreviousFrameValues(ETargetToFollow target)
        {
            switch (target)
            {
                case ETargetToFollow.FirstTarget:
                    _firstTargetPositionPreviousFrame = FirstTargetToFollow.position;
                    break;
                case ETargetToFollow.SecondTarget:
                    _secondTargetPositionPreviousFrame = SecondTargetToFollow.position;
                    break;
            }
        }
    }
}
