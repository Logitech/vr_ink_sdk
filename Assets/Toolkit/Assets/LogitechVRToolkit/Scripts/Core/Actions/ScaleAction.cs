namespace Logitech.XRToolkit.Actions
{
    using Action = Logitech.XRToolkit.Core.Action;
    using System;
    using UnityEngine;

    /// <summary>
    /// Scales an object (additive, adjustment, override).
    /// </summary>
    [Serializable]
    public class ScaleAction : Action
    {
        public enum ObjectScaleType
        {
            AdjustmentFromStart,
            Additive,
            Set
        }

        [HideInInspector]
        public Transform ObjectToScale;
        private Transform _previousObjectToScale;

        public ObjectScaleType ScaleType;
        public float ScaleMultiplier = 1;

        // Scale Adjustment.
        [HideInInspector]
        public float ScaleValueX;
        [HideInInspector]
        public float ScaleValueY;
        [HideInInspector]
        public float ScaleValueZ;

        // Scale limits.
        [SerializeField]
        private bool _useScaleLimit = true;
        [SerializeField]
        private float _scaleMinimalRatio = 0.5f;
        [SerializeField]
        private float _scaleMaximalRatio = 1.5f;

        private Vector3 _cappedScaleValue;
        private float _diffDistanceValue;
        private float _initialXValue = 0;

        protected override void OnTriggerValid()
        {
            if (ObjectToScale == null)
            {
                throw new ArgumentNullException(ObjectToScale.name);
            }
            if (_previousObjectToScale != ObjectToScale && _initialXValue == 0)
            {
                _initialXValue = ObjectToScale.localScale.x;
                _previousObjectToScale = ObjectToScale;
            }
            if (ScaleType == ObjectScaleType.AdjustmentFromStart)
            {
                _diffDistanceValue = Mathf.Max(ScaleValueX, ScaleValueY, ScaleValueZ) * ScaleMultiplier;
            }
        }

        protected override void TriggerValid()
        {
            if (ObjectToScale == null)
            {
                throw new ArgumentNullException(ObjectToScale.name);
            }

            ScaleObject();
        }

        private void ScaleObject()
        {
            Vector3 objectScale;

            switch (ScaleType)
            {
                case ObjectScaleType.AdjustmentFromStart:
                    {
                        objectScale = new Vector3(
                            ObjectToScale.localScale.x + (ScaleValueX * ScaleMultiplier - _diffDistanceValue),
                            ObjectToScale.localScale.y + (ScaleValueY * ScaleMultiplier - _diffDistanceValue),
                            ObjectToScale.localScale.z + (ScaleValueZ * ScaleMultiplier - _diffDistanceValue));
                        if (IsNewScaleInRange(objectScale.x))
                        {
                            ObjectToScale.localScale = objectScale;
                        }
                        _diffDistanceValue = Mathf.Max(ScaleValueX, ScaleValueY, ScaleValueZ) * ScaleMultiplier;
                        break;
                    }
                case ObjectScaleType.Additive:
                    {
                        objectScale = new Vector3(
                            ObjectToScale.localScale.x + ScaleValueX * ScaleMultiplier,
                            ObjectToScale.localScale.y + ScaleValueY * ScaleMultiplier,
                            ObjectToScale.localScale.z + ScaleValueZ * ScaleMultiplier);
                        if (IsNewScaleInRange(objectScale.x))
                        {
                            ObjectToScale.localScale = objectScale;
                        }
                        break;
                    }
                case ObjectScaleType.Set:
                    {
                        if (IsNewScaleInRange(ScaleValueX * ScaleMultiplier))
                        {
                            ObjectToScale.localScale = new Vector3(ScaleValueX * ScaleMultiplier, ScaleValueY * ScaleMultiplier, ScaleValueZ * ScaleMultiplier);
                        }
                        break;
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private bool IsNewScaleInRange(float newXValue)
        {
            return !(newXValue <= _initialXValue * _scaleMinimalRatio || newXValue >= _initialXValue * _scaleMaximalRatio) || !_useScaleLimit;
        }
    }
}
