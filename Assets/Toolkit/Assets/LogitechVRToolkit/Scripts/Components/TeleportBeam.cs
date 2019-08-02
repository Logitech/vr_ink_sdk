namespace Logitech.XRToolkit.Components
{
    using Logitech.XRToolkit.IO;
    using System.Collections.Generic;
    using UnityEngine;

    public enum TeleportBeamType
    {
        Line,
        Arc,
        LandingZoneOnly
    }

    /// <summary>
    /// Display the teleportation visual and generate the landing position on the ground.
    /// </summary>
    public class TeleportBeam : MonoBehaviour
    {
        public TeleportBeamType TeleportBeamType;
        public bool TeleportEnabled = false;

        // TODO this will fail if not 0, needs fix.
        private const float OffsetAngle = 0f;
        public float OffsetForwardOrigin = 0.01f;
        public float OffsetHeightOrigin = 0f;
        public float ArcRadius = 10;

        private const int ArcPointCount = 80;
        private const int LinePointCount = 2;

        private float _curveWidth = 0.002f;
        private float _planeAngleThreshold = 5f;
        private List<Vector3> _beamPoints = new List<Vector3>();
        private LineRenderer _lineRenderer;
        private GameObject _landingArea;
        private GameObject _landingAreaBody;
        private Vector3 _landingPos;
        private bool _teleportValid = false;
        private const float OffsetFromGround = 0.005f;

        /// <summary>
        /// On first call, create the sub Component and GameObject used to render the teleportation visual.
        /// </summary>
        private void OnEnable()
        {
            var colour = Resources.Load<Material>("Teleport/Materials/TeleportBodyMat").color;
            colour.a = 1f;

            if (_landingArea == null)
            {
                CreateLandingAreaObject(colour);
            }

            if (_lineRenderer == null)
            {
                _lineRenderer = gameObject.AddComponent<LineRenderer>();
                _lineRenderer.widthMultiplier = _curveWidth;

                _lineRenderer.material = _landingArea.GetComponent<SpriteRenderer>().material;
                _lineRenderer.material.color = colour;
                _lineRenderer.sortingOrder = 1;
            }
        }

        void LateUpdate()
        {
            if (TeleportEnabled)
            {
                _lineRenderer.enabled = true;
                _teleportValid = true;
                switch (TeleportBeamType)
                {
                    case TeleportBeamType.Arc:
                        UpdateArcValue();
                        break;
                    case TeleportBeamType.Line:
                        UpdateLineValue();
                        break;
                    case TeleportBeamType.LandingZoneOnly:
                        UpdateLineValue();
                        _lineRenderer.enabled = false;
                        break;
                }

                UpdateLandingArea();
            }
            else
            {
                _lineRenderer.enabled = false;
                _landingArea.SetActive(false);
                _teleportValid = false;
            }
        }

        public Vector3 GetBeamHitPoint()
        {
            return _landingPos;
        }

        public bool IsValid()
        {
            return _teleportValid;
        }

        private void UpdateArcValue()
        {
            _lineRenderer.positionCount = ArcPointCount;
            _beamPoints.Clear();
            if (ArcRadius < transform.position.y)
            {
                ArcRadius = transform.position.y + 0.02f;
            }
            Debug.Assert(GetComponent<TrackedDevice>() != null);

            // Get the angle of the controller where pointing the ground is 0 and increasing while going from pointing the ground to the sky.
            var initialAngle = Mathf.Deg2Rad * (transform.localEulerAngles.x) + Mathf.PI / 2;
            initialAngle = initialAngle > Mathf.PI ? initialAngle - (Mathf.PI * 2) : initialAngle;
            initialAngle = Mathf.PI - initialAngle + OffsetAngle;

            // Get the angle of the arc starting from the Stylus and hitting the ground.
            var opposedSide = Mathf.Abs(Mathf.Sin(initialAngle) * ArcRadius - transform.position.y);
            var finalAngle = Mathf.Asin(opposedSide / ArcRadius);

            // Get the initial x value of the arc.
            var startX = Mathf.Cos(initialAngle) * ArcRadius;

            var beamOrigin = transform.position + transform.forward * OffsetForwardOrigin + new Vector3(0f, OffsetHeightOrigin, 0f);
            _beamPoints.Add(beamOrigin);

            // Generate the points used in the Bezier curve.
            var beamMaxDistance = Mathf.Cos(finalAngle) * ArcRadius - startX;
            var controlPoint = beamOrigin + (transform.forward * (beamMaxDistance / 2));
            var endPoint = beamOrigin + (transform.forward * beamMaxDistance);
            endPoint.y = OffsetFromGround;

            var stepping = 1f / ArcPointCount;
            for (var i = 0f; i < 1f; i += stepping)
            {
                var firstSegment = Vector3.Lerp(beamOrigin, controlPoint, i);
                var secondSegment = Vector3.Lerp(controlPoint, endPoint, i);
                var curvePoint = Vector3.Lerp(firstSegment, secondSegment, i);

                _beamPoints.Add(curvePoint);
            }

            _landingPos = _beamPoints[ArcPointCount];

            _lineRenderer.SetPositions(_beamPoints.ToArray());
            _lineRenderer.enabled = _teleportValid;
        }

        private void UpdateLineValue()
        {
            _lineRenderer.positionCount = LinePointCount;
            Debug.Assert(GetComponent<TrackedDevice>() != null);
            // Do nothing if the Stylus doesn't point towards the ground.
            _beamPoints.Clear();
            if (transform.localEulerAngles.x > _planeAngleThreshold)
            {
                var ray = new Ray(transform.position, transform.forward);
                var hPlane = new Plane(Vector3.up, Vector3.zero);
                var distance = 0f;

                if (hPlane.Raycast(ray, out distance))
                {
                    // Get the hit point.
                    _landingPos = ray.GetPoint(distance);
                    var beamOrigin = transform.position + transform.forward * OffsetForwardOrigin + new Vector3(0f, OffsetHeightOrigin, 0f);
                    _beamPoints.Add(beamOrigin);
                    _beamPoints.Add(_landingPos);
                }
                _lineRenderer.SetPositions(_beamPoints.ToArray());
            }
            else
            {
                _teleportValid = false;
            }
            _lineRenderer.enabled = _teleportValid;
        }

        private void UpdateLandingArea()
        {
            var landingPos = GetBeamHitPoint();
            landingPos.y = OffsetFromGround;

            _landingArea.transform.position = landingPos;
            _landingArea.transform.eulerAngles = new Vector3(90f, 0f, 0f);
            _landingArea.SetActive(_teleportValid);
        }

        private void CreateLandingAreaObject(Color colour)
        {
            if (_landingArea == null)
            {
                _landingArea = new GameObject("LandingArea");
                _landingArea.transform.parent = gameObject.transform;
                _landingArea.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
                _landingArea.transform.position = new Vector3(0, 0, 0);
                var landingSprite = _landingArea.AddComponent<SpriteRenderer>();
                landingSprite.sprite = Resources.Load<Sprite>("Teleport/Sprites/teleportTarget");
                landingSprite.color = colour;
                landingSprite.sortingOrder = 1;

                _landingArea.SetActive(false);

                _landingAreaBody = Instantiate(Resources.Load<GameObject>("Teleport/3DModel/TeleportBody"));
                _landingAreaBody.transform.parent = _landingArea.transform;
                _landingAreaBody.name = "LandingAreaBody";
                _landingAreaBody.transform.localScale = new Vector3(4.5f, 2f, 4.5f);
                _landingAreaBody.transform.position = new Vector3(0f, 0f, 0f);
                _landingAreaBody.transform.eulerAngles = new Vector3(-90f, 0f, 0f);
                var landingBodyMeshRenderer = _landingAreaBody.GetComponentInChildren<MeshRenderer>();
                landingBodyMeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                landingBodyMeshRenderer.material = Resources.Load<Material>("Teleport/Materials/TeleportBodyMat");
                landingBodyMeshRenderer.sortingOrder = 1;
            }
        }
    }
}
