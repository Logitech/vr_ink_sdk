namespace Logitech.XRToolkit.Interactions
{
    using Logitech.XRToolkit.Actions;
    using Logitech.XRToolkit.Triggers;
    using UnityEngine;

    /// <summary>
    /// Highlights an object on collision.
    /// </summary>
    public class HighlightOnCollision : MonoBehaviour
    {
        [SerializeField]
        private CollisionTrigger _collideTrigger;
        [SerializeField]
        private HighlightAction _highlightAction;

        private void Update()
        {
            _highlightAction.ObjectToHighlight = _collideTrigger.CollidedTransform;
            _highlightAction.Update(_collideTrigger.IsValid());
        }
    }
}
