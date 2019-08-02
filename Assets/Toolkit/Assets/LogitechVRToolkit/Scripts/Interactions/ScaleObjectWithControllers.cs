namespace Logitech.XRToolkit.Interactions
{
    using Logitech.XRToolkit.Actions;
    using Logitech.XRToolkit.Triggers;
    using UnityEngine;

    /// <summary>
    /// Scales an object with two controllers.
    /// </summary>
    public class ScaleObjectWithControllers : MonoBehaviour
    {
        [SerializeField]
        private CollisionTrigger _collisionTrigger;
        [SerializeField]
        private InputTrigger _leftButtonDownTrigger;
        [SerializeField]
        private InputTrigger _rightButtonDownTrigger;
        [SerializeField]
        private InputTrigger _leftButtonUpTrigger;
        [SerializeField]
        private InputTrigger _rightButtonUpTrigger;

        [SerializeField]
        private ScaleAction _scaleAction;

        private bool _scaling;
        private bool _canScaleLeft;
        private bool _canScaleRight;

        private void Update()
        {
            _scaleAction.ObjectToScale = _collisionTrigger.CollidedTransform;

            if (_leftButtonDownTrigger.IsValid() && (_collisionTrigger.IsValid() || _canScaleRight))
            {
                _canScaleLeft = true;
            }

            if (_rightButtonDownTrigger.IsValid() && (_collisionTrigger.IsValid() || _canScaleLeft))
            {
                _canScaleRight = true;
            }

            if (_leftButtonUpTrigger.IsValid())
            {
                _canScaleLeft = false;
            }

            if (_rightButtonUpTrigger.IsValid())
            {
                _canScaleRight = false;
            }

            _scaleAction.Update(_canScaleLeft && _canScaleRight);
        }
    }
}
