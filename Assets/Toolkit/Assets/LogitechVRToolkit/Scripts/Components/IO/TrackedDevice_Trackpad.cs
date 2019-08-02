namespace Logitech.XRToolkit.IO
{
    using Logitech.XRToolkit.Utils;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    /// <summary>
    /// Handles custom touch input for the trackpad of a <see cref="TrackedDevice"/>.
    /// </summary>
    [RequireComponent(typeof(TrackedDevice))]
    public class TrackedDevice_Trackpad : MonoBehaviour, ITouchEvents
    {
        [Serializable]
        public struct TouchZone
        {
            public ETouchZone Zone;
            public Vector2 Center;
            public Vector2 Size;

            public TouchZone(ETouchZone zone, Vector2 center, Vector2 size)
            {
                Zone = zone;
                Center = center;
                Size = size;
            }
        }

        private enum ETapState
        {
            WaitingForTap,
            FirstTouchDown,
            Tapped,
            DoubleTapped
        }

        // Track current frame to avoid calling Update() more than once per frame.
        private int _updateFrame;

        private TrackedDevice _trackedDevice;
        [SerializeField]
        private bool _showAdvancedOptions;

        // Developer defined touch zones. Center of the trackpad is assumed to be 0,0.
        [SerializeField]
        private List<TouchZone> _touchZones = new List<TouchZone>()
        {
            new TouchZone(ETouchZone.BottomZone, new Vector2(0.0f, -0.25f), new Vector2(1, 0.5f)),
            new TouchZone(ETouchZone.MiddleZone, new Vector2(0.0f, 0.0f), new Vector2(1, 0.5f)),
            new TouchZone(ETouchZone.TopZone, new Vector2(0, 0.25f), new Vector2(1, 0.5f)),
            new TouchZone(ETouchZone.LeftZone, new Vector2(-0.25f, 0), new Vector2(0.5f, 1)),
            new TouchZone(ETouchZone.RightZone, new Vector2(0.25f, 0), new Vector2(0.5f, 1))
        };

        // Zones in on touch down that have not been exited.
        private readonly List<ETouchZone> _persistedOnTouchDownZones = new List<ETouchZone>();

        // Touch.
        private Vector2 _touchPosition = Vector2.zero;
        private Vector2 _initialTouchPosition = Vector2.zero;
        private Vector2 _previousTouchPosition = Vector2.zero;
        private Vector2 _initialTouchMoveDistance = Vector2.zero;
        private Vector2 _currentTouchMoveDistance = Vector2.zero;
        private float _touchStartTime = Mathf.Infinity;

        // Tap.
        [Space(10)]
        [SerializeField, ShowIf("_showAdvancedOptions")]
        private float _tapMoveDistanceLockout = 0.5f;
        [Tooltip("Time in seconds to touch up after touch to fire a tap event.")]
        [SerializeField, ShowIf("_showAdvancedOptions")]
        private float _timeToTap = 0.3f;
        [SerializeField]
        private bool _enableDoubleTap = true;
        [Tooltip("Time in seconds to touch down after first touch to fire a double tap event.")]
        [SerializeField, ShowIf("_showAdvancedOptions")]
        private float _timeToDoubleTap = 0.3f;
        [Tooltip("Prevent a tap event from firing at the same time as a double tap event.")]
        [SerializeField, ShowIf("_showAdvancedOptions")]
        private bool _blockTapEventOnDoubleTap = true;
        private float _firstTapTime;
        private float _secondTapTime;
        private ETapState _tapState;
        private bool _tapped;
        private bool _doubleTapped;

        // Swipe.
        [Space(10)]
        [SerializeField]
        private bool _enableXAxisSwipeGestures = true;
        [SerializeField]
        private bool _enableYAxisSwipeGestures = true;
        [Tooltip("Time in seconds to swipe and touch up after touch to fire a swipe gesture event.")]
        [SerializeField, ShowIf("_showAdvancedOptions")]
        private float _timeToSwipeGesture = 0.3f;
        [Tooltip("Distance moved on trackpad to beat to fire a swipe event.")]
        [SerializeField, ShowIf("_showAdvancedOptions")]
        private Vector2 _swipeDistanceThreshold = new Vector2(1f, 1f);
        [Tooltip("Trackpad size ratio is applied to the distance moved comparison between x and y when determining which axis has priority on a swipe gesture.")]
        [SerializeField, ShowIf("_showAdvancedOptions")]
        private Vector2 _trackpadSizeRatio = new Vector2(0.42f, 1f);
        private ESwipeDirection _swipeState;

        // Scroll.
        [Space(10)]
        [Tooltip("Touch distance from initial touch position required to move before Scroll() starts reporting values.")]
        [SerializeField, ShowIf("_showAdvancedOptions")]
        private float _scrollDistanceThreshold = 0.1f;
        private bool _canScroll;

        // Visual trackpad representation and touch location.
        [Header("Visual")]
        [SerializeField]
        private Transform _trackpadRepresentation;
        [SerializeField]
        private Transform _pointer;

        private void Awake()
        {
            _trackedDevice = GetComponent<TrackedDevice>();
        }

        /// <summary>
        /// Updates current device input states.
        /// </summary>
        /// /// <remarks>
        /// Update is also called by public methods in this class to ensure input states are current.
        /// </remarks>
        private void Update()
        {
            if (_updateFrame != Time.frameCount)
            {
                _updateFrame = Time.frameCount;
                UpdateTouchPosition();
                UpdateTapState();
                UpdateSwipeState();
            }
        }

        /// <summary>
        /// Updates touch position, zones on touch down, scroll and the visual representation if provided.
        /// </summary>
        private void UpdateTouchPosition()
        {
            if (_trackedDevice.GetButton(StylusButton.TouchstripTouch))
            {
                _touchPosition.x = _trackedDevice.GetAxis(StylusAxisInput.TrackpadX);
                _touchPosition.y = _trackedDevice.GetAxis(StylusAxisInput.TrackpadY);
                _currentTouchMoveDistance = _touchPosition - _previousTouchPosition;
                _previousTouchPosition = _touchPosition;
            }

            if (_trackedDevice.GetButtonDown(StylusButton.TouchstripTouch))
            {
                _touchStartTime = Time.time;
                _initialTouchPosition = _touchPosition;

                _persistedOnTouchDownZones.Clear();
                foreach (var zone in _touchZones)
                {
                    if (IsInZone(zone.Zone))
                    {
                        _persistedOnTouchDownZones.Add(zone.Zone);
                    }
                }
            }

            if (_trackedDevice.GetButton(StylusButton.TouchstripTouch))
            {
                _initialTouchMoveDistance = _touchPosition - _initialTouchPosition;

                if (!_canScroll && Vector2.Distance(_initialTouchPosition, _touchPosition) > _scrollDistanceThreshold)
                {
                    _canScroll = true;
                }

                // Show the touch position pointer.
                if (_pointer != null)
                {
                    _pointer.gameObject.SetActive(true);
                    _pointer.localPosition = new Vector2(_touchPosition.x / 2, _touchPosition.y / 2);
                }

                List<ETouchZone> zones = new List<ETouchZone>(_persistedOnTouchDownZones);
                foreach (var zone in zones)
                {
                    if (!IsInZone(zone))
                    {
                        _persistedOnTouchDownZones.Remove(zone);
                    }
                }
            }
            else
            {
                // Hide the touch position pointer.
                if (_pointer != null)
                {
                    _pointer.gameObject.SetActive(false);
                }
            }

            if (_trackedDevice.GetButtonUp(StylusButton.TouchstripTouch))
            {
                _canScroll = false;
            }
        }

        /// <summary>
        /// Updates trackpad tap state for this frame.
        /// </summary>
        private void UpdateTapState()
        {
            _tapped = false;
            _doubleTapped = false;

            switch (_tapState)
            {
                case ETapState.WaitingForTap:
                {
                    if (_trackedDevice.GetButtonDown(StylusButton.TouchstripTouch))
                    {
                        _tapState = ETapState.FirstTouchDown;
                        _firstTapTime = _touchStartTime;
                    }

                    break;
                }
                case ETapState.FirstTouchDown:
                {
                    if ((Time.time - _firstTapTime) > _timeToTap)
                    {
                        _tapState = ETapState.WaitingForTap;
                        break;
                    }

                    if (_trackedDevice.GetButton(StylusButton.TouchstripTouch))
                    {
                        if (Vector2.Distance(_initialTouchPosition, _touchPosition) > _tapMoveDistanceLockout)
                        {
                            _tapState = ETapState.WaitingForTap;
                            break;
                        }
                    }

                    if (_trackedDevice.GetButtonUp(StylusButton.TouchstripTouch))
                    {
                        _tapped = true;
                        _tapState = _enableDoubleTap ? ETapState.Tapped : ETapState.WaitingForTap;
                    }

                    break;
                }
                case ETapState.Tapped:
                {
                    if ((Time.time - _firstTapTime) > _timeToDoubleTap)
                    {
                        _tapState = ETapState.WaitingForTap;
                        break;
                    }

                    if (_trackedDevice.GetButtonDown(StylusButton.TouchstripTouch))
                    {
                        _doubleTapped = true;

                        if (!_blockTapEventOnDoubleTap)
                        {
                            _tapState = ETapState.DoubleTapped;
                            _secondTapTime = _touchStartTime;
                        }
                        else
                        {
                            _tapState = ETapState.WaitingForTap;
                        }
                    }

                    break;
                }
                case ETapState.DoubleTapped:
                {
                    if ((Time.time - _secondTapTime) > _timeToTap)
                    {
                        _tapState = ETapState.WaitingForTap;
                        break;
                    }

                    if (_trackedDevice.GetButton(StylusButton.TouchstripTouch))
                    {
                        if (Vector2.Distance(_initialTouchPosition, _touchPosition) > _tapMoveDistanceLockout)
                        {
                            _tapState = ETapState.WaitingForTap;
                            break;
                        }
                    }

                    if (_trackedDevice.GetButtonUp(StylusButton.TouchstripTouch))
                    {
                        _tapped = true;
                        _tapState = ETapState.WaitingForTap;
                    }

                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Updates the current swipe gesture for the frame. Must stop touching the trackpad to complete the gesture.
        /// </summary>
        private void UpdateSwipeState()
        {
            if (_trackedDevice.GetButtonUp(StylusButton.TouchstripTouch))
            {
                if ((Time.time - _touchStartTime) <= _timeToSwipeGesture)
                {
                    _swipeState = GetSwipeDirection(_initialTouchMoveDistance);
                    return;
                }
            }

            _swipeState = ESwipeDirection.None;
        }

        /// <summary>
        /// Returns the first swipe gesture to beat the threshold, preferring the axis that moved the most.
        /// </summary>
        private ESwipeDirection GetSwipeDirection(Vector2 distance)
        {
            bool xBeatThreshold = Mathf.Abs(distance.x) > _swipeDistanceThreshold.x;
            bool yBeatThreshold = Mathf.Abs(distance.y) > _swipeDistanceThreshold.y;
            bool xLargerThanY = Math.Abs(distance.x * _trackpadSizeRatio.x) >
                                Math.Abs(distance.y * _trackpadSizeRatio.y);

            if (xLargerThanY)
            {
                if (_enableXAxisSwipeGestures && xBeatThreshold)
                {
                    return distance.x > 0 ? ESwipeDirection.Right : ESwipeDirection.Left;
                }

                if (_enableYAxisSwipeGestures && yBeatThreshold)
                {
                    return distance.y > 0 ? ESwipeDirection.Forward : ESwipeDirection.Backward;
                }
            }
            else
            {
                if (_enableYAxisSwipeGestures && yBeatThreshold)
                {
                    return distance.y > 0 ? ESwipeDirection.Forward : ESwipeDirection.Backward;
                }

                if (_enableXAxisSwipeGestures && xBeatThreshold)
                {
                    return distance.x > 0 ? ESwipeDirection.Right : ESwipeDirection.Left;
                }
            }

            return ESwipeDirection.None;
        }

        /// <summary>
        /// Checks if the trackpad has been tapped.
        /// </summary>
        /// <returns>
        /// If the trackpad has been tapped.
        /// </returns>
        public bool OnTap()
        {
            Update();
            return _tapped;
        }

        /// <summary>
        /// Checks if the trackpad has been double tapped.
        /// </summary>
        /// <returns>
        /// If the trackpad has been double tapped.
        /// </returns>
        public bool OnDoubleTap()
        {
            Update();
            return _doubleTapped;
        }

        /// <summary>
        /// Checks if the touch position is in the specified touch zone.
        /// </summary>
        /// <param name="touchZone">The queried touch zone.</param>
        /// <returns>
        /// If the touch position is in the specified touch zone.
        /// </returns>
        public bool IsInZone(ETouchZone touchZone)
        {
            // Return false if not currently touching the trackpad.
            if (!_trackedDevice.GetButton(StylusButton.TouchstripTouch) &&
                !_trackedDevice.GetButtonUp(StylusButton.TouchstripTouch))
            {
                return false;
            }

            Update();

            TouchZone zone = _touchZones.First(x => x.Zone == touchZone);

            if (!_touchZones.Select(x => x.Zone).Contains(touchZone))
            {
                return false;
            }

            float maxX = (zone.Center.x * 2) + (zone.Size.x);
            float minX = (zone.Center.x * 2) - (zone.Size.x);
            float maxY = (zone.Center.y * 2) + (zone.Size.y);
            float minY = (zone.Center.y * 2) - (zone.Size.y);

            return (_touchPosition.x <= maxX
                    && _touchPosition.x >= minX
                    && _touchPosition.y <= maxY
                    && _touchPosition.y >= minY);
        }

        /// <summary>
        /// Checks if specified touch zones have been exited.
        /// </summary>
        /// <param name="requireAllTrue">Should all touch zones specified have never been exited?</param>
        /// <param name="zones">The queried touch zones.</param>
        /// <returns>
        /// True if last touch has always been in the same specified zones that were in OnTouchDown().
        /// </returns>
        public bool TouchRemainedInZone(bool requireAllTrue, params ETouchZone[] zones)
        {
            // Return false if not currently touching the trackpad.
            if (!_trackedDevice.GetButton(StylusButton.TouchstripTouch) &&
                !_trackedDevice.GetButtonUp(StylusButton.TouchstripTouch))
            {
                return false;
            }

            Update();

            return requireAllTrue
                ? zones.All(zone => _persistedOnTouchDownZones.Contains(zone))
                : zones.Any(zone => _persistedOnTouchDownZones.Contains(zone));
        }

        /// <summary>
        /// Checks if the trackpad has been swiped in a direction.
        /// </summary>
        /// <param name="swipeDirection">The swipe direction to check against.</param>
        /// <returns>
        /// If the tracked has been swiped in the specified direction.
        /// </returns>
        public bool OnSwipe(ESwipeDirection swipeDirection)
        {
            Update();
            return _swipeState == swipeDirection;
        }

        /// <summary>
        /// Checks if the trackpad has been swiped.
        /// </summary>
        /// <returns>
        /// The swipe direction.
        /// </returns>
        public ESwipeDirection OnSwipe()
        {
            Update();
            return _swipeState;
        }

        /// <summary>
        /// Gets the current scroll value of the trackpad.
        /// </summary>
        /// <returns>
        /// The distance moved in X and Y from the last frame.
        /// </returns>
        public Vector2 Scroll()
        {
            Update();
            return _canScroll ? _currentTouchMoveDistance : Vector2.zero;
        }

        /// <summary>
        /// Draw gizmo lines in the scene view for a visual representation of the touch zones, if a _trackpadRepresentation is provided.
        /// </summary>
        private void OnDrawGizmos()
        {
            if (_trackpadRepresentation != null)
            {
                Gizmos.color = Color.green;
                foreach (var zone in _touchZones)
                {
                    Vector3 bottomLeftLocalPosition = new Vector3(
                        zone.Center.x - (zone.Size.x / 2),
                        zone.Center.y - (zone.Size.y / 2),
                        0);
                    Vector3 bottomRightLocalPosition = new Vector3(
                        zone.Center.x + (zone.Size.x / 2),
                        zone.Center.y - (zone.Size.y / 2),
                        0);
                    Vector3 topLeftLocalPosition = new Vector3(
                        zone.Center.x - (zone.Size.x / 2),
                        zone.Center.y + (zone.Size.y / 2),
                        0);
                    Vector3 topRightLocalPosition = new Vector3(
                        zone.Center.x + (zone.Size.x / 2),
                        zone.Center.y + (zone.Size.y / 2),
                        0);
                    Vector3 bottomLeftGlobalPosition = _trackpadRepresentation.TransformPoint(bottomLeftLocalPosition);
                    Vector3 bottomRightGlobalPosition =
                        _trackpadRepresentation.TransformPoint(bottomRightLocalPosition);
                    Vector3 topLeftGlobalPosition = _trackpadRepresentation.TransformPoint(topLeftLocalPosition);
                    Vector3 topRightGlobalPosition = _trackpadRepresentation.TransformPoint(topRightLocalPosition);

                    Gizmos.DrawLine(bottomLeftGlobalPosition, bottomRightGlobalPosition);
                    Gizmos.DrawLine(bottomRightGlobalPosition, topRightGlobalPosition);
                    Gizmos.DrawLine(topRightGlobalPosition, topLeftGlobalPosition);
                    Gizmos.DrawLine(topLeftGlobalPosition, bottomLeftGlobalPosition);
                }
            }
        }
    }
}
