namespace Logitech.XRToolkit.Triggers
{
    using Logitech.XRToolkit.Components;
    using Logitech.XRToolkit.Core;
    using Logitech.XRToolkit.Utils;
    using System;
    using UnityEngine;

    /// <summary>
    /// Trigger that is valid when a specified CollisionInteractable collides with another CollisionInteractable that
    /// meets a set of conditions.
    /// </summary>
    [Serializable]
    public class CollisionTrigger : Trigger
    {
        [Flags]
        public enum CollidableTargetType
        {
            AnyInteractable,
            Interactable,
            InteractableTag
        }

        public CollisionInteractable Source;
        public CollidableTargetType TargetType;
        [ShowIf("TargetType", CollidableTargetType.Interactable)]
        public CollisionInteractable Target;
        [ShowIf("TargetType", CollidableTargetType.InteractableTag)]
        public EInteractable Tag = EInteractable.Untagged;

        [Tooltip("Return the last valid collision, otherwise return the first valid collision")]
        public bool IsLast;
        [Tooltip("After a collision is finished, store the last collision rather than setting it to null")]
        public bool SaveLastCollision;

        public Transform CollidedTransform { get; private set; }

        public CollisionTrigger() { }

        public CollisionTrigger(CollisionInteractable source, CollisionInteractable target, bool isLast, bool saveLastCollision)
        {
            Source = source;
            Target = target;
            IsLast = isLast;
            SaveLastCollision = saveLastCollision;
        }

        public CollisionTrigger(CollisionInteractable source, EInteractable tag, CollisionInteractable target, bool isLast, bool saveLastCollision)
        {
            Source = source;
            Tag = tag;
            IsLast = isLast;
            SaveLastCollision = saveLastCollision;
        }

        public override bool IsValid()
        {
            switch (TargetType)
            {
                case CollidableTargetType.InteractableTag:
                    return CollisionWithTag();
                case CollidableTargetType.Interactable:
                    return CollisionWithTarget();
                case CollidableTargetType.AnyInteractable:
                    return LastCollision();
                default:
                    return false;
            }
        }

        /// <summary>
        /// Checks if the Source has collided with a CollisionInteractable with the specified tag.
        /// </summary>
        /// <remarks>
        /// The other CollisionInteractable will be saved as 'CollidedTransform' until the collision has exited or, if
        /// 'SaveLastCollision' is true, until another collision meets the conditions.
        /// </remarks>
        /// <returns>
        /// If the Source collided with a CollisionInteractable with the specified tag.
        /// </returns>
        private bool CollisionWithTag()
        {
            if (Source.CollisionWith(Tag, IsLast) != null)
            {
                CollidedTransform = Source.CollisionWith(Tag, IsLast).transform;
                return true;
            }

            if (!SaveLastCollision)
            {
                CollidedTransform = null;
            }

            return false;
        }

        /// <summary>
        /// Checks if the Source has collided with a specified CollisionInteractable.
        /// </summary>
        /// <remarks>
        /// The other CollisionInteractable will be saved as 'CollidedTransform' until the collision has exited or, if
        /// 'SaveLastCollision' is true, until another collision meets the conditions.
        /// </remarks>
        /// <returns>
        /// If the Source collided with a specified CollisionInteractable.
        /// </returns>
        private bool CollisionWithTarget()
        {
            if (Source.CollisionWith(Target, IsLast) != null)
            {
                CollidedTransform = Source.CollisionWith(Target, IsLast).transform;
                return true;
            }

            if (!SaveLastCollision)
            {
                CollidedTransform = null;
            }

            return false;
        }

        /// <summary>
        /// Checks if the Source has collided with any CollisionInteractable.
        /// </summary>
        /// <remarks>
        /// The other CollisionInteractable will be saved as 'CollidedTransform' until the collision has exited or, if
        /// 'SaveLastCollision' is true, until another collision meets the conditions.
        /// </remarks>
        /// <returns>
        /// If the Source collided with any CollisionInteractable.
        /// </returns>
        private bool LastCollision()
        {
            if (Source.LastCollision() != null)
            {
                CollidedTransform = Source.LastCollision().transform;
                return true;
            }

            if (!SaveLastCollision)
            {
                CollidedTransform = null;
            }

            return false;
        }
    }
}
