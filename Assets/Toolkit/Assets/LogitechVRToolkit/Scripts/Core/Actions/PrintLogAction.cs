namespace Logitech.XRToolkit.Actions
{
    using Action = Logitech.XRToolkit.Core.Action;
    using System;
    using UnityEngine;

    /// <summary>
    /// Prints a given message in the console.
    /// </summary>
    [Serializable]
    public class PrintLogAction : Action
    {
        [SerializeField]
        private string _message;

        protected override void TriggerValid()
        {
            Debug.Log(_message);
        }
    }
}
