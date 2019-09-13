namespace Logitech.XRToolkit.Interactions
{
    using Logitech.XRToolkit.Components;
    using Logitech.XRToolkit.Providers;
    using Logitech.XRToolkit.Triggers;
    using Logitech.XRToolkit.Utils;
    using Logitech.XRToolkit.IO;
    using System.Linq;
    using UnityEngine;

    /// <summary>
    /// Looks for UIInteractables, and passes hover and button events from a <see cref="TrackedDevice"/> to them.
    /// </summary>
    public class RaycastClicker : MonoBehaviour
    {
        [HideInInspector]
        public bool IsRaycasting;

        [SerializeField]
        private TrackedDeviceProvider _trackedDevice;
        [SerializeField]
        private StylusButton _stylusButton;
        private InputTrigger _onButtonDownTrigger, _onButtonTrigger, _onButtonUpTrigger;

        [SerializeField]
        private RaycastTrigger _raycastTrigger;
        [SerializeField]
        private RaycastTrigger _raycastCollisionTrigger;
        [SerializeField]
        private Material _raycastMaterial;

        [SerializeField]
        private PropertyStateTrigger[] _blockRaycastTriggers;

        private UIInteractable _hoveredElement;
        private LineRenderer _line;

        private void Awake()
        {
            _line = SetLineRenderer();

            _onButtonDownTrigger = new InputTrigger(_trackedDevice, _stylusButton, EButtonEvent.OnButtonDown);
            _onButtonTrigger = new InputTrigger(_trackedDevice, _stylusButton, EButtonEvent.OnButton);
            _onButtonUpTrigger = new InputTrigger(_trackedDevice, _stylusButton, EButtonEvent.OnButtonUp);
        }

        private void LateUpdate()
        {
            bool block = _blockRaycastTriggers.Any(propertyStateTrigger => propertyStateTrigger.IsValid());

            if (!block && _raycastTrigger.IsValid() && _raycastTrigger.RaycastHit.collider.gameObject.GetComponent<UIInteractable>() != null)
            {
                IsRaycasting = true;
                RaycastHit hit = _raycastTrigger.RaycastHit;
                UIInteractable newHoveredElement = hit.collider.gameObject.GetComponent<UIInteractable>();

                // Did we hover a new element?
                if (newHoveredElement != _hoveredElement)
                {
                    // Hover out on the previous hovered element.
                    if (_hoveredElement != null)
                    {
                        _hoveredElement.HoverOut();
                    }

                    if (newHoveredElement.RaycastInteraction)
                    {
                        _line.gameObject.SetActive(true);
                    }
                    else
                    {
                        _line.gameObject.SetActive(false);
                    }

                    _hoveredElement = newHoveredElement;
                    Debug.Assert(_hoveredElement != null, "We asked for colliders attached to game objects with ClickableInteractable and now we can't find it?");

                    _hoveredElement.HoverIn(_onButtonTrigger.IsValid());
                }

                if (_raycastCollisionTrigger.IsValid())
                {
                    _hoveredElement.Hover(_raycastCollisionTrigger.RaycastHit, transform, true);
                }
                else
                {
                    _hoveredElement.Hover(_raycastCollisionTrigger.RaycastHit, transform, false);
                }

                // Update ray.
                Vector3[] positions = { transform.position, transform.position + transform.forward * (hit.distance + _raycastTrigger.Offset) };
                _line.SetPositions(positions);

                // Pass button events.
                if (_onButtonDownTrigger.IsValid())
                {
                    _hoveredElement.ButtonDown();
                }
                if (_onButtonTrigger.IsValid())
                {
                    _hoveredElement.Button(hit);
                }
                if (_onButtonUpTrigger.IsValid())
                {
                    _hoveredElement.ButtonUp();
                }
            }
            else
            {
                IsRaycasting = false;

                // Did we just leave an element?
                if (_hoveredElement != null)
                {
                    _line.gameObject.SetActive(false);
                    _hoveredElement.HoverOut();
                    _hoveredElement = null;
                }
            }
        }

        private LineRenderer SetLineRenderer()
        {
            var line = new GameObject("Menu raycast line").AddComponent(typeof(LineRenderer)) as LineRenderer;
            line.material = _raycastMaterial;
            line.positionCount = 2;
            line.widthCurve = AnimationCurve.EaseInOut(0f, 0.002f, 1f, 0.0005f);
            line.startColor = new Color(1f, 1f, 1f, 1f);
            line.endColor = new Color(1f, 1f, 1f, 0.5f);
            line.gameObject.SetActive(false);
            return line;
        }

        private void OnValidate()
        {
            _onButtonDownTrigger = new InputTrigger(_trackedDevice, _stylusButton, EButtonEvent.OnButtonDown);
            _onButtonTrigger = new InputTrigger(_trackedDevice, _stylusButton, EButtonEvent.OnButton);
            _onButtonUpTrigger = new InputTrigger(_trackedDevice, _stylusButton, EButtonEvent.OnButtonUp);
        }
    }
}
