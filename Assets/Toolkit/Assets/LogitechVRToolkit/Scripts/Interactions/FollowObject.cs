namespace Logitech.XRToolkit.Interactions
{
    using Logitech.XRToolkit.Actions;
    using UnityEngine;

    /// <summary>
    /// Allows an object to follow another object (position, rotation, scale).
    /// </summary>
    public class FollowObject : MonoBehaviour
    {
        public FollowObjectAction FollowObjectAction;

        private void Update()
        {
            FollowObjectAction.Update(true);
        }
    }
}
