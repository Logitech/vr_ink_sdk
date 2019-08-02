namespace Logitech.XRToolkit.Components
{
    using System.Collections;
    using Logitech.XRToolkit.Utils;
    using UnityEngine;
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
        /// These events can be used to externally subscribe to UI events.
        /// </summary>
        public delegate void InteractionEvent();
        public event InteractionEvent OnPhysicalPressDown;
        public event InteractionEvent OnPhysicalPressUp;
        public event InteractionEvent OnButtonDown;
        public event InteractionEvent OnButtonUp;
        public event InteractionEvent OnHoverIn;
        public event InteractionEvent OnHoverOut;

        [HideInInspector]
        public bool OnPhysicalPress;
        [HideInInspector]
        public bool OnButton;
        [HideInInspector]
        public bool OnHover;

        protected Selectable UIElement;
        [Tooltip("Allow raycast button interaction.")]
        public bool RaycastInteraction = true;
        [Tooltip("Allow physical interaction.")]
        public bool PhysicalInteraction = true;
        [Tooltip("Allow buffering the press button before hover in?")]
        [SerializeField]
        private bool _allowPressDownBeforeHoverIn = true;

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
        [Tooltip("The distance per second that the button will move when raising or returning to its resting position.")]
        [SerializeField]
        private float _animationSpeed = 100f;
        [Tooltip("The animation speed multiplier for clicking down.")]
        [SerializeField, ShowIf("RaycastInteraction")]
        private float _animationSpeedMultiplaier = 1;

        private Coroutine _coroutine;

        private PointerEventData _pointerEventData;

        private void Start()
        {
            _pointerEventData = new PointerEventData(EventSystem.current);
            _restingPosition = _3DButton.localPosition;
        }

        public void HoverIn(bool pressed)
        {
            if (RaycastInteraction)
            {
                if (OnHoverIn != null)
                {
                    OnHoverIn();
                }

                OnHover = true;
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

                OnHoverInEvent(pressed);
            }
        }

        protected virtual void OnHoverInEvent(bool pressed) { }

        public void Hover(RaycastHit hit, Transform pushPoint, bool isColliding)
        {
            if (PhysicalInteraction)
            {
                if (!_snapped && isColliding)
                {
                    _snapped = true;
                    if (_coroutine != null)
                    {
                        StopCoroutine(_coroutine);
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
            }

            if (PhysicalInteraction || RaycastInteraction)
            {
                OnHoverEvent(hit, pushPoint, isColliding);
            }
        }

        protected virtual void OnHoverEvent(RaycastHit hit, Transform raycastOrigin, bool isColliding) { }

        public void HoverOut()
        {
            if (RaycastInteraction)
            {
                if (OnHoverOut != null)
                {
                    OnHoverOut();
                }

                OnHover = false;
                if (UIElement != null)
                {
                    UIElement.OnPointerExit(_pointerEventData);
                }

                if (OnButton)
                {
                    if (OnButtonUp != null)
                    {
                        OnButtonUp();
                    }

                    OnButton = false;
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
            if (RaycastInteraction)
            {
                if (OnButtonDown != null)
                {
                    OnButtonDown();
                }

                OnButton = true;
                if (UIElement != null)
                {
                    UIElement.OnPointerDown(_pointerEventData);
                }

                MoveUIElementToZPosition(_restingPosition.z + _maxPressDistance, _animationSpeed * _animationSpeedMultiplaier);
                OnButtonDownEvent();
            }
        }

        protected virtual void OnButtonDownEvent() { }

        public void Button(RaycastHit hit)
        {
            if (RaycastInteraction && OnButton)
            {
                OnButtonEvent(hit);
            }
        }

        protected virtual void OnButtonEvent(RaycastHit hit) { }

        public void ButtonUp()
        {
            if (RaycastInteraction)
            {
                if (OnButton)
                {
                    if (OnButtonUp != null)
                    {
                        OnButtonUp();
                    }

                    OnButton = false;
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
                if (OnPhysicalPressDown != null)
                {
                    OnPhysicalPressDown();
                }

                OnPhysicalPress = true;
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

            if (OnPhysicalPressUp != null)
            {
                OnPhysicalPressUp();
            }

            OnPhysicalPress = false;
            if (UIElement != null && !OnButton)
            {
                UIElement.OnPointerUp(_pointerEventData);
            }
        }

        private void OffsetUIElement()
        {
            if (_coroutine != null)
            {
                StopCoroutine(_coroutine);
            }

            _coroutine = StartCoroutine(MoveToLocalZPosition(_3DButton, _restingPosition.z + _highlightRaiseOffset, _animationSpeed));
        }

        private void MoveUIElementToZPosition(float zPosition, float animationSpeed)
        {
            if (_coroutine != null)
            {
                StopCoroutine(_coroutine);
            }

            if (gameObject.activeInHierarchy && _3DButton.gameObject.activeSelf)
            {
                _coroutine = StartCoroutine(MoveToLocalZPosition(_3DButton, zPosition, animationSpeed));
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
