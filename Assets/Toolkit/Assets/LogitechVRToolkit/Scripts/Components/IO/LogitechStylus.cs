namespace Logitech.XRToolkit.Components
{
    using Logitech.XRToolkit.Interactions;
    using Logitech.XRToolkit.IO;
    using Logitech.XRToolkit.Utils;
    using UnityEngine;

    /// <summary>
    /// Used to identify the Stylus.
    /// TODO Currently only supports a single Stylus.
    /// </summary>
    public class LogitechStylus : SingletonBehaviour<LogitechStylus>
    {
        [SerializeField]
        private KeyCode _forceHandSwap = KeyCode.Alpha4;

        private void Update()
        {
            if (Input.GetKeyDown(_forceHandSwap))
            {
                LogitechToolkitManager.Instance.SwapHandedness();
            }
        }

        /// <summary>
        /// Checks if the Stylus is snapped to a surface.
        /// </summary>
        /// <returns>
        /// If the Stylus is snapped to a surface.
        /// </returns>
        public static bool IsStylusSnapped()
        {
            var snapInteraction = Instance.GetComponent<SnapInteraction>();
            return snapInteraction != null && snapInteraction.IsSnapped();
        }
    }
}
