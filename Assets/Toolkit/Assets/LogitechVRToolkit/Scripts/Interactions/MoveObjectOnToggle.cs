namespace Logitech.XRToolkit.Interactions
{
    using Logitech.XRToolkit.Actions;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Moves an object when a toggle is on.
    /// </summary>
    public class MoveObjectOnToggle : MonoBehaviour
    {
        [SerializeField]
        private Toggle _toggle;
        [SerializeField]
        private AnimationMoveAction _animationMoveAction;

        void Update()
        {
            _animationMoveAction.Update(_toggle.isOn);
        }
    }
}
