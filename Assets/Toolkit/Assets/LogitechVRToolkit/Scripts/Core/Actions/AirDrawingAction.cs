namespace Logitech.XRToolkit.Actions
{
    using Action = Logitech.XRToolkit.Core.Action;
    using Logitech.XRToolkit.Core;
    using Logitech.XRToolkit.Inking;
    using Logitech.XRToolkit.Providers;
    using Logitech.XRToolkit.UndoRedo;
    using Logitech.XRToolkit.Utils;
    using System.Linq;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Draws lines in 3D space.
    /// </summary>
    [Serializable]
    public class AirDrawingAction : Action
    {
        [Header("Device To Draw With")]
        [SerializeField]
        private TrackedDeviceTransformProvider _trackedDeviceProvider;

        [Header("Line Settings")]
        [SerializeField]
        private bool _useAnalog = true;

        [SerializeField, ShowIf("_useAnalog")]
        private AxisValueProvider _lineWidthProvider;

        [SerializeField]
        private Material _lineMaterial;

        private Provider<float> _maxLineWidth = new FunctionProvider<float>(() => DrawingVariables.Instance.LineMaxWidth * DrawingVariables.AirWidthModifier);
        private Provider<Color> _colorProvider = new FunctionProvider<Color>(() => DrawingVariables.Instance.Colour);

        private const float TimeInterval = 0f;

        private float _timer = 0f;
        private LineRenderer _currentLine = null;

        [SerializeField]
        private Transform _drawingParent;

        private WidthCurve _currentWidthCurve;
        private Vector3 _lastPosition;
        private const float MinimalDrawingDistance = 0.001f;

        [Header("Line Smoothing")]
        [SerializeField]
        private bool _isSmoothingActive = false;

        [SerializeField, Range(0, 11)]
        private int _windowSize = 2;

        private Vector3[] _lastPositionsBuffer;

        /// <summary>
        /// Starts a new drawing.
        /// </summary>
        protected override void OnTriggerValid()
        {
            _timer = TimeInterval;
            // Start a new line
            bool autoTaper = !_useAnalog;
            if (_lineMaterial == null)
            {
                _lineMaterial = new Material(Shader.Find("Sprites/Default"));
            }

            StartNewLine(_trackedDeviceProvider.GetOutput().position, _lineMaterial, _colorProvider.GetOutput(), _maxLineWidth.GetOutput(), autoTaper);
        }

        /// <summary>
        /// Adds points to the current line.
        /// </summary>
        protected override void TriggerValid()
        {
            Debug.Assert(_currentLine != null, "If we are already drawing there should be a current line");
            if (_timer < Time.time)
            {
                // Reset timer
                _timer = Time.time;

                // Add a point to the current line
                // TODO: provide a way to add an offset to the position (either by Provider<Transform> or built-in Vector3)
                float lineWidth = 0f;
                if (_lineWidthProvider != null)
                {
                    lineWidth = _useAnalog ? _lineWidthProvider.GetOutput() : DrawingVariables.Instance.LineMaxWidth;
                }
                if (_isSmoothingActive)
                {
                    AddMeanPoint(_currentLine, _currentWidthCurve, _trackedDeviceProvider.GetOutput().position, lineWidth);
                }
                else
                {
                    AddPoint(_currentLine, _currentWidthCurve, _trackedDeviceProvider.GetOutput().position, lineWidth);
                }
            }
        }

        /// <summary>
        /// Finishes the drawing.
        /// </summary>
        protected override void OnTriggerInvalid()
        {
            _currentLine.gameObject.AddComponent<UndoRedoGameObject>();
        }

        private void StartNewLine(Vector3 position, Material material, Color color, float lineWidth, bool automatedTaper)
        {
            var go = new GameObject("Drawing");
            go.transform.position = position;
            LineRenderer lineRenderer = go.AddComponent<LineRenderer>();
            lineRenderer.material = material;
            lineRenderer.material.color = color;
            lineRenderer.widthMultiplier = lineWidth;
            lineRenderer.positionCount = 0;
            lineRenderer.useWorldSpace = false;
            lineRenderer.transform.parent = _drawingParent;
            _currentWidthCurve = new WidthCurve(automatedTaper);
            _lastPosition = position;

            if (_isSmoothingActive)
            {
                _lastPositionsBuffer = null;
                AddMeanPoint(lineRenderer, _currentWidthCurve, position, 0f);
            }
            else
            {
                AddPoint(lineRenderer, _currentWidthCurve, position, 0f);
            }

            _currentLine = lineRenderer;
        }

        private void AddPoint(LineRenderer line, WidthCurve curve, Vector3 newPosition, float width)
        {
            float distance = Vector3.Distance(_lastPosition, newPosition);
            if (distance < MinimalDrawingDistance && curve.Distances.Count > 0)
            {
                line.widthCurve = curve.GetCurve();
                line.SetPosition(line.positionCount - 1, line.transform.InverseTransformPoint(newPosition));
                return;
            }
            _lastPosition = newPosition;
            curve.AddPoint(width, distance);
            line.widthCurve = curve.GetCurve();
            line.positionCount++;
            line.SetPosition(line.positionCount - 1, line.transform.InverseTransformPoint(newPosition));
        }

        private void AddMeanPoint(LineRenderer line, WidthCurve curve, Vector3 newPosition, float width)
        {
            float distance = Vector3.Distance(_lastPosition, newPosition);
            if (distance < MinimalDrawingDistance && curve.Distances.Count > 0)
            {
                line.widthCurve = curve.GetCurve();
                line.SetPosition(line.positionCount - 1, line.transform.InverseTransformPoint(newPosition));
                return;
            }
            AddPointInBuffer(_lastPosition);
            _lastPosition = newPosition;
            curve.AddPoint(width, distance);
            line.widthCurve = curve.GetCurve();
            line.positionCount++;
            line.SetPosition(line.positionCount - 1, line.transform.InverseTransformPoint(newPosition));
            ApplyMean(line);
        }

        private void ApplyMean(LineRenderer line)
        {
            int meanSize = _windowSize;
            Vector3 meanAverage = Vector3.zero;
            foreach (Vector3 elem in _lastPositionsBuffer)
            {
                if (elem == Vector3.zero)
                {
                    meanSize--;
                }
                else
                {
                    meanAverage += elem;
                }
            }
            if (meanSize > 0)
            {
                meanAverage = meanAverage / meanSize;
            }

            if (line.positionCount >= 2)
            {
                line.SetPosition(line.positionCount - 2, line.transform.InverseTransformPoint(meanAverage));
            }
        }

        public void SetSmoothingMode(bool smoothingStatus)
        {
            _isSmoothingActive = smoothingStatus;
        }

        /// <summary>
        /// Remember the last 10 points to be able to apply a mean average on each elements with it's neighbor. Keep
        /// this array as a FIFO.
        /// </summary>
        /// <param name="newPosition">The new latest element.</param>
        private void AddPointInBuffer(Vector3 newPosition)
        {
            if (_lastPositionsBuffer == null || _lastPositionsBuffer.Length != _windowSize)
            {
                _lastPositionsBuffer = new Vector3[_windowSize];
                for (int i = 0; i < _lastPositionsBuffer.Length; i++)
                {
                    _lastPositionsBuffer[i] = Vector3.zero;
                }
            }

            var tempArray = new Vector3[_windowSize];
            for (int i = 0; i < _lastPositionsBuffer.Length; i++)
            {
                if (i == _lastPositionsBuffer.Length - 1)
                {
                    tempArray[i] = newPosition;
                }
                else
                {
                    tempArray[i] = _lastPositionsBuffer[i + 1];
                }
            }
            _lastPositionsBuffer = tempArray;
        }

        /// <summary>
        /// Holds information regarding the width of the line along its path.
        /// </summary>
        private class WidthCurve
        {
            public List<float> Distances;
            private List<float> _widths;
            private bool _simulateTaper;

            public WidthCurve(bool simulateTaper)
            {
                _simulateTaper = simulateTaper;
                _widths = new List<float>();
                Distances = new List<float>();
            }

            /// <summary>
            /// Add a point at a given distance from the last one and its corresponding width to the curve representing
            /// the overall width of the line along its path.
            /// </summary>
            public void AddPoint(float width, float distance)
            {
                _widths.Add(width);
                Distances.Add(distance);
            }

            /// <summary>
            /// Use this to get the current curve that a line renderer with points corresponding to
            /// these previously added to this data structure can understand as a width curve.
            /// </summary>
            public AnimationCurve GetCurve()
            {
                Debug.Assert(Distances.Count == _widths.Count, "Both lists should have the same length");
                Debug.Assert(Distances[0] == 0, "The first length should be zero");

                if (_widths.Count == 0)
                {
                    Debug.LogError("Cannot get you a curve with no points.");
                    return null;
                }

                float totalDistance = Distances.Aggregate((sum, next) => sum + next);

                if (_simulateTaper)
                {
                    return TaperSimulation(0.05f / totalDistance);
                }
                else
                {
                    var keyframes = new Keyframe[_widths.Count];
                    float distanceToThisPoint = 0f;
                    for (int i = 0; i < keyframes.Length; i++)
                    {
                        distanceToThisPoint += Distances[i];
                        keyframes[i] = new Keyframe(distanceToThisPoint / totalDistance, _widths[i]);
                    }
                    return new AnimationCurve(keyframes);
                }
            }

            private static AnimationCurve TaperSimulation(float taperProportion)
            {
                if (taperProportion > 0.5f)
                {
                    taperProportion = 0.5f;
                }

                Keyframe[] easeIn = AnimationCurve.EaseInOut(0f, 0f, taperProportion, 1f).keys;
                Keyframe[] constantBit = AnimationCurve.Constant(taperProportion, 1f - taperProportion, 1f).keys;
                Keyframe[] easeOut = AnimationCurve.EaseInOut(1f - taperProportion, 1f, 1f, 0f).keys;

                var keyframes = new Keyframe[easeIn.Length + constantBit.Length + easeOut.Length];
                easeIn.CopyTo(keyframes, 0);
                constantBit.CopyTo(keyframes, easeIn.Length);
                easeOut.CopyTo(keyframes, easeIn.Length + constantBit.Length);

                return new AnimationCurve(keyframes);
            }
        }
    }
}
