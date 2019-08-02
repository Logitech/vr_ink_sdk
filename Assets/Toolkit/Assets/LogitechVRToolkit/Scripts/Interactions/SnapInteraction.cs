namespace Logitech.XRToolkit.Interactions
{
    using Logitech.XRToolkit.Actions;
    using Logitech.XRToolkit.Components;
    using Logitech.XRToolkit.Providers;
    using Logitech.XRToolkit.Triggers;
    using Logitech.XRToolkit.Utils;
    using UnityEngine;

    /// <summary>
    /// Prevents the Stylus model from passing through a snappable surface, and otherwise sets the Stylus model position
    /// and rotation to the TrackedDevice position and rotation.
    /// </summary>
    public class SnapInteraction : MonoBehaviour
    {
        private RaycastHit _raycastHit;
        private Transform _controllerTransform;
        private Transform _3DTransform;
        private Vector3 _rayDirection;
        private bool _isSnapping;
        [SerializeField, Tooltip("Distance from the surface for the stylus to snap.")]
        private float _snapDistance = 0.001f;
        [SerializeField, Tooltip("Decrease this value to get the stylus to snap from further.")]
        private float _offset = -0.005f;
        [SerializeField]
        private Transform _stylusModelTransform;
        [SerializeField]
        private TrackedDeviceTransformProvider _trackedDevice;
        [SerializeField, HideInInspector]
        private RaycastTrigger _raycastTrigger;
        [SerializeField, EnumFlag]
        private EInteractable _snappingTag = EInteractable.Snappable;

        [Header("Haptics"), SerializeField]
        private bool _enableHaptics;
        [SerializeField, ShowIf("_enableHaptics")]
        private HapticAction _vibrationOnSnap;
        [SerializeField, ShowIf("_enableHaptics")]
        private EInteractable _hapticTaggedObjects = EInteractable.WritableSurface;

        // Update raycast Trigger tag on inspector change.
        private void OnValidate()
        {
            _raycastTrigger.Tag = _snappingTag;
        }

        void Update()
        {
            _controllerTransform = _trackedDevice.GetOutput();

            if (!_isSnapping)
            {
                _rayDirection = _controllerTransform.forward;
            }

            // The distance for snapping increases if the distance between the 3D model and the real device increases.
            float distance = Vector3.Distance(_stylusModelTransform.position, _controllerTransform.position);
            Vector3 rayOrigin = _controllerTransform.position;
            rayOrigin = rayOrigin + _rayDirection.normalized * (_offset - distance);

            Ray ray = new Ray(rayOrigin, _rayDirection);
            _raycastTrigger.RaycastRay = ray;

            if (_raycastTrigger.IsValid())
            {
                _raycastHit = _raycastTrigger.RaycastHit;

                float raycastOffset = Vector3.Distance(_controllerTransform.position, _raycastHit.point);

                Vector3 toOther = _raycastHit.point - _controllerTransform.position;
                float dotProduct = Vector3.Dot(_controllerTransform.forward, toOther);

                // This will tell us if we approach the snapping surface with the front of the back of the stylus.
                // Only snap when approaching with the tip of the stylus.
                raycastOffset = dotProduct >= 0 ? raycastOffset : -raycastOffset;

                if (raycastOffset <= _snapDistance)
                {
                    if (!_isSnapping && _raycastHit.transform.GetComponent<Interactable>().ContainsTag(_hapticTaggedObjects))
                    {
                        _vibrationOnSnap.TriggerOnce();
                    }
                    _isSnapping = true;

                    _stylusModelTransform.position = _raycastHit.point;
                    _rayDirection = -_raycastHit.normal;
                    _stylusModelTransform.rotation = _controllerTransform.rotation;

                    return;
                }
            }
            _isSnapping = false;

            _stylusModelTransform.position = _controllerTransform.position;
            _stylusModelTransform.rotation = _controllerTransform.rotation;
        }

        public bool IsSnapped()
        {
            return _isSnapping;
        }
    }
}
