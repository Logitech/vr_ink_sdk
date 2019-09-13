namespace Logitech.XRToolkit.Components
{
    using Logitech.XRToolkit.Utils;
    using System;
    using System.Collections;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    /// <summary>
    /// Base class for Unity UI elements that can receive hover and raycast button events. Elements may also be
    /// physically pushed.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public abstract class UIInteractable : Interactable
    {
        /// <summary>
        /// These events can be used to subscribe to UI events in the Inspector.
        /// </summary>
        [Serializable]
        private struct UnityEvents
        {
            public UnityEvent OnPressDown;
            public UnityEvent OnPressUp;
            public UnityEvent OnPhysicalPressDown;
            public UnityEvent OnPhysicalPressUp;
            public UnityEvent OnControllerButtonDown;
            public UnityEvent OnControllerButtonUp;
            public UnityEvent OnHoverIn;
            public UnityEvent OnHoverOut;
        }

        /// <summary>
        /// These events can be used to externally subscribe to UI events.
        /// </summary>
        public delegate void InteractionEvent();
        public event InteractionEvent OnPressDown;
        public event InteractionEvent OnPressUp;
        public event InteractionEvent OnPhysicalPressDown;
        public event InteractionEvent OnPhysicalPressUp;
        public event InteractionEvent OnControllerButtonDown;
        public event InteractionEvent OnControllerButtonUp;
        public event InteractionEvent OnHoverIn;
        public event InteractionEvent OnHoverOut;

        // If the UI element is pressed.
        [HideInInspector]
        public bool OnPress;
        // If the UI element is physically pressed.
        [HideInInspector]
        public bool OnPhysicalPress;
        // If the UI element is pressed with a controller button.
        [HideInInspector]
        public bool OnControllerButton;
        // If the UI element is hovered.
        [HideInInspector]
        public bool OnHover;

        [Tooltip("Unity events for usage in the inspector.")]
        [SerializeField]
        private UnityEvents _unityEvents;

        protected Selectable UIElement;
        [Tooltip("Allow raycast hover events and controller button interaction.")]
        public bool RaycastInteraction = true;
        [Tooltip("Allow physical interaction.")]
        public bool PhysicalInteraction = true;
        [Tooltip("Allow pressing the controller button before hover in?")]
        [SerializeField]
        private bool _allowPressDownBeforeHoverIn = true;
        [Tooltip("Allow controller button presses while physically pressing the UI, and physical presses while pressing the controller button.")]
        [SerializeField]
        private bool _allowSimultaneousButtonAndPhysicalPress = false;

        // Required.
        [Space(10)]
        [Tooltip("The object that will raise on hover, and can be physically pushed; it does not have to be the collider we are interacting with. The below values are based on its initial local position.")]
        [SerializeField]
        private Transform _3DButton;
        private Vector3 _restingPosition;
        private bool _snapped;

        [Tooltip("The distance the button will raise from its resting position when hovered.")]
        [SerializeField]
        private float _highlightRaiseOffset = -10f;
        [Tooltip("The the max distance the button can be pressed from its resting position.")]
        [SerializeField]
        private float _maxPressDistance = 5f;
        [Tooltip("The distance the button must reach from its resting position to invoke a click.")]
        [SerializeField, ShowIf("PhysicalInteraction")]
        private float _pressDistanceForClick;
        [Tooltip("The local distance per second that the button will move when raising or returning to its resting position.")]
        [SerializeField]
        private float _animationSpeed = 500f;
        [Tooltip("The local distance per second that the button will move down when raycast clicking.")]
        [SerializeField, ShowIf("RaycastInteraction")]
        private float _clickAnimationSpeed = 500f;

        private Coroutine _animationCoroutine;
        private PointerEventData _pointerEventData;

        private void Start()
        {
            _pointerEventData = new PointerEventData(EventSystem.current);
            _restingPosition = _3DButton.localPosition;

            OnPressDown += () => OnPress = true;
            OnPressUp += () => OnPress = false;

            OnPhysicalPressDown += () => OnPressDown();
            OnPhysicalPressUp += () => OnPressUp();
            OnControllerButtonDown += () => OnPressDown();
            OnControllerButtonUp += () => OnPressUp();

            OnPressDown += _unityEvents.OnPressDown.Invoke;
            OnPressUp += _unityEvents.OnPressUp.Invoke;
            OnPhysicalPressDown += _unityEvents.OnPhysicalPressDown.Invoke;
            OnPhysicalPressUp += _unityEvents.OnPhysicalPressUp.Invoke;
            OnControllerButtonDown += _unityEvents.OnControllerButtonDown.Invoke;
            OnControllerButtonUp += _unityEvents.OnControllerButtonUp.Invoke;
            OnHoverIn += _unityEvents.OnHoverIn.Invoke;
            OnHoverOut += _unityEvents.OnHoverOut.Invoke;
        }

        public void HoverIn(bool pressed)
        {
            if (RaycastInteraction)
            {
                OnHover = true;

                if (OnHoverIn != null)
                {
                    OnHoverIn();
                }

                if (UIElement != null)
                {
                    UIElement.OnPointerEnter(_pointerEventData);
                }

                if (pressed && _allowPressDownBeforeHoverIn)
                {
                    ButtonDown();
                }
                else
                {
                    OffsetUIElement();
                }

                OnHoverInEvent(pressed && _allowPressDownBeforeHoverIn);
            }
        }

        protected virtual void OnHoverInEvent(bool pressed) { }

        public void Hover(RaycastHit hit, Transform pushPoint, bool isColliding)
        {
            if (PhysicalInteraction)
            {
                if (!_allowSimultaneousButtonAndPhysicalPress && OnControllerButton)
                {
                    return;
                }

                if (!_snapped && isColliding)
                {
                    _snapped = true;
                    if (_animationCoroutine != null)
                    {
                        StopCoroutine(_animationCoroutine);
                    }
                }
                else if (_snapped && !isColliding)
                {
                    _snapped = false;
                    ReleasePhysicalClickState();
                    OffsetUIElement();
                }

                if (_snapped)
                {
                    PushUIElement(pushPoint);
                }

                OnHoverEvent(hit, pushPoint, isColliding);
                return;
            }

            if (RaycastInteraction)
            {
                OnHoverEvent(hit, pushPoint, false);
            }
        }

        protected virtual void OnHoverEvent(RaycastHit hit, Transform raycastOrigin, bool isColliding) { }

        public void HoverOut()
        {
            if (RaycastInteraction)
            {
                OnHover = false;

                if (OnHoverOut != null)
                {
                    OnHoverOut();
                }

                if (UIElement != null)
                {
                    UIElement.OnPointerExit(_pointerEventData);
                }

                if (OnControllerButton)
                {
                    OnControllerButton = false;

                    if (OnControllerButtonUp != null)
                    {
                        OnControllerButtonUp();
                    }

                    if (UIElement != null)
                    {
                        UIElement.OnPointerUp(_pointerEventData);
                    }
                }

                OnHoverOutEvent();
            }

            if (PhysicalInteraction || RaycastInteraction)
            {
                MoveUIElementToZPosition(_restingPosition.z, _animationSpeed);
            }

            if (PhysicalInteraction)
            {
                ReleasePhysicalClickState();
            }
        }

        protected virtual void OnHoverOutEvent() { }

        public void ButtonDown()
        {
            if (!RaycastInteraction)
            {
                return;
            }

            if (!_allowSimultaneousButtonAndPhysicalPress && OnPhysicalPress)
            {
                return;
            }

            OnControllerButton = true;

            if (OnControllerButtonDown != null)
            {
                OnControllerButtonDown();
            }

            if (UIElement != null)
            {
                UIElement.OnPointerDown(_pointerEventData);
            }

            MoveUIElementToZPosition(_restingPosition.z + _maxPressDistance, _clickAnimationSpeed);
            OnButtonDownEvent();
        }

        protected virtual void OnButtonDownEvent() { }

        public void Button(RaycastHit hit)
        {
            if (RaycastInteraction && OnControllerButton)
            {
                OnButtonEvent(hit);
            }
        }

        protected virtual void OnButtonEvent(RaycastHit hit) { }

        public void ButtonUp()
        {
            if (RaycastInteraction)
            {
                if (OnControllerButton)
                {
                    OnControllerButton = false;

                    if (OnControllerButtonUp != null)
                    {
                        OnControllerButtonUp();
                    }

                    if (UIElement != null && !OnPhysicalPress)
                    {
                        UIElement.OnPointerUp(_pointerEventData);
                    }
                }

                OffsetUIElement();
                OnButtonUpEvent();
            }
        }

        protected virtual void OnButtonUpEvent() { }

        private void PushUIElement(Transform pushPoint)
        {
            float adjustDistance = _3DButton.localPosition.z + _3DButton.InverseTransformPoint(pushPoint.position).z;

            if (adjustDistance > _restingPosition.z + _maxPressDistance)
            {
                _3DButton.localPosition = new Vector3(_3DButton.localPosition.x, _3DButton.localPosition.y, _restingPosition.z + _maxPressDistance);
            }
            else if (adjustDistance < _restingPosition.z + _highlightRaiseOffset)
            {
                _3DButton.localPosition = new Vector3(_3DButton.localPosition.x, _3DButton.localPosition.y, _restingPosition.z + _highlightRaiseOffset);
            }
            else
            {
                _3DButton.localPosition = new Vector3(_3DButton.localPosition.x, _3DButton.localPosition.y, adjustDistance);
            }

            SetPhysicalClickState(adjustDistance);
        }

        private void SetPhysicalClickState(float adjustDistance)
        {
            if (adjustDistance < _restingPosition.z + _pressDistanceForClick)
            {
                ReleasePhysicalClickState();
            }
            else if (!OnPhysicalPress && adjustDistance >= _restingPosition.z + _pressDistanceForClick)
            {
                OnPhysicalPress = true;

                if (OnPhysicalPressDown != null)
                {
                    OnPhysicalPressDown();
                }

                if (UIElement != null)
                {
                    UIElement.OnPointerDown(_pointerEventData);
                }
            }
        }

        private void ReleasePhysicalClickState()
        {
            if (!OnPhysicalPress)
            {
                return;
            }

            OnPhysicalPress = false;

            if (OnPhysicalPressUp != null)
            {
                OnPhysicalPressUp();
            }

            if (UIElement != null && !OnControllerButton)
            {
                UIElement.OnPointerUp(_pointerEventData);
            }
        }

        /// <summary>
        /// Raise the UI Element to show it can be interacted with.
        /// </summary>
        public void OffsetUIElement()
        {
            if (_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine);
            }

            _animationCoroutine = StartCoroutine(MoveToLocalZPosition(_3DButton, _restingPosition.z + _highlightRaiseOffset, _animationSpeed));
        }

        private void MoveUIElementToZPosition(float zPosition, float animationSpeed)
        {
            if (_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine);
            }

            if (gameObject.activeInHierarchy && _3DButton.gameObject.activeSelf)
            {
                _animationCoroutine = StartCoroutine(MoveToLocalZPosition(_3DButton, zPosition, animationSpeed));
            }
            else
            {
                _3DButton.localPosition = new Vector3(_3DButton.localPosition.x, _3DButton.localPosition.y, _restingPosition.z);
            }
        }

        private IEnumerator MoveToLocalZPosition(Transform targetTransform, float zDestination, float speed)
        {
            while (targetTransform.localPosition.z != zDestination)
            {
                targetTransform.localPosition = Vector3.MoveTowards(targetTransform.localPosition, new Vector3(_3DButton.localPosition.x, _3DButton.localPosition.y, zDestination), Time.deltaTime * speed);
                yield return null;
            }
        }
    }
}
