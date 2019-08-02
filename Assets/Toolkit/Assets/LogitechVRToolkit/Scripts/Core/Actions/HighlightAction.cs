namespace Logitech.XRToolkit.Actions
{
    using Action = Logitech.XRToolkit.Core.Action;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    /// <summary>
    /// Highlights an object with an outline.
    /// </summary>
    [Serializable]
    public class HighlightAction : Action
    {
        [HideInInspector]
        public Transform ObjectToHighlight;
        [SerializeField]
        private Material _highlightMat;

        private Component _lastTarget;
        private int _savedRenderQueue;

        protected override void TriggerValid()
        {
            // If the last object to highlight and current object to highlight are the same, do nothing.
            if (_lastTarget == ObjectToHighlight)
            {
                return;
            }

            // If the last object to highlight wasn't the same as the current object to highlight, remove the highlight
            // from the previous object.
            if (_lastTarget != null)
            {
                RemoveHighlight(_lastTarget);
            }

            // Add the highlight to the current object to highlight.
            _lastTarget = ObjectToHighlight;
            AddHighlight(_lastTarget);
        }

        protected override void TriggerInvalid()
        {
            if (_lastTarget != null)
            {
                RemoveHighlight(_lastTarget);
                _lastTarget = null;
            }
        }

        public void SetCurrentHighlightColor(Color color)
        {
            if (_lastTarget == null)
            {
                return;
            }

            Renderer renderer = GetRenderer(_lastTarget);

            if (renderer == null)
            {
                Debug.LogError(_lastTarget + " does not have a renderer component. Cannot edit highlight!");
                return;
            }

            int index = Array.IndexOf(renderer.sharedMaterials, _highlightMat);
            if (index == -1)
            {
                Debug.LogError(_lastTarget + " does not have a highlight to update!");
            }

            renderer.sharedMaterials[index].SetColor("_OutlineColor", color);
        }

        public void SetCurrentHighlightOutline(float outlineWidth)
        {
            if (_lastTarget == null)
            {
                return;
            }

            Renderer renderer = GetRenderer(_lastTarget);

            if (renderer == null)
            {
                Debug.LogError(_lastTarget + " does not have a renderer component. Cannot edit highlight!");
                return;
            }

            int index = Array.IndexOf(renderer.sharedMaterials, _highlightMat);
            if (index == -1)
            {
                Debug.LogError(_lastTarget + " does not have a highlight to update!");
            }

            renderer.sharedMaterials[index].SetFloat("_OutlineWidth", outlineWidth);
        }

        private void AddHighlight(Component target)
        {
            Renderer renderer = GetRenderer(target);

            if (renderer == null)
            {
                Debug.LogError(target + " does not have a renderer component. Cannot edit highlight!");
                return;
            }

            if (!renderer.sharedMaterials.Contains(_highlightMat))
            {
                _savedRenderQueue = renderer.sharedMaterial.renderQueue;
                renderer.sharedMaterial.renderQueue = _highlightMat.renderQueue + 1;
                //_highlightMat.renderQueue = renderer.sharedMaterial.renderQueue - 1;

                List<Material> materialList = renderer.sharedMaterials.ToList();
                materialList.Add(_highlightMat);
                renderer.sharedMaterials = materialList.ToArray();
            }
        }

        private void RemoveHighlight(Component target)
        {
            Renderer renderer = GetRenderer(target);

            if (renderer == null)
            {
                Debug.LogError(target + " does not have a renderer component. Cannot add a highlight!");
                return;
            }

            if (renderer.sharedMaterials.Contains(_highlightMat))
            {
                List<Material> materialList = renderer.sharedMaterials.ToList();
                materialList.Remove(_highlightMat);
                renderer.sharedMaterials = materialList.ToArray();
                renderer.sharedMaterial.renderQueue = _savedRenderQueue;
            }
        }

        private Renderer GetRenderer(Component target)
        {
            Renderer renderer = target.GetComponent<MeshRenderer>();

            if (renderer == null)
            {
                // Trying to fall back to a renderer of target's children.
                renderer = target.GetComponentInChildren<MeshRenderer>();
            }

            return renderer;
        }
    }
}
