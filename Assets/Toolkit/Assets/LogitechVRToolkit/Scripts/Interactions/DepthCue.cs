namespace Logitech.XRToolkit.Interactions
{
    using Logitech.XRToolkit.Triggers;
    using UnityEngine;

    /// <summary>
    /// Shows a shadow that gets smaller and more opaque as a given transform gets closer to that object's collider.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class DepthCue : MonoBehaviour
    {
        [Header("Size")]
        [SerializeField]
        private float _minSize = 0f;
        [SerializeField]
        private float _maxSize = 0.03f;
        [Header("Alpha")]
        [SerializeField]
        private float _minAlpha = 0f;
        [SerializeField]
        private float _maxAlpha = 0.8f;

        [SerializeField]
        private RaycastTrigger _raycastTrigger;

        [SerializeField]
        private Texture2D _shadowTexture;
        [SerializeField]
        private Material _shadowMaterial;

        private Renderer _shadow;

        private void Start()
        {
            _shadow = CreateShadow(_shadowTexture);
        }

        private Renderer CreateShadow(Texture tex)
        {
            var shadow = GameObject.CreatePrimitive(PrimitiveType.Quad);
            shadow.name = "Shadow on " + name;
            Destroy(shadow.GetComponent<MeshCollider>());
            var renderer = shadow.GetComponent<Renderer>();
            Material material = _shadowMaterial;
            material.mainTexture = tex;
            material.color = Color.white;
            renderer.material = material;

            return renderer;
        }

        public void SetRaycastSource(Transform t)
        {
            _raycastTrigger.Source = t;
        }

        private void LateUpdate()
        {
            if (_raycastTrigger.IsValid())
            {
                RaycastHit hit = _raycastTrigger.RaycastHit;

                // Set pose.
                _shadow.transform.position = hit.point + hit.normal.normalized * 0.001f;
                _shadow.transform.forward = -hit.normal; // this orients the transform!

                // Change scale.
                var scale = Mathf.Lerp(_minSize, _maxSize, hit.distance / _raycastTrigger.MaxDistance);
                _shadow.transform.localScale = new Vector3(scale, scale, scale);

                // Change alpha.
                var alpha = Mathf.Lerp(_minAlpha, _maxAlpha, 1f - hit.distance / _raycastTrigger.MaxDistance);
                var color = _shadow.material.color;
                color.a = alpha;
                _shadow.material.color = color;
            }
            else
            {
                _shadow.transform.position = Vector3.zero;
                _shadow.transform.localScale = Vector3.zero;
            }
        }
    }
}
