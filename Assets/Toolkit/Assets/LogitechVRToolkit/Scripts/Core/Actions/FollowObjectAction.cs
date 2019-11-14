namespace Logitech.XRToolkit.Actions
{
    using Action = Logitech.XRToolkit.Core.Action;
    using Logitech.XRToolkit.Providers;
    using Logitech.XRToolkit.Utils;
    using System;
    using UnityEngine;

    /// <summary>
    /// Allows an object to follow another object (position, rotation, scale).
    /// </summary>
    [Serializable]
    public class FollowObjectAction : Action
    {
        [Header("Settings")]
        [Tooltip("Object that is going to follow TargetToFollow")]
        public Transform Follower;
        public bool FollowTrackedDevice;
        [ShowIf("FollowTrackedDevice")]
        public TrackedDeviceProvider TrackedDeviceProvider;
        [HideIf("FollowTrackedDevice")]
        public Transform TargetToFollow;

        private Transform _previousTargetToFollow;

        [Header("Position")]
        public bool FollowObjectPosition;
        [ShowIf("FollowObjectPosition")]
        public FollowObjectPositionAction FollowPositionAction;

        [Header("Rotation")]
        public bool FollowObjectRotation;
        [ShowIf("FollowObjectRotation")]
        public FollowObjectRotationAction FollowRotationAction;

        [Header("Scale")]
        public bool FollowObjectScale;
        [ShowIf("FollowObjectScale")]
        public FollowObjectScaleAction FollowScaleAction;

        protected override void TriggerValid()
        {
            FollowPositionAction.Follower = Follower;
            FollowRotationAction.Follower = Follower;
            FollowScaleAction.Follower = Follower;

            if (FollowTrackedDevice)
            {
                TargetToFollow = TrackedDeviceProvider.GetOutput().transform;
            }

            FollowPositionAction.TargetToFollow = TargetToFollow;
            FollowRotationAction.TargetToFollow = TargetToFollow;
            FollowScaleAction.TargetToFollow = TargetToFollow;

            FollowPositionAction.Update(FollowObjectPosition);
            FollowRotationAction.Update(FollowObjectRotation);
            FollowScaleAction.Update(FollowObjectScale);
        }

        protected override void TriggerInvalid()
        {
            FollowPositionAction.Update(false);
            FollowRotationAction.Update(false);
            FollowScaleAction.Update(false);
        }
    }
}
