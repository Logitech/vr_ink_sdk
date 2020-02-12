namespace Logitech.XRToolkit.Components
{
    using Logitech.XRToolkit.Inking;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Update material colours using the active colour in Drawing Variables.
    /// </summary>
    /// <remarks>
    /// This can be used to create visual feedback for a device, such as updating the accents of a controller.
    /// </remarks>
    public class UseActiveDrawingColour : MonoBehaviour
    {
        [Serializable]
        private struct ReplacementColour
        {
            public Color ColourToReplace;
            public Color NewColour;
        }

        private Color _activeColour;

        [SerializeField]
        private Material[] _targetMaterials;

        [SerializeField]
        private List<ReplacementColour> _replacementColours;

        private void Start()
        {
            Debug.Assert(DrawingVariables.Instance != null, "There must be only one instance of Drawing Variables in the scene!");
            _activeColour = DrawingVariables.Instance.Colour;
            foreach (var replacementColour in _replacementColours)
            {
                if (_activeColour == replacementColour.ColourToReplace)
                {
                    UpdateColours(replacementColour.NewColour);
                    return;
                }
            }

            UpdateColours(_activeColour);
        }

        private void Update()
        {
            Color color = DrawingVariables.Instance.Colour;
            foreach (Material mat in _targetMaterials)
            {
                if (mat.color != color)
                {
                    _activeColour = color;
                    foreach (var replacementColour in _replacementColours)
                    {
                        if (_activeColour == replacementColour.ColourToReplace)
                        {
                            UpdateColours(replacementColour.NewColour);
                            return;
                        }
                    }
                    UpdateColours(color);
                    return;
                }
            }
        }

        private void UpdateColours(Color color)
        {
            foreach (var mat in _targetMaterials)
            {
                if (mat.shader == Shader.Find("Logitech/Circle"))
                {
                    mat.SetColor("_ForegroundColor", color);
                }

                mat.SetColor("_Color", color);
            }
        }
    }
}
