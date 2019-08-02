namespace Logitech.XRToolkit.Inking
{
    using System;
    using System.Diagnostics;
    using UnityEngine;

    [Serializable]
    public class DrawingModifier
    {
        [SerializeField]
        private Stopwatch _timer;
        [SerializeField]
        private DrawingModificationProperties _brushProperties;

        private Transform _stylusTransform;
        private Transform _surfaceTransform;

        public DrawingModifier(DrawingModificationProperties brushProperties, Transform stylusTransform,
          Transform surfaceTransform)
        {
            _brushProperties = brushProperties;
            _timer = Stopwatch.StartNew();
            _stylusTransform = stylusTransform;
            _surfaceTransform = surfaceTransform;
        }

        public void UpdateProperties(DrawingModificationProperties brushProperties)
        {
            _brushProperties = brushProperties;
        }

        public float CalculateScalarSize(float brushSize, float scalarValue)
        {
            return brushSize * scalarValue;
        }

        public float CalculateVelocitySize(float brushSize, float averageSpeed)
        {
            float newBrushRatio;
            try
            {
                var maxWidthVelocity = _brushProperties.MaxWidthVelocity;
                newBrushRatio = averageSpeed / maxWidthVelocity;
            }
            catch (DivideByZeroException)
            {
                newBrushRatio = 1.0f;
            }

            var inverseVelocitySize = _brushProperties.InverseVelocitySize;
            newBrushRatio = inverseVelocitySize ? newBrushRatio : 1.0f - newBrushRatio;
            var minRatio = _brushProperties.MinRatio;
            newBrushRatio = Mathf.Clamp(newBrushRatio, minRatio, 1.0f);

            return brushSize * newBrushRatio;
        }

        public float CalculateStartSize(float brushSize)
        {
            var startUpTime = _brushProperties.TimeLength;
            _timer.Start();

            if (_timer.Elapsed.TotalSeconds > startUpTime) return brushSize;

            if (startUpTime <= 0)
            {
                return 0;
            }

            var startRatio = (float)_timer.Elapsed.TotalSeconds / startUpTime;

            return brushSize * startRatio;
        }

        public float CalculateAngleSize(float brushSize)
        {
            var angleSizeRatio = _brushProperties.AngleSizeRatio;
            var minAngle = _brushProperties.MinAngle;

            var angleDot = 1 - Vector3.Dot(_surfaceTransform.forward, _stylusTransform.forward);
            angleDot = angleDot / (minAngle / 90.0f);
            angleDot = Mathf.Clamp01(angleDot);

            var angleRatio = Mathf.Lerp(1.0f, angleSizeRatio, angleDot);
            if (angleRatio <= 0)
            {
                angleRatio = 0.0001f;
            }

            return brushSize * angleRatio;
        }

        public float CalculateAbruptStopSize(float brushSize, float lastAverageSpeed, float averageSpeed)
        {
            var stopSizeScalar = _brushProperties.StopSizeScalar;
            var velocityThreshold = _brushProperties.VelocityThreshold;
            var changeInVelocity = lastAverageSpeed - averageSpeed;

            if (changeInVelocity >= velocityThreshold)
            {
                brushSize *= stopSizeScalar;
            }

            return brushSize;
        }

        public float CalculateAngleAbruptStopSize(float brushSize, float lastAngularVelocity, float angularVelocity)
        {
            var angularStopSizeScalar = _brushProperties.AngleStopSizeScalar;
            var angularVelocityThreshold = _brushProperties.AngularVelocityThreshold;
            var changeInAngularVelocity = lastAngularVelocity - angularVelocity;

            if (changeInAngularVelocity >= angularVelocityThreshold)
            {
                brushSize *= angularStopSizeScalar;
            }

            return brushSize;
        }

        public float CalculateVelocityFrequency(float splattersPerSecond, float averageSpeed)
        {
            var frequencyVelocityScalar = _brushProperties.FrequencyVelocityScalar;
            var maxFrequencyVelocity = _brushProperties.MaxFrequencyVelocity;
            float velocityRatio;
            try
            {
                velocityRatio = averageSpeed / maxFrequencyVelocity;
            }
            catch (DivideByZeroException)
            {
                velocityRatio = 1.0f;
            }

            velocityRatio = Mathf.Clamp01(velocityRatio);
            var newFrequencyRatio = Mathf.Lerp(1.0f, frequencyVelocityScalar, velocityRatio);
            return splattersPerSecond * newFrequencyRatio;
        }

        public float CalculateAngularVelocityFrequency(float splattersPerSecond, float angularVelocity)
        {
            var frequencyAngleScalar = _brushProperties.FrequencyAngleScalar;
            var minAngularVelocity = _brushProperties.MinAngularVelocity;
            float newFrequencyRatio;
            try
            {
                newFrequencyRatio = angularVelocity / minAngularVelocity;
            }
            catch (DivideByZeroException)
            {
                newFrequencyRatio = 1.0f;
            }

            newFrequencyRatio = Mathf.Clamp(newFrequencyRatio, 1.0f, newFrequencyRatio);
            newFrequencyRatio -= 1;
            newFrequencyRatio *= frequencyAngleScalar;
            newFrequencyRatio += 1;

            return splattersPerSecond * newFrequencyRatio;
        }

        public void FinishDrawingLine()
        {
            _timer.Reset();
        }
    }
}
