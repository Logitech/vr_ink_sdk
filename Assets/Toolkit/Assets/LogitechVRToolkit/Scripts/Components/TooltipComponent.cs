namespace Logitech.XRToolkit.Components
{
    using Logitech.XRToolkit.Actions;
    using UnityEngine;

    /// <summary>
    /// Used on a tooltip game object (see prefab). Makes sure the tooltip faces the camera, and is connected to where
    /// it should point to.
    /// </summary>
    public class TooltipComponent : MonoBehaviour
    {
        [SerializeField, Tooltip("Will default to Camera.main if left blank")]
        private Camera _mainCamera;
        [SerializeField, Tooltip("Where this tooltip should point to")]
        private Transform _lineEnd;

        [Header("Line Properties")]
        [SerializeField]
        private Color _lineColor = Color.white;
        [SerializeField, Tooltip("Colour of the line when it cannot reach the target position (e.g. going through the Stylus)")]
        private Color _unfinishedLineColor = Color.black;
        [SerializeField, Range(0f, 0.01f)]
        private float _lineWidth = 0.001f;
        [SerializeField]
        private AnimationCurve _lineWidthCurve = AnimationCurve.Constant(0f, 1f, 1f);
        [SerializeField]
        private FaceObjectAction _textFaceAction, _backgroundFaceAction;

        private Transform _background, _text, _lineStart;
        private LineRenderer _line;
        private const int LINE_SEGMENTS = 10;

        /// <summary>
        /// Changes the text of that tooltip.
        /// </summary>
        /// <param name="text">The new text to be displayed.</param>
        public void SetText(string text)
        {
            var textMesh = _text.GetComponent<TextMesh>();

            // Set the actual text.
            textMesh.text = text;

            // Compute new background width.
            Font font = textMesh.font;
            CharacterInfo ci;
            int size = 0;
            foreach (char c in text)
                if (font.GetCharacterInfo(c, out ci, textMesh.fontSize, textMesh.fontStyle))
                    size += ci.advance;
            Vector3 scale = _background.localScale;
            float actualSize = size * 0.0004f; // Don't ask, it works.
            scale.x = actualSize;
            _background.localScale = scale;

            // Reposition line start.
            Vector3 position = _lineStart.position;
            position.x = Mathf.Sign(position.x) * (actualSize / 2f + 0.05f);
        }

        private void Awake()
        {
            if (_mainCamera == null)
                _mainCamera = Camera.main;

            Debug.Assert(_mainCamera != null, "You need to either assign the camera, or have one tagged MainCamera");

            _background = transform.Find("Background");
            _text = transform.Find("Text");
            _lineStart = transform.Find("Background/Line Start");

            Debug.Assert(_background != null && _text != null && _lineStart != null);

            _line = gameObject.AddComponent<LineRenderer>();
            _line.material = new Material(Shader.Find("Sprites/Default"));
            _line.material.color = _lineColor;
            _line.widthMultiplier = _lineWidth;
            _line.widthCurve = _lineWidthCurve;
            _line.positionCount = LINE_SEGMENTS;
        }

        private void Update()
        {
            // Make the elements face the camera.
            _backgroundFaceAction.Update(true);
            _textFaceAction.Update(true);

            // Check if we can reach the end of the line.
            Vector3 lineVector = _lineEnd.position - _lineStart.position;
            RaycastHit hit;
            Vector3 endPosition = _lineEnd.position;
            Color color = _lineColor;
            if (Physics.Raycast(_lineStart.position, lineVector, out hit, lineVector.magnitude))
            {
                // Stop where we collide and change colour if applicable.
                endPosition = hit.point;
                color = _unfinishedLineColor;
            }

            // Apply computed values to line renderer.
            _line.SetPositions(InterpolatedArray(_lineStart.position, endPosition, LINE_SEGMENTS - 1));
            //_line.widthCurve = ScaleAnimationCurve(ref _lineWidthCurve, (endPosition - _lineStart.position).magnitude);
            _line.widthCurve = _lineWidthCurve;
            _line.startColor = color;
            _line.endColor = color;
        }

        /// <summary>
        /// Returns an array A of size steps + 1, where A[0] is start, A[steps] is end, and A[1..steps-1] are linearly
        /// interpolated between start and end.
        /// </summary>
        private Vector3[] InterpolatedArray(Vector3 start, Vector3 end, int steps)
        {
            var array = new Vector3[steps + 1];
            for (int i = 0; i <= steps; i++)
            {
                array[i] = Vector3.Lerp(start, end, (float)i / steps);
            }
            return array;
        }
    }
}
