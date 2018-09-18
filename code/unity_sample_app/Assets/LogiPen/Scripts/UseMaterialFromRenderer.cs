/* Copyright (c) Logitech Corporation. All rights reserved. Licensed under the MIT License.*/

namespace LogiPen.Scripts
{
    using UnityEngine;

    /// <summary>
    ///     Put this onn a gameobject you want to use material of target Renderer on any gameobject
    ///     Updates every frame
    ///     Require RendererComponent
    /// </summary>
    [RequireComponent(typeof(Renderer))]
    public class UseMaterialFromRenderer : MonoBehaviour
    {
        private Renderer _renderer;

        [SerializeField] private Renderer _target;

        private void Start()
        {
            _renderer = GetComponent<Renderer>();
        }

        private void Update()
        {
            if (_target == null)
            {
                return;
            }
            _renderer.materials = _target.materials;
        }
    }
}