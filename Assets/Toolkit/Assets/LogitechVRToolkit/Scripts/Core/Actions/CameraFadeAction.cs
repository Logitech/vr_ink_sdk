namespace Logitech.XRToolkit.Actions
{
    using Action = Logitech.XRToolkit.Core.Action;
    using System;
    using System.Collections;
    using UnityEngine;

    /// <summary>
    /// Fades the camera screen to black and back.
    /// </summary>
    /// <remarks>
    /// Can be used for scene transitions.
    /// </remarks>
    [Serializable]
    public class CameraFadeAction : Action
    {
        public Camera CameraObject;
        public float FadeInDuration = 0.3f;
        public float FadeOutDuration = 0.3f;
        public float DarkDuration = 0f;
        public Material FadeMaterial;

        private Renderer _fadeQuadRenderer;
        public MonoBehaviour MonoBehaviour;

        protected override void TriggerValid()
        {
            SetUpFadeQuad();
            MonoBehaviour.StartCoroutine(Fade());
        }

        private void SetUpFadeQuad()
        {
            GameObject fadeQuad = null;

            foreach (var mesh in CameraObject.GetComponentsInChildren<MeshFilter>())
            {
                if (mesh.gameObject.name == "FadeQuad")
                {
                    fadeQuad = mesh.gameObject;
                }
            }

            if (fadeQuad == null)
            {
                fadeQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                fadeQuad.name = "FadeQuad";
                fadeQuad.GetComponent<Renderer>().material = FadeMaterial;
                fadeQuad.transform.parent = CameraObject.transform;
                fadeQuad.transform.rotation = Quaternion.LookRotation(CameraObject.transform.forward, CameraObject.transform.up);
                fadeQuad.transform.localPosition = new Vector3(0f, 0f, CameraObject.nearClipPlane + 0.1f);
            }

            fadeQuad.SetActive(true);
            fadeQuad.GetComponent<MeshRenderer>().material.color = new Color(0, 0, 0, 0);
            _fadeQuadRenderer = fadeQuad.GetComponent<Renderer>();
        }

        private IEnumerator Fade()
        {
            Color startAlpha = _fadeQuadRenderer.material.color;
            var targetAlpha = new Color(0, 0, 0, 1);

            float currentTime = 0;
            while (currentTime < 1 && FadeInDuration != 0f)
            {
                currentTime += Time.deltaTime / FadeInDuration;
                _fadeQuadRenderer.material.color = Color.Lerp(startAlpha, targetAlpha, currentTime);
                yield return null;
            }
            currentTime = 0;

            // Keep the screen dark for the _darkDuration time.
            while (currentTime < 1 && DarkDuration != 0f)
            {
                currentTime += Time.deltaTime / DarkDuration;
                yield return null;
            }
            currentTime = 0;

            while (currentTime < 1 && FadeOutDuration != 0f)
            {
                currentTime += Time.deltaTime / FadeOutDuration;
                _fadeQuadRenderer.material.color = Color.Lerp(targetAlpha, startAlpha, currentTime);
                yield return null;
            }
        }
    }
}
