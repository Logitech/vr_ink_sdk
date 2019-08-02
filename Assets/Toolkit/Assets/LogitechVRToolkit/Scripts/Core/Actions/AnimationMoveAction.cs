namespace Logitech.XRToolkit.Actions
{
    using Action = Logitech.XRToolkit.Core.Action;
    using Logitech.XRToolkit.Utils;
    using System;
    using UnityEngine;

    /// <summary>
    /// Moves a transform to another transform over a period of time.
    /// </summary>
    [Serializable]
    public class AnimationMoveAction : Action
    {
        [SerializeField]
        public Transform ObjectToMove;
        [SerializeField]
        public Transform FinalDestination;

        [SerializeField]
        public bool EnableTranslation = true;
        [SerializeField, ShowIf("EnableTranslation")]
        public float TranslationSpeedRatio = 1f;

        [SerializeField]
        public bool EnableRotation = true;
        [SerializeField, ShowIf("EnableRotation")]
        public float RotationSpeedRatio = 1f;

        [SerializeField]
        public bool EnableScaling = true;
        [SerializeField, ShowIf("EnableScaling")]
        public float ScaleSpeedRatio = 1f;

        [SerializeField]
        public bool MoveAfterReachingDestination;

        private bool _freeToMove = true;
        private bool _freeToRotate = true;
        private bool _freeToScale = true;

        /// <summary>
        /// When triggering the action, enable the desired movement.
        /// </summary>
        protected override void OnTriggerValid()
        {
            _freeToMove = EnableTranslation;
            _freeToRotate = EnableRotation;
            _freeToScale = EnableScaling;
        }

        protected override void TriggerValid()
        {
            if (_freeToMove)
            {
                ObjectToMove.position = Vector3.Lerp(ObjectToMove.position, FinalDestination.position,
                    0.05f * TranslationSpeedRatio);
                float distance = Vector3.Distance(FinalDestination.position, ObjectToMove.position);
                if (distance < 0.001f && !MoveAfterReachingDestination)
                {
                    _freeToMove = false;
                }
            }

            if (_freeToRotate)
            {
                ObjectToMove.rotation = Quaternion.Lerp(ObjectToMove.rotation, FinalDestination.rotation,
                    0.07f * RotationSpeedRatio);
                float angle = Quaternion.Angle(ObjectToMove.rotation, FinalDestination.rotation);
                if (angle < 1f && !MoveAfterReachingDestination)
                {
                    _freeToRotate = false;
                }
            }

            if (_freeToScale)
            {
                ObjectToMove.localScale = Vector3.Lerp(ObjectToMove.localScale, FinalDestination.localScale,
                    0.07f * ScaleSpeedRatio);
                float sizeDiff = Vector3.Distance(FinalDestination.localScale, ObjectToMove.localScale);
                if (sizeDiff < 0.001f && !MoveAfterReachingDestination)
                {
                    _freeToScale = false;
                }
            }
        }

        protected override void OnTriggerInvalid()
        {
            _freeToMove = false;
            _freeToRotate = false;
            _freeToScale = false;
        }
    }
}
