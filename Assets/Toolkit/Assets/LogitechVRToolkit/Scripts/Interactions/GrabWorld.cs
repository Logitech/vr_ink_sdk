namespace Logitech.XRToolkit.Scripts.Interactions
{
    using Logitech.XRToolkit.Actions;
    using Logitech.XRToolkit.Providers;
    using Logitech.XRToolkit.Triggers;
    using Logitech.XRToolkit.Utils;
    using UnityEngine;

    /// <summary>
    /// Grabs a defined world object with a controller.
    /// </summary>
    /// <remarks>
    /// The world can be grabbed when a specified button is pressed and the specified collision is false (to prevent
    /// grabbing another object at the same time). The world can be further manipulated with a second controller.
    /// </remarks>
    public class GrabWorld : MonoBehaviour
    {
        [SerializeField]
        private Transform _world;

        [Header("Grab Options")]
        [SerializeField, Tooltip("Allow world grab if collision trigger is false")]
        private CollisionTrigger _collisionTrigger;
        [Space(10)]
        [SerializeField]
        private InputTrigger _grabButtonTrigger;
        [Space(10)]
        [SerializeField]
        private InputTrigger _rotateScaleAndMoveButtonTrigger;

        [Header("Position")]
        [SerializeField]
        private FollowObjectPositionAction _followObjectPositionAction;
        [SerializeField]
        bool _2ControllerPosition;
        [SerializeField, ShowIf("_2ControllerPosition")]
        private FollowTwoObjectsPositionAction _followTwoObjectPositionAction;

        [Header("Rotation")]
        [SerializeField]
        private FollowObjectRotationAction _followObjectRotationAction;
        [Space(10)]
        [SerializeField]
        private bool _2ControllerRotate;
        [SerializeField, ShowIf("_2ControllerRotate")]
        private AngleProvider2D _rotationAngleProvider;
        [SerializeField, ShowIf("_2ControllerRotate")]
        private RotateAction _rotateAction;

        [Header("Scale")]
        [SerializeField]
        private bool _2ControllerScale;
        [SerializeField, ShowIf("_2ControllerScale")]
        private DistanceProvider _distanceProvider;
        [SerializeField, ShowIf("_2ControllerScale")]
        private ScaleAction _scaleAction;

        private bool _grabButtonDown;
        private bool _grabbing;
        private bool _rotating;
        private bool _scaling;
        private bool _doubleGrabbing;

        private void Update()
        {
            _followObjectPositionAction.Follower = _world;
            _followTwoObjectPositionAction.Follower = _world;
            _followObjectRotationAction.Follower = _world;
            _rotateAction.ObjectToRotate = _world;
            _scaleAction.ObjectToScale = _world;

            Transform targetToFollow = _grabButtonTrigger.TrackedDeviceProvider.GetOutput().transform;
            _followObjectPositionAction.TargetToFollow = targetToFollow;
            _followObjectRotationAction.TargetToFollow = targetToFollow;

            Transform secondTargetToFollow = _rotateScaleAndMoveButtonTrigger.TrackedDeviceProvider.GetOutput().transform;
            _followTwoObjectPositionAction.FirstTargetToFollow = targetToFollow;
            _followTwoObjectPositionAction.SecondTargetToFollow = secondTargetToFollow;

            if (_grabButtonTrigger.IsValid())
            {
                if (!_collisionTrigger.IsValid() && !_grabbing && !_grabButtonDown)
                {
                    _grabbing = true;
                }

                _grabButtonDown = true;
            }
            else if (_grabButtonDown)
            {
                _grabButtonDown = false;
                _grabbing = false;
            }

            if (_2ControllerPosition)
            {
                if (!_doubleGrabbing && _grabbing && _rotateScaleAndMoveButtonTrigger.IsValid())
                {
                    _doubleGrabbing = true;
                }
                else if (!_rotateScaleAndMoveButtonTrigger.IsValid() && _doubleGrabbing)
                {
                    _doubleGrabbing = false;
                }
            }

            if (_2ControllerScale)
            {
                if (!_scaling && _grabbing && _grabButtonTrigger.IsValid() && _rotateScaleAndMoveButtonTrigger.IsValid())
                {
                    _scaling = true;
                }
                else if ((!_rotateScaleAndMoveButtonTrigger.IsValid()) && _scaling)
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

            if (_2ControllerRotate)
            {
                _followObjectRotationAction.FollowTargetRotation = 0;

                if (!_rotating && _grabbing && _grabButtonTrigger.IsValid() && _rotateScaleAndMoveButtonTrigger.IsValid())
                {
                    _rotating = true;
                    _rotationAngleProvider.Init();
                }
                else if ((!_rotateScaleAndMoveButtonTrigger.IsValid()) && _rotating)
                {
                    _rotating = false;
                }

                if (_rotating)
                {
                    _rotateAction.RotationValue = _rotationAngleProvider.GetOutput();
                }

                _rotateAction.Update(_rotating);
            }

            _followObjectPositionAction.Update(_grabbing);
            _followObjectRotationAction.Update(_rotating || _grabbing);
            _followTwoObjectPositionAction.Update(_doubleGrabbing);
        }
    }
}
