namespace Logitech.XRToolkit.Components
{
    using Logitech.XRToolkit.Utils;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Collides with and stores information of other CollisionInteractables.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class CollisionInteractable : Interactable
    {
        /// <summary>
        /// A list of CollisionInteractables that this object is currently collided with.
        /// </summary>
        private List<CollisionInteractable> _collisionInteractables = new List<CollisionInteractable>();
        /// <summary>
        /// The number of colliders that belong to the RigidBody of each CollisionInteractable.
        /// </summary>
        /// <remarks>
        /// This is used in the case that one collider exits but another attached to the RigidBody is still colliding.
        /// </remarks>
        private List<int> _colliderCounts = new List<int>();

        private void OnCollisionEnter(Collision other)
        {
            if (other.collider.attachedRigidbody == null)
            {
                return;
            }

            CollisionInteractable otherCollisionInteractable =
                other.collider.attachedRigidbody.transform.GetComponent<CollisionInteractable>();

            if (otherCollisionInteractable != null)
            {
                if (!_collisionInteractables.Contains(otherCollisionInteractable))
                {
                    _collisionInteractables.Add(otherCollisionInteractable);
                    _colliderCounts.Add(1);
                }
                else
                {
                    _colliderCounts[_collisionInteractables.IndexOf(otherCollisionInteractable)]++;
                }
            }
        }

        private void OnCollisionExit(Collision other)
        {
            if (other.collider.attachedRigidbody == null)
            {
                return;
            }

            CollisionInteractable otherCollisionInteractable =
                other.collider.attachedRigidbody.transform.GetComponent<CollisionInteractable>();

            if (_collisionInteractables.Contains(otherCollisionInteractable))
            {
                if (_colliderCounts[_collisionInteractables.IndexOf(otherCollisionInteractable)] > 1)
                {
                    _colliderCounts[_collisionInteractables.IndexOf(otherCollisionInteractable)]--;
                }
                else
                {
                    _colliderCounts.RemoveAt(_collisionInteractables.IndexOf(otherCollisionInteractable));
                    _collisionInteractables.Remove(otherCollisionInteractable);
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.attachedRigidbody == null)
            {
                return;
            }

            CollisionInteractable otherCollisionInteractable =
                other.attachedRigidbody.transform.GetComponent<CollisionInteractable>();

            if (otherCollisionInteractable != null)
            {
                if (!_collisionInteractables.Contains(otherCollisionInteractable))
                {
                    _collisionInteractables.Add(otherCollisionInteractable);
                    _colliderCounts.Add(1);
                }
                else
                {
                    _colliderCounts[_collisionInteractables.IndexOf(otherCollisionInteractable)] += 1;
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.attachedRigidbody == null)
            {
                return;
            }

            CollisionInteractable otherCollisionInteractable =
                other.attachedRigidbody.transform.GetComponent<CollisionInteractable>();

            if (_collisionInteractables.Contains(otherCollisionInteractable))
            {
                if (_colliderCounts[_collisionInteractables.IndexOf(otherCollisionInteractable)] > 1)
                {
                    _colliderCounts[_collisionInteractables.IndexOf(otherCollisionInteractable)]--;
                }
                else
                {
                    _colliderCounts.RemoveAt(_collisionInteractables.IndexOf(otherCollisionInteractable));
                    _collisionInteractables.Remove(otherCollisionInteractable);
                }
            }
        }

        /// <summary>
        /// Disabling a GameObject doesn't fire collision and trigger exit events, so we manually remove this
        /// CollisionInteractable from other CollisionInteractable's tracking, and clear all other CollisionInteractables
        /// from this. When enabled, collision and trigger enter events will fire as expected.
        /// </summary>
        private void OnDisable()
        {
            foreach (CollisionInteractable collisionInteractable in _collisionInteractables)
            {
                collisionInteractable.RemoveCollision(this);
            }

            _collisionInteractables.Clear();
            _colliderCounts.Clear();
        }

        /// <summary>
        /// Removes tracking of a specified CollisionInteractable.
        /// </summary>
        /// <param name="otherCollisionInteractable">The CollisionInteractable to be removed.</param>
        public void RemoveCollision(CollisionInteractable otherCollisionInteractable)
        {
            _colliderCounts.RemoveAt(_collisionInteractables.IndexOf(otherCollisionInteractable));
            _collisionInteractables.Remove(otherCollisionInteractable);
        }

        /// <summary>
        /// Gets the last CollisionInteractable that was collided with.
        /// </summary>
        /// <returns>
        /// The last CollisionInteractable collided with.
        /// </returns>
        public CollisionInteractable LastCollision()
        {
            return _collisionInteractables.Count != 0 ? _collisionInteractables[_collisionInteractables.Count - 1] : null;
        }

        /// <summary>
        /// Gets a CollisionInteractable by specified tag.
        /// </summary>
        /// <param name="tag">The target CollisionInteractable tag.</param>
        /// <param name="isLast">If the target CollisionInteractable must be the last collision.</param>
        /// <returns>
        /// The collided CollisionInteractable or null if the conditions are not met.
        /// </returns>
        public CollisionInteractable CollisionWith(EInteractable tag, bool isLast)
        {
            if (isLast)
            {
                if (_collisionInteractables.Count == 0)
                {
                    return null;
                }

                CollisionInteractable collisionInteractable =
                    _collisionInteractables[_collisionInteractables.Count - 1].transform.GetComponent<CollisionInteractable>();

                return collisionInteractable.ContainsTag(tag)
                    ? _collisionInteractables[_collisionInteractables.Count - 1]
                    : null;
            }

            foreach (var collisionInteractable in _collisionInteractables)
            {
                if (collisionInteractable != null && collisionInteractable.ContainsTag(tag))
                {
                    return collisionInteractable;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets a CollisionInteractable by specified tag.
        /// </summary>
        /// <param name="target">The target CollisionInteractable.</param>
        /// <param name="isLast">If the target CollisionInteractable must be the last collision.</param>
        /// <returns>
        /// The target CollisionInteractable or null if the conditions are not met.
        /// </returns>
        public CollisionInteractable CollisionWith(CollisionInteractable target, bool isLast)
        {
            if (isLast)
            {
                if (_collisionInteractables.Count == 0)
                {
                    return null;
                }
                return _collisionInteractables[_collisionInteractables.Count - 1].transform == target.transform
                    ? _collisionInteractables[_collisionInteractables.Count - 1]
                    : null;
            }

            foreach (var collisionInteractable in _collisionInteractables)
            {
                if (collisionInteractable == target)
                {
                    return collisionInteractable;
                }
            }

            return null;
        }
    }
}
