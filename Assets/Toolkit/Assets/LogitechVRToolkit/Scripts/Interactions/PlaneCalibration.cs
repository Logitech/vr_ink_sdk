namespace Logitech.XRToolkit.Interactions
{
    using Logitech.XRToolkit.Providers;
    using Logitech.XRToolkit.Triggers;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    /// <summary>
    /// Places a plane at a specific location, which is defined by pointing out three points in space.
    /// </summary>
    public class PlaneCalibration : MonoBehaviour
    {
        // Used for other scripts to detect when in calibration mode.
        public bool CalibrationMode;
        public delegate void DoneCallback(Transform calibratedPlane);

        [SerializeField]
        private TrackedDeviceTransformProvider _stylusTransform;

        [SerializeField]
        private InputTrigger _newPointTrigger;
        [SerializeField, Tooltip("The plane to be repositioned by the procedure.")]
        private Transform _drawingPlane;

        [SerializeField]
        private Material _lineMaterial;
        [SerializeField]
        private Texture2D _lineTexture;

        private List<Vector3> _calibrationPoints = new List<Vector3>(3);

        private const float DotSize = 0.01f;
        private const float DashesPerMeter = 500f;

        private LineRenderer _dashedLine;
        private Transform _dotUnderPen;

        private DoneCallback _callback;

        public void StartCalibration(DoneCallback callback)
        {
            _callback = callback;

            if (_dotUnderPen != null)
            {
                Destroy(_dotUnderPen.gameObject);
            }

            if (_dashedLine != null)
            {
                Destroy(_dashedLine.gameObject);
            }

            _calibrationPoints.Clear();
            _dashedLine = CreateLineRenderer();
            _dotUnderPen = CreateCursor();
        }

        public void StartCalibration()
        {
            if (_dotUnderPen != null)
            {
                Destroy(_dotUnderPen.gameObject);
            }

            if (_dashedLine != null)
            {
                Destroy(_dashedLine.gameObject);
            }

            _calibrationPoints.Clear();
            _dashedLine = CreateLineRenderer();
            _dotUnderPen = CreateCursor();
            CalibrationMode = true;
        }

        private void Awake()
        {
            // fill the list so that calibration starts only when requested by the user
            for (int i = 0; i < 5; i++)
            {
                _calibrationPoints.Add(Vector3.zero);
            }
        }

        private LineRenderer CreateLineRenderer()
        {
            var line = new GameObject("Plane creation line").AddComponent<LineRenderer>();
            line.material = _lineMaterial;
            line.material.mainTexture = _lineTexture;
            line.widthMultiplier = 1f / DashesPerMeter;
            line.positionCount = 5;
            line.gameObject.SetActive(false);
            return line;
        }

        private Transform CreateCursor()
        {
            var dot = GameObject.CreatePrimitive(PrimitiveType.Quad);
            dot.name = "Plane creation cursor";
            dot.transform.localScale = new Vector3(DotSize, DotSize, DotSize);
            var dotRenderer = dot.GetComponent<Renderer>();
            dotRenderer.material = _lineMaterial;
            dotRenderer.material.mainTexture = _lineTexture;
            return dot.transform;
        }

        private void Update()
        {
            if (_calibrationPoints.Count > 3)
            {
                CalibrationMode = false;
                return;
            }

            var unconfirmedPoint = _stylusTransform.GetOutput().position;

            // Position first dot under Stylus.
            PositionDotUnderPen(_dotUnderPen, unconfirmedPoint);

            switch (_calibrationPoints.Count)
            {
                case 0:
                    // Add new point to list if button pressed.
                    if (_newPointTrigger.IsValid())
                    {
                        _calibrationPoints.Add(_stylusTransform.GetOutput().position);
                    }
                    break;
                case 1:
                    // Draw dotted line between first point and Stylus.
                    _dashedLine.gameObject.SetActive(true);
                    var linePoints = new Vector3[] { _calibrationPoints[0], unconfirmedPoint };
                    DrawClosedPolygon(_dashedLine, linePoints);

                    // Add new point to list if button pressed.
                    if (_newPointTrigger.IsValid())
                    {
                        _calibrationPoints.Add(_stylusTransform.GetOutput().position);
                    }
                    break;
                case 2:
                    // Draw rectangle constrained by Stylus position.
                    var thirdPoint = GetProjectionOnPlane(unconfirmedPoint, _calibrationPoints[1], _calibrationPoints[1] - _calibrationPoints[0]);
                    var fourthPoint = GetProjectionOnPlane(unconfirmedPoint, _calibrationPoints[0], _calibrationPoints[0] - _calibrationPoints[1]);

                    Vector3[] rectPoints = { _calibrationPoints[0], _calibrationPoints[1], thirdPoint, fourthPoint };
                    DrawClosedPolygon(_dashedLine, rectPoints);

                    // Add last two points to list if button pressed.
                    if (_newPointTrigger.IsValid())
                    {
                        _calibrationPoints.Add(thirdPoint);
                        _calibrationPoints.Add(fourthPoint);

                        // The Main camera's forward direction is used to disambiguate between the four possible orientation.
                        FinishPositioningRectangle(_drawingPlane, _calibrationPoints, Camera.main.transform.forward);

                        if (_callback != null) _callback(_drawingPlane);
                    }
                    break;
                default:
                    Debug.LogError("Calibration has illegal number of points: " + _calibrationPoints.Count);
                    break;
            }
        }

        private static void PositionDotUnderPen(Transform dot, Vector3 position)
        {
            dot.position = position;
            dot.forward = Vector3.down;
        }

        private static Pose GetPlaneOrientation(List<Vector3> corners, Vector3 headForward)
        {
            var pose = new Pose();

            var center = corners[0] + (corners[2] - corners[0]) / 2f;
            pose.position = center;

            var l1 = (corners[1] - corners[0]).normalized;
            var l2 = (corners[1] - corners[2]).normalized;

            var normal = Vector3.Cross(l1, l2).normalized;
            if (normal.y < 0f)
            {
                normal = Vector3.Cross(l2, l1).normalized;
            }

            Vector3 snappedL1 = Vector3.Project(headForward, l1);
            Vector3 snappedL2 = Vector3.Project(headForward, l2);
            Vector3 snappedUp = snappedL1;
            if (snappedL1.magnitude < snappedL2.magnitude)
            {
                snappedUp = snappedL2;
            }
            pose.rotation = Quaternion.LookRotation(-normal, snappedUp);

            return pose;
        }

        private void FinishPositioningRectangle(Transform quad, List<Vector3> corners, Vector3 headForward)
        {
            var pose = GetPlaneOrientation(corners, headForward);
            var l1 = (corners[1] - corners[0]);
            var l2 = (corners[1] - corners[2]);
            var x = l1;
            var z = l2;
            if (Mathf.Abs(Vector3.Dot(z.normalized, pose.up.normalized)) < 0.5f)
            {
                x = l2;
                z = l1;
            }
            // Position quad, and scale it.
            quad.position = pose.position;
            quad.rotation = Quaternion.LookRotation(pose.up, -pose.forward);
            quad.localScale = new Vector3(x.magnitude, 1f, z.magnitude);

            Destroy(_dotUnderPen.gameObject);
            Destroy(_dashedLine.gameObject);
        }

        private static void DrawClosedPolygon(LineRenderer line, Vector3[] points)
        {
            Debug.Assert(points.Length > 1, "Cannot draw a polygon with one point.");

            // Set the corners.
            var pointsList = new List<Vector3>(points);

            // Compute the total line distance.
            var distance = 0f;
            for (int i = 1; i < pointsList.Count; i++)
            {
                distance += Vector3.Distance(pointsList[i - 1], pointsList[i]);
            }

            // Close the loop if we have more than two points.
            if (points.Length > 2)
            {
                distance += Vector3.Distance(points.Last(), pointsList.First());
                pointsList.Add(pointsList[0]);
            }

            var dashesCount = distance * DashesPerMeter;

            // Give all this to the line renderer.
            line.positionCount = pointsList.Count;
            line.SetPositions(pointsList.ToArray());
            line.materials[0].mainTextureScale = new Vector3(dashesCount, 1, 1);
        }

        /// <summary>
        /// Returns the projection of a point on a given plane.
        /// </summary>
        private static Vector3 GetProjectionOnPlane(Vector3 toProject, Vector3 planePoint, Vector3 planeNormal)
        {
            Vector3 v = toProject - planePoint;
            Vector3 d = Vector3.Project(v, planeNormal.normalized);
            return toProject - d;
        }
    }
}
