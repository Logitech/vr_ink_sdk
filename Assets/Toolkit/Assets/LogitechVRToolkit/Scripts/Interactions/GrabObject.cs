namespace Logitech.XRToolkit.Interactions
{
    using Logitech.XRToolkit.Actions;
    using Logitech.XRToolkit.Components;
    using Logitech.XRToolkit.Providers;
    using Logitech.XRToolkit.Triggers;
    using Logitech.XRToolkit.Utils;
    using UnityEngine;

    /// <summary>
    /// Grabs and manipulates an object with a controller.
    /// </summary>
    /// <remarks>
    /// The object can be grabbed when collided with and a specified button is pressed. The object can also be
    /// highlighted, and further manipulated with a second controller.
    /// </remarks>
    public class GrabObject : MonoBehaviour
    {
        [Header("Grab Options")]
        [SerializeField]
        private CollisionTrigger _collisionTrigger;

        [Space(10)]
        [SerializeField]
        private bool _highlightGrabTarget;
        [SerializeField, ShowIf("_highlightGrabTarget")]
        private EInteractable _highlightTag = EInteractable.Highlight;
        [SerializeField, ShowIf("_highlightGrabTarget")]
        private float _baseHighlightSize = 1.02f;
        [SerializeField, ShowIf("_highlightGrabTarget")]
        private float _grabHighlightSize = 1.05f;
        [SerializeField, ShowIf("_highlightGrabTarget")]
        private HighlightAction _highlightAction;

        [Space(10)]
        [SerializeField]
        private InputTrigger _grabButtonTrigger;
        [SerializeField]
        private bool _allowBufferedGrabbing;
        [Space(10)]
        [SerializeField]
        private InputTrigger _scaleButtonTrigger;

        [Header("Position")]
        [SerializeField]
        private FollowObjectPositionAction _followObjectPositionAction;

        [Header("Rotation")]
        [SerializeField]
        private FollowObjectRotationAction _followObjectRotationAction;

        [Header("Scale")]
        [SerializeField]
        private bool _2ControllerScale;
        [SerializeField, ShowIf("_2ControllerScale")]
        private DistanceProvider _distanceProvider;
        [SerializeField, ShowIf("_2ControllerScale")]
        private ScaleAction _scaleAction;

        private bool _grabbing;
        private bool _scaling;
        private bool _buttonDown;

        private void Reset()
        {
            if (_collisionTrigger == null)
            {
                _collisionTrigger = new CollisionTrigger();
            }
            _collisionTrigger.IsLast = true;
            _collisionTrigger.SaveLastCollision = true;

            _highlightGrabTarget = true;

            _grabButtonTrigger.StylusButton = StylusButton.Grip;
            _grabButtonTrigger.InputButtonEvent = EButtonEvent.OnButton;

            _scaleButtonTrigger.TrackedDeviceProvider.TrackedDevice = Handedness.NonDominant;
            _scaleButtonTrigger.StylusButton = StylusButton.Grip;
            _scaleButtonTrigger.InputButtonEvent = EButtonEvent.OnButton;

            _followObjectPositionAction.FollowTargetPosition = (EAxis) (-1);
            _followObjectPositionAction.PositionIsOffset = true;

            _followObjectRotationAction.FollowTargetRotation = (EAxis) (-1);
            _followObjectRotationAction.RotationIsOffset = true;
            _followObjectRotationAction.PivotAroundTarget = (EAxis) (-1);

            _2ControllerScale = true;
            _scaleAction.ScaleMultiplier = 5;
        }

        private void Update()
        {
            Transform targetToFollow = _grabButtonTrigger.TrackedDeviceProvider.GetOutput().transform;
            _followObjectPositionAction.TargetToFollow = targetToFollow;
            _followObjectRotationAction.TargetToFollow = targetToFollow;

            if (_highlightGrabTarget && !_grabbing)
            {
                if (_collisionTrigger.IsValid())
                {
                    if (_collisionTrigger.CollidedTransform.GetComponent<CollisionInteractable>().ContainsTag(_highlightTag))
                    {
                        _highlightAction.ObjectToHighlight = _collisionTrigger.CollidedTransform;
                        _highlightAction.SetCurrentHighlightOutline(_baseHighlightSize);
                    }
                }
                _highlightAction.Update(_collisionTrigger.IsValid());
            }


            if (_grabButtonTrigger.IsValid())
            {
                if (_collisionTrigger.IsValid() && !_grabbing && !(!_allowBufferedGrabbing && _buttonDown))
                {

                    _followObjectPositionAction.Follower = _collisionTrigger.CollidedTransform;
                    _followObjectRotationAction.Follower = _collisionTrigger.CollidedTransform;
                    _grabbing = true;

                    if (_highlightGrabTarget && _collisionTrigger.CollidedTransform.GetComponent<CollisionInteractable>().ContainsTag(EInteractable.Highlight))
                    {
                        _highlightAction.SetCurrentHighlightOutline(_grabHighlightSize);
                    }
                }

                _buttonDown = true;
            }
            else if (_buttonDown)
            {
                _buttonDown = false;
                _grabbing = false;
                if (_highlightGrabTarget)
                {
                    _highlightAction.SetCurrentHighlightOutline(_baseHighlightSize);
                }
            }

            if (_2ControllerScale)
            {
                if (!_scaling && _grabbing && _scaleButtonTrigger.IsValid())
                {
                    _scaling = true;
                    _scaleAction.ObjectToScale = _collisionTrigger.CollidedTransform;
                }
                else if ((!_scaleButtonTrigger.IsValid() || !_grabbing) && _scaling)
                {
                    _scaling = false;
                }

                if (_scaling)
                {
                    _scaleAction.ScaleValueX = _distanceProvider.GetOutput();
                    _scaleAction.ScaleValueY = _distanceProvider.GetOutput();
                    _scaleAction.ScaleValueZ = _distanceProvider.GetOutput();
                }

                _scaleAction.Update(_scaling);
            }

            _followObjectPositionAction.Update(_grabbing);
            _followObjectRotationAction.Update(_grabbing);
        }
    }
}
