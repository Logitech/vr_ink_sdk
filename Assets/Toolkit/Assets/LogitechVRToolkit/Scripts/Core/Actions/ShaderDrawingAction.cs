namespace Logitech.XRToolkit.Inking
{
    using Action = Logitech.XRToolkit.Core.Action;
    using Logitech.XRToolkit.Core;
    using Logitech.XRToolkit.Providers;
    using Logitech.XRToolkit.UndoRedo;
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Rendering;
    using Random = UnityEngine.Random;
    using Object = UnityEngine.Object;


    /// <summary>
    /// Draws on a surface using a compute shader referenced in the ShaderDrawingProperties passed upon construction.
    /// </summary>
    [Serializable]
    public class ShaderDrawingAction : Action, IUndoRedo
    {
        public ShaderDrawingProperties ShaderDrawingProperties;
        [SerializeField]
        private GameObject _source;

        // If left empty this will use the StylusModelTransformProvider.
        [SerializeField]
        private Transform _target;

        private Vector3? _lastTargetPosition;
        private Quaternion? _lastTargetAngle;
        private const int MaxNumberOfPositions = 8;
        private Queue<Vector3> _velocities = new Queue<Vector3>(MaxNumberOfPositions);
        private const int MaxNumberOfAngles = 15;
        private Queue<float> _angles = new Queue<float>(MaxNumberOfAngles);
        private float _averageSpeed;
        private float _lastAverageSpeed;
        private float _angularVelocity;
        private float _lastAngularVelocity;

        private RaycastHit _hit;

        private RenderTexture _drawingTexture;
        private RenderTexture _undoTexture;

        private ComputeShader _cs;

        private EToolState _ogToolState;
        private bool _firstRaycastFrame;
        private float _previousBrushSize;
        private float _brushSize = 5;
        private float _currSplattersPerSecond;

        private EBrushMode _currentBrushMode;
        private DrawingModificationProperties _brushProperties;

        private DrawingModifier _drawingModifier;

        [SerializeField]
        private Provider<Color> _color = new FunctionProvider<Color>(() => DrawingVariables.Instance.Colour);

        [SerializeField]
        private AxisValueProvider _lineWidthProvider;

        private int _csmainKernel;
        private int _saveFrameKernel;
        private int _writeFrameKernel;
        private int _ereaseAllKernel;

        private Vector2? _lastDrawPosition;
        private Vector2? _lastMidPoint;
        private Vector2? _lastDripPosition;

        private int _latestSaved;
        private int _currentDepth;
        private int _currentUndoDepth;
        private int _deepestUndoFrame;
        private bool _loopedAroundHistory;

        private float _dripTimer;

        /// <summary>
        /// Initializes the compute shader.
        /// TODO: Avoid using an Init method.
        /// </summary>
        public void Init(RenderTexture drawingTexture = null)
        {
            _drawingTexture = drawingTexture;
            UpdateBrushMode();
            InitializeComputeShader();
            if (_target == null) _target = new StylusModelTransformProvider().GetOutput();
            _currentDepth = 0;
            _drawingModifier = new DrawingModifier(_brushProperties, _target, _source.transform);
        }

        /// <summary>
        /// Calculates the brush size and raycasts from the Stylus to see if a surface is hit. Upon hitting the surface,
        /// start drawing.
        /// </summary>
        protected override void TriggerValid()
        {
            // Save the tool state at the start of surface drawing.
            if (!_firstRaycastFrame)
            {
                _ogToolState = DrawingVariables.Instance.PenToolState;
                _firstRaycastFrame = true;
            }

            CalculateAverageVelocity();
            CheckBrushMode();

            // If you miss the surface with your raycast, finish up your drawing.
            if (!RaycastOntoSurface())
            {
                // TODO: use the raycast only to get the position and have the raycast as a second trigger for this action.
                TriggerInvalid();
                return;
            }

            DrawingVariables.Instance.PenToolState = EToolState.SurfaceDrawing;

            CalculateAverageAngularVelocity();

            // Save the brush size at the start of drawing a new line and change the size based on modifiers.
            _previousBrushSize = _brushSize;
            ChangeSize();
            ApplyBrushSizeModifiers();

            // If your brush size is greater than 0, draw the line.
            if (_brushSize > 0)
            {
                DetermineDrawingAction(_hit.textureCoord);
                return;
            }

            // However, if your brush size is 0 cause of the modifiers, finish up the line.
        }

        protected override void TriggerInvalid()
        {
            if (_lastDrawPosition != null)
            {
                // Adds to the undo stack.
                AddToHistory();
            }

            // Reset drawing variables
            _lastDrawPosition = null;
            _lastTargetAngle = null;
            _lastMidPoint = null;

            CalculateAverageVelocity();
            _drawingModifier.FinishDrawingLine();
            _angles.Clear();

            // Set the current state of drawing variables to the state that it was before it started drawing.
            if (_firstRaycastFrame)
            {
                DrawingVariables.Instance.PenToolState = _ogToolState;
                _firstRaycastFrame = false;
            }
        }

        private void InitializeComputeShader()
        {
            Debug.Assert(ShaderDrawingProperties.ComputeShaderRef != null);
#if !UNITY_2018_2_OR_NEWER
            Debug.Log("Disregard the next assertion error, this is a bug solved in Unity 2018.2");
#endif
            _cs = Object.Instantiate(ShaderDrawingProperties.ComputeShaderRef);

            _csmainKernel = _cs.FindKernel("CSMain");
            _saveFrameKernel = _cs.FindKernel("SaveFrame");
            _writeFrameKernel = _cs.FindKernel("WriteFrame");
            _ereaseAllKernel = _cs.FindKernel("EraseAll");

            CreateTextures();

            // Init the texture variables in the compute shader.
            _cs.SetTexture(_csmainKernel, "Result", _drawingTexture);
            _cs.SetTexture(_saveFrameKernel, "UndoTexture", _undoTexture);
            _cs.SetTexture(_saveFrameKernel, "Result", _drawingTexture);
            _cs.SetTexture(_writeFrameKernel, "UndoTexture", _undoTexture);
            _cs.SetTexture(_writeFrameKernel, "Result", _drawingTexture);
            _cs.SetTexture(_ereaseAllKernel, "Result", _drawingTexture);

            // Init variables in the compute shader
            var textureDimension = ShaderDrawingProperties.TextureDimension;
            _cs.SetFloat("previousBrushSize", _previousBrushSize);
            _cs.SetFloat("brushSize", _brushSize);
            _cs.SetFloat("hardness", _brushProperties.Hardness);
            _cs.SetInt("arrayLength", 100);
            _cs.SetVectorArray("penPosition", null);
            _cs.SetVector("penColor", _color.GetOutput());
            _cs.SetVector("TextureDimension", textureDimension);

            _cs.Dispatch(_csmainKernel, (int)(textureDimension.x / 32), (int)(textureDimension.y / 32), 1);
            _cs.Dispatch(_saveFrameKernel, (int)(textureDimension.x / 32), (int)(textureDimension.y / 32), 1);
            _cs.Dispatch(_writeFrameKernel, (int)(textureDimension.x / 32), (int)(textureDimension.y / 32), 1);
            _cs.Dispatch(_ereaseAllKernel, (int)(textureDimension.x / 32), (int)(textureDimension.y / 32), 1);

            for (int i = 0; i < _source.GetComponent<Renderer>().materials.Length; i++)
            {
                _source.GetComponent<Renderer>().materials[i].mainTexture = _drawingTexture;
            }
        }

        private void CreateTextures()
        {
            var startTexture = _source.GetComponent<Renderer>().material.mainTexture;
            var textureDimension = ShaderDrawingProperties.TextureDimension;

            if (_drawingTexture == null)
            {
                _drawingTexture =
                    new RenderTexture((int)textureDimension.x, (int)textureDimension.y, 1)
                    {
                        enableRandomWrite = true,
                        filterMode = FilterMode.Trilinear,
                    };
            }

            if (startTexture != null)
            {
                Graphics.Blit(startTexture, _drawingTexture);
            }

            _drawingTexture.Create();

            _undoTexture = new RenderTexture((int)textureDimension.x, (int)textureDimension.y, 0)
            {
                enableRandomWrite = true,
                volumeDepth = ShaderDrawingProperties.UndoHistorySize,
                dimension = TextureDimension.Tex2DArray
            };

            _undoTexture.Create();
        }

        /// <summary>
        /// Raycasts forward from the Stylus to see if it hits a surface.
        /// </summary>
        /// <returns>
        /// True if surface is hit.
        /// </returns>
        private bool RaycastOntoSurface()
        {
            var origin = _target.position + _target.forward.normalized * ShaderDrawingProperties.Offset;
            var ray = new Ray(origin, _target.forward);
            var raycastLength = _brushProperties.RaycastLength;
            var hitAnObject = Physics.Raycast(origin, _target.forward, out _hit, raycastLength);
            Debug.DrawLine(ray.origin, ray.origin + (raycastLength * ray.direction));

            var onTexture = _hit.textureCoord.x > 0 && _hit.textureCoord.y > 0;

            //var isSelf = _hit.collider?.transform == transform;
            var isSelf = false;
            if (_hit.collider != null)
            {
                isSelf = _hit.collider.transform == _source.transform;
            }

            return hitAnObject && onTexture && isSelf;
        }

        /// <summary>
        /// Checks whether a different brush mode has been selected and if it has, updates the brush mode.
        /// </summary>
        private void CheckBrushMode()
        {
            if (_currentBrushMode == DrawingVariables.Instance.BrushMode)
            {
                return;
            }

            UpdateBrushMode();
        }

        /// <summary>
        /// Retrieves the currently selected properties from the DrawingVariables instance and updates it inside the
        /// _drawingModifier instance
        /// </summary>
        private void UpdateBrushMode()
        {
            _currentBrushMode = DrawingVariables.Instance.BrushMode;

            switch (_currentBrushMode)
            {
                case EBrushMode.Airbrush:
                    _brushProperties = DrawingVariables.Instance.AirbrushMode;
                    break;
                case EBrushMode.Inking:
                    _brushProperties = DrawingVariables.Instance.InkingMode;
                    break;
                case EBrushMode.Shading:
                    _brushProperties = DrawingVariables.Instance.ShadingMode;
                    break;
                case EBrushMode.Splatter:
                    _brushProperties = DrawingVariables.Instance.SplatterMode;
                    break;
                default:
                    _brushProperties = DrawingVariables.Instance.AirbrushMode;
                    break;
            }

            if (_drawingModifier != null)
                _drawingModifier.UpdateProperties(_brushProperties);
        }

        private void ApplyBrushSizeModifiers()
        {
            if (_brushProperties.EnableScalarSize) ApplyScalarSize();
            if (_brushProperties.EnableVelocitySize) ApplyVelocitySize();
            if (_brushProperties.EnableStartSize) ApplyStartSize();
            if (_brushProperties.EnableAngleSize) ApplyAngleSize();
            if (_brushProperties.EnableAbruptStopSize) ApplyAbruptStopSize();
            if (_brushProperties.EnableAngleAbruptStopSize) ApplyAngleAbruptStopSize();
        }

        private void ApplyAbruptStopSize()
        {
            _brushSize = _drawingModifier.CalculateAbruptStopSize(_brushSize, _lastAverageSpeed, _averageSpeed);
        }

        private void ApplyScalarSize()
        {
            var scalarValue = _lineWidthProvider.GetOutput();
            _brushSize = _drawingModifier.CalculateScalarSize(_brushSize, scalarValue);
        }

        private void ApplyAngleAbruptStopSize()
        {
            _brushSize =
                _drawingModifier.CalculateAngleAbruptStopSize(_brushSize, _lastAngularVelocity, _angularVelocity);
        }

        private void ApplyVelocitySize()
        {
            _brushSize = _drawingModifier.CalculateVelocitySize(_brushSize, _averageSpeed);
        }

        private void ApplyStartSize()
        {
            _brushSize = _drawingModifier.CalculateStartSize(_brushSize);
        }

        private void ApplyAngleSize()
        {
            _brushSize = _drawingModifier.CalculateAngleSize(_brushSize);
        }

        /// <summary>
        /// Records the velocities of the Stylus over the last MaxNumberOfPositions and calculates its average velocity.
        /// </summary>
        private void CalculateAverageVelocity()
        {
            if (!_lastTargetPosition.HasValue)
            {
                _lastTargetPosition = _target.position;
                return;
            }

            var curVelocity = (_target.position - _lastTargetPosition.Value) / Time.deltaTime;
            _lastTargetPosition = _target.position;

            // Add the latest velocity and remove the oldest.
            _velocities.Enqueue(curVelocity);
            if (_velocities.Count > MaxNumberOfPositions) _velocities.Dequeue();


            var averageVelocity = Vector3.zero;

            foreach (var velocity in _velocities)
            {
                averageVelocity += velocity;
            }

            if (_velocities.Count > 0)
                averageVelocity /= _velocities.Count;

            _lastAverageSpeed = _averageSpeed;
            _averageSpeed = averageVelocity.magnitude;
        }

        /// <summary>
        /// Records the changes in the Stylus' angle along the X and Y axis over the last MaxNumberOfAngles and
        /// calculates the average speed at which it is rotating.
        /// </summary>
        private void CalculateAverageAngularVelocity()
        {
            if (!_lastTargetAngle.HasValue)
            {
                var currAngle = _target.rotation.eulerAngles;
                currAngle.z = 0;
                _lastTargetAngle = Quaternion.Euler(currAngle);
                return;
            }

            var currentAngle = _target.rotation.eulerAngles;
            currentAngle.z = 0;
            var currentAngleQuaternion = Quaternion.Euler(currentAngle);

            var currAngularVelocity = Quaternion.Angle((Quaternion)_lastTargetAngle, currentAngleQuaternion);
            _angles.Enqueue(currAngularVelocity);
            if (_angles.Count > MaxNumberOfAngles) _angles.Dequeue();

            _lastAngularVelocity = _angularVelocity;
            _angularVelocity = 0.0f;
            foreach (var angle in _angles)
            {
                _angularVelocity += angle;
            }

            _angularVelocity /= MaxNumberOfAngles;

            _lastTargetAngle = currentAngleQuaternion;
        }

        /// <summary>
        /// Determines, and executes the correct action for the appropriate drawing frame. Calls drip method if drip
        /// mode is on. Otherwise draws lines: On the first drawing frame, save the current position as
        /// _lastDrawPosition. On the second drawing frame, save _midPoint and draw a line to it. For future points,
        /// draw bezier curves.
        /// </summary>
        /// <param name="hitCoords">Coordinates of where the surface was hit.</param>
        private void DetermineDrawingAction(Vector2 hitCoords)
        {
            var textureDimension = ShaderDrawingProperties.TextureDimension;
            var pixelCoords = new Vector2(Mathf.Abs(hitCoords.x) * textureDimension.x,
                Mathf.Abs(hitCoords.y) * textureDimension.y);

            // First frame.
            if (_lastDrawPosition == null)
            {
                _lastDrawPosition = pixelCoords;
                return;
            }

            // Second frame.
            if (_lastMidPoint == null)
            {
                _lastMidPoint = (pixelCoords + _lastDrawPosition) / 2f;

                if (!_brushProperties.DisableLineDrawing)
                {
                    DrawLine(_lastDrawPosition.Value, _lastMidPoint.Value);
                }

                if (_brushProperties.EnableDripMode)
                {
                    PrepareDripAction();
                }

                _lastDrawPosition = pixelCoords;

                return;
            }

            // Future frames.
            var newMidPoint = (pixelCoords + _lastDrawPosition) / 2f;

            if (!_brushProperties.DisableLineDrawing)
            {
                DrawBezierCurve(_lastMidPoint.Value, _lastDrawPosition.Value, newMidPoint.Value);
            }

            if (_brushProperties.EnableDripMode)
            {
                PrepareDripAction();
            }

            _lastMidPoint = (pixelCoords + _lastDrawPosition) / 2f;
            _lastDrawPosition = pixelCoords;
        }

        /// <summary>
        /// Updates splatter properties and calls the drip action.
        /// </summary>
        private void PrepareDripAction()
        {
            _currSplattersPerSecond = _brushProperties.SplattersPerSecond;

            // Drip mods.
            if (_brushProperties.EnableDripVelocityFrequency)
            {
                ApplyVelocityFrequency();
            }

            if (_brushProperties.EnableDripAngleFrequency)
            {
                ApplyAngularVelocityFrequency();
            }

            var ogBrushSize = _brushSize;

            if (_lastMidPoint != null)
            {
                DripSplatters((Vector2)_lastMidPoint);
            }

            _brushSize = ogBrushSize;
        }

        /// <summary>
        /// Change the frequency of splatters based on how fast the Stylus is moving.
        /// </summary>
        private void ApplyVelocityFrequency()
        {
            _currSplattersPerSecond = _drawingModifier.CalculateVelocityFrequency(_currSplattersPerSecond, _averageSpeed);
        }

        /// <summary>
        /// Change the frequency of splatters based on how fast the Stylus is rotating.
        /// </summary>
        private void ApplyAngularVelocityFrequency()
        {
            _currSplattersPerSecond = _drawingModifier.CalculateAngularVelocityFrequency(_currSplattersPerSecond, _angularVelocity);
        }

        /// <summary>
        /// Update compute shader parameters and call the stamps painting method.
        /// </summary>
        /// <param name="point">Positions of stamps. Only X, Y, and Z components are used.</param>
        private void PaintTexture(Vector4[] point)
        {
            _cs.SetFloat("previousBrushSize", _previousBrushSize);
            _cs.SetFloat("brushSize", _brushSize);
            _cs.SetFloat("hardness", _brushProperties.Hardness);
            _cs.SetInt("arrayLength", point.Length);
            _cs.SetVectorArray("penPosition", point);
            _cs.SetVector("penColor", _color.GetOutput());
            var textureDimension = ShaderDrawingProperties.TextureDimension;
            _cs.Dispatch(_csmainKernel, (int)(textureDimension.x / 32), (int)(textureDimension.y / 32), 1);
        }

        /// <summary>
        /// Adjust _brushSize based on the drawable surface scale.
        /// </summary>
        private void ChangeSize()
        {
            var textureDimension = ShaderDrawingProperties.TextureDimension;
            var boardScale = _source.transform.lossyScale;
            var xPixelsPerM = textureDimension.x / boardScale.x;
            var yPixelsPerM = textureDimension.y / boardScale.y;
            var pixelDensity = Mathf.Min(xPixelsPerM, yPixelsPerM);

            var newSize = DrawingVariables.Instance.LineMaxWidth;
            _brushSize = pixelDensity * newSize / 2;
        }

        /// <summary>
        /// Calculates random positions around pixelCoords and paints stamps on them.
        /// </summary>
        private void DripSplatters(Vector2 pixelCoords)
        {
            _dripTimer += Time.deltaTime;
            var timePerSplatter = 1.0f / _currSplattersPerSecond;

            var numberOfSplatters = 0;
            if (_dripTimer >= timePerSplatter)
            {
                numberOfSplatters = (int)(_dripTimer / timePerSplatter);
                _dripTimer -= numberOfSplatters * timePerSplatter;
            }

            var positionArray = new Vector4[numberOfSplatters];

            var dripPosition = pixelCoords;
            for (var i = 0; i < numberOfSplatters; i++)
            {
                if (_lastDripPosition != null)
                {
                    var t = (float)i / numberOfSplatters;
                    dripPosition = Vector2.Lerp(pixelCoords, (Vector2)_lastDripPosition, t);
                }

                var randomPoint = Random.insideUnitCircle * _brushSize;
                randomPoint += dripPosition;

                positionArray[i] = new Vector4(randomPoint.x, randomPoint.y);
            }

            _brushSize = _brushProperties.SplatterSize;
            PaintTexture(positionArray);
            _lastDripPosition = pixelCoords;
        }

        /// <summary>
        /// Calculates up to 100 stamps between 2 points, and paints them onto the texture.
        /// </summary>
        private void DrawLine(Vector2 startPixelsCoords, Vector2 endPixelsCoords)
        {
            var pixelDistance = endPixelsCoords - startPixelsCoords;
            var stampsNo = Mathf.FloorToInt((pixelDistance.magnitude / _brushSize) / ShaderDrawingProperties.Spacing) + 1;

            if (stampsNo > 100) stampsNo = 100;
            var positionArray = new Vector4[stampsNo];
            for (var i = 0; i <= stampsNo; i++)
            {
                if (i == 0)
                {
                    continue;
                }

                var lerp = i / (float)stampsNo;
                var pixelCoord = Vector2.Lerp(startPixelsCoords, endPixelsCoords, lerp);
                positionArray[i - 1] = pixelCoord;
            }

            PaintTexture(positionArray);
        }

        /// <summary>
        /// Calculates up to 100 stamps between points p0, p1 and p2, and paints them onto the texture. Uses Bezier curve
        /// with the latest 3 drawing points. More accurately p1: midpoint between drawing point = p(n), and last drawing
        /// point p(n-1), p2: p(n-1), p3: midpoint between p(n-1) and p(n-2).
        /// </summary>
        private void DrawBezierCurve(Vector2 p0, Vector2 p1, Vector2 p2)
        {
            var aDistance = p1 - p0;
            var bDistance = p2 - p1;
            aDistance.x = Mathf.Abs(aDistance.x);
            aDistance.y = Mathf.Abs(aDistance.y);
            bDistance.x = Mathf.Abs(bDistance.x);
            bDistance.y = Mathf.Abs(bDistance.y);

            var totalDistance = aDistance + bDistance;
            var stampsNo = Mathf.FloorToInt((totalDistance.magnitude / _brushSize) / ShaderDrawingProperties.Spacing) + 1;


            if (stampsNo > 100) stampsNo = 100;
            var positionArray = new Vector4[stampsNo];

            for (var i = 0; i <= stampsNo; i++)
            {
                if (i == 0)
                {
                    continue;
                }

                var lerp = i / (float)stampsNo;
                var pixelCoord = GetPointOnBezierCurve(p0, p1, p2, lerp);
                positionArray[i - 1] = pixelCoord;
            }

            PaintTexture(positionArray);
        }

        private Vector2 GetPointOnBezierCurve(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            var a = Vector2.Lerp(p0, p1, t);
            var b = Vector2.Lerp(p1, p2, t);
            var pointOnCurve = Vector2.Lerp(a, b, t);

            return pointOnCurve;
        }

        /// <summary>
        /// Save the current texture in the undo stack. Max stack size is 32.
        /// </summary>
        private void SaveFrame()
        {
            _currentDepth++;

            if (_currentDepth >= ShaderDrawingProperties.UndoHistorySize)
            {
                _currentDepth = 0;
            }

            _latestSaved = _currentDepth;

            if (_currentDepth == _deepestUndoFrame)
            {
                _deepestUndoFrame++;
            }

            if (_deepestUndoFrame >= ShaderDrawingProperties.UndoHistorySize)
            {
                _deepestUndoFrame = 0;
            }

            _cs.SetInt("currentDepth", _currentDepth);
            Vector2 textureDimension = ShaderDrawingProperties.TextureDimension;
            _cs.Dispatch(_saveFrameKernel, (int)(textureDimension.x / 32), (int)(textureDimension.y / 32), 1);
        }

        /// <summary>
        /// Switch texture to the previous texture in the undo stack.
        /// </summary>
        /// <returns>Returns false if trying to call a frame older than 32.</returns>
        private bool UndoStroke()
        {
            if (!_source.activeInHierarchy)
            {
                return true;
            }
            if (_deepestUndoFrame == _currentDepth) return true;
            _currentDepth --;

            if (_currentDepth < 0)
            {
                _currentDepth = ShaderDrawingProperties.UndoHistorySize - 1;
            }

            _cs.SetInt("currentDepth", _currentDepth);
            Vector2 textureDimension = ShaderDrawingProperties.TextureDimension;
            _cs.Dispatch(_writeFrameKernel, (int)(textureDimension.x / 32), (int)(textureDimension.y / 32), 1);

            return false;
        }

        /// <summary>
        /// Switch texture to the next texture in the redo stack.
        /// </summary>
        /// <returns>Return false if it's already the latest texture.</returns>
        private bool RedoStroke()
        {
            if (!_source.activeInHierarchy)
                return true;
            var currentDepthIncremented = _currentDepth + 1;
            if (currentDepthIncremented >= ShaderDrawingProperties.UndoHistorySize)
            {
                currentDepthIncremented = 0;
            }

            if (currentDepthIncremented == _deepestUndoFrame)
            {
                return true;
            }

            var latestSavedIncremented = _latestSaved + 1;
            if (latestSavedIncremented >= ShaderDrawingProperties.UndoHistorySize)
            {
                latestSavedIncremented = 0;
            }

            if (latestSavedIncremented == currentDepthIncremented)
            {
                return true;
            }

            _currentDepth = currentDepthIncremented;

            _cs.SetInt("currentDepth", _currentDepth);
            var textureDimension = ShaderDrawingProperties.TextureDimension;
            _cs.Dispatch(_writeFrameKernel, (int) (textureDimension.x / 32), (int) (textureDimension.y / 32), 1);

            return false;
        }

        /// <summary>
        /// Calls a method to save the texture in the undo stack, and adds the undo/redo actions onto the undo/redo
        /// action stack.
        /// </summary>
        private void AddToHistory()
        {
            SaveFrame();
            UndoRedoManager.Instance.RegisterNewAction(this);
        }

        public void EraseAll()
        {
            _cs.Dispatch(_ereaseAllKernel, (int)(ShaderDrawingProperties.TextureDimension.x / 32), (int)(ShaderDrawingProperties.TextureDimension.y / 32), 1);
        }

        public void Undo()
        {
            UndoStroke();
        }

        public void Redo()
        {
            RedoStroke();
        }

        /// <summary>
        /// Will clear the redo stack. Nothing to do with this.
        /// </summary>
        public void Clear() { }
    }
}
