namespace Logitech.XRToolkit.Interactions
{
    using Logitech.XRToolkit.Actions;
    using Logitech.XRToolkit.Triggers;
    using UnityEngine;

    /// <summary>
    /// Sends a haptic pulse when a specified button is pressed.
    /// </summary>
    public class HapticOnButton : MonoBehaviour
    {
        [SerializeField]
        private InputTrigger _hapticTrigger;
        [SerializeField]
        private HapticAction _hapticAction;

        private void Update()
        {
            _hapticAction.Update(_hapticTrigger.IsValid());
        }
    }
}
