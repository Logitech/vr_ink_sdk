namespace Logitech.XRToolkit.Components
{
    using Logitech.XRToolkit.Utils;
    using UnityEngine;

    /// <summary>
    /// A control point that can be moved around a plane. Receives hover and button events which allows physical and
    /// raycast button interaction.
    /// </summary>
    public class ControlPointInteractable : UIInteractable
    {
        [SerializeField]
        private Transform _controlPoint;
        private Vector3 _toFollow;
        private bool _isFollowing;

        void Reset()
        {
            Tags = EInteractable.UIInteractable;
        }

        private void Awake()
        {
            _toFollow = _controlPoint.position;
        }

        protected override void OnHoverEvent(RaycastHit hit, Transform pushPoint, bool isCollding)
        {
            if (!OnPhysicalPress)
            {
                return;
            }
            _toFollow = hit.point;
        }

        protected override void OnButtonEvent(RaycastHit hit)
        {
            _toFollow = hit.point;
        }

        // TODO Move this to OnHoverEvent?
        private void LateUpdate()
        {
            if (_isFollowing || OnPhysicalPress)
            {
                _controlPoint.position = _toFollow;
            }
        }

        protected override void OnButtonDownEvent()
        {
            _isFollowing = true;
        }

        protected override void OnButtonUpEvent()
        {
            _isFollowing = false;
        }

        protected override void OnHoverOutEvent()
        {
            _isFollowing = false;
        }
    }
}
