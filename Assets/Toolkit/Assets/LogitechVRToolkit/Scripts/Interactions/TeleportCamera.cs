namespace Logitech.XRToolkit.Interactions
{
    using System.Collections;

    using Logitech.XRToolkit.Actions;
    using Logitech.XRToolkit.Components;
    using Logitech.XRToolkit.Triggers;

    using UnityEngine;

    /// <summary>
    /// Shows a teleportation beam and teleports the main camera to the beam target location.
    /// </summary>
    public class TeleportCamera : MonoBehaviour
    {
        [SerializeField]
        private float _fadeInDuration = 0.3f;

        [SerializeField]
        private float _fadeOutDuration = 0.3f;

        private Renderer _fadeQuadRenderer;

        [SerializeField]
        private Material _fadeMaterial;

        [SerializeField]
        private InputTrigger _showBeamTrigger;

        [SerializeField]
        private ShowTeleportBeamAction _showBeamAction;

        [SerializeField]
        private InputTrigger _teleportTrigger;

        [SerializeField]
        private TeleportAction _teleportAction;

        private Coroutine _coroutine;
        private bool _teleporting;

        private TeleportBeam _teleportBeam;

        /// <summary>
        /// Initialize the Quad used to fade the camera and create the teleport beam.
        /// </summary>
        private void Start()
        {
            SetUpFadeQuad();
        }

        private void SetUpFadeQuad()
        {
            GameObject fadeQuad = null;

            Camera mainCamera = _teleportAction.CameraParentTransform.GetComponentInChildren<Camera>();
            Debug.Assert(mainCamera != null, "No camera is child of " + _teleportAction.CameraParentTransform.name);

            foreach (MeshFilter mesh in mainCamera.GetComponentsInChildren<MeshFilter>())
            {
                if (mesh.gameObject.name == "FadeQuad")
                {
                    fadeQuad = mesh.gameObject;
                }
            }

            if (fadeQuad == null)
            {
                fadeQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                fadeQuad.GetComponent<Renderer>().material = _fadeMaterial;
                fadeQuad.transform.parent = mainCamera.transform;
                fadeQuad.transform.rotation = Quaternion.LookRotation(mainCamera.transform.forward, mainCamera.transform.up);
                fadeQuad.transform.localPosition = new Vector3(0f, 0f, mainCamera.nearClipPlane + 0.1f);
            }

            fadeQuad.SetActive(true);
            fadeQuad.GetComponent<MeshRenderer>().material.color = new Color(0, 0, 0, 0);
            _fadeQuadRenderer = fadeQuad.GetComponent<Renderer>();
        }

        private void Update()
        {
            if (_teleportBeam == null)
            {
                _teleportBeam = _showBeamAction.TeleportBeam;
            }
            _showBeamAction.Update(_showBeamTrigger.IsValid() && !_teleporting);
            if (_teleportTrigger.IsValid() && _showBeamAction.TeleportBeam.IsValid() && _coroutine == null)
            {
                _coroutine = _teleportBeam.StartCoroutine(FadeAndTeleport());
            }
        }

        private IEnumerator FadeAndTeleport()
        {
            _teleporting = true;
            Color startAlpha = _fadeQuadRenderer.material.color;
            var targetAlpha = new Color(0, 0, 0, 1);

            float currentTime = 0;
            while (currentTime < 1)
            {
                currentTime += Time.deltaTime / _fadeInDuration;
                _fadeQuadRenderer.material.color = Color.Lerp(startAlpha, targetAlpha, currentTime);
                yield return null;
            }
            _teleportAction.UpdateLocation(_showBeamAction.TeleportBeam.GetBeamHitPoint());
            _teleportAction.TriggerOnce();
            _teleporting = false;
            currentTime = 0;

            while (currentTime < 1)
            {
                currentTime += Time.deltaTime / _fadeOutDuration;
                _fadeQuadRenderer.material.color = Color.Lerp(targetAlpha, startAlpha, currentTime);
                yield return null;
            }

            _coroutine = null;
        }

        private void OnDisable()
        {
            _showBeamAction.Update(false);
        }
    }
}
