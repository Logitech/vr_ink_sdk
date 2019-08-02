namespace Logitech.XRToolkit.Interactions
{
    using Logitech.XRToolkit.Actions;
    using Logitech.XRToolkit.Triggers;
    using Logitech.XRToolkit.IO;
    using UnityEngine;

    /// <summary>
    /// Prints a message to the console when a <see cref="TrackedDevice"/> button is pressed.
    /// </summary>
    public class PrintOnButton : MonoBehaviour
    {
        [SerializeField]
        private InputTrigger _printTrigger;
        [SerializeField]
        private PrintLogAction _printAction;

        private void Update()
        {
            _printAction.Update(_printTrigger.IsValid());
        }
    }
}
