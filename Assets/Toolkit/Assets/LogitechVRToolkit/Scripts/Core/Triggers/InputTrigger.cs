namespace Logitech.XRToolkit.Triggers
{
    using Logitech.XRToolkit.Core;
    using Logitech.XRToolkit.Providers;
    using Logitech.XRToolkit.Utils;
    using UnityEngine;
    using System;

    /// <summary>
    /// Trigger that is valid when a given button is pressed on the provided tracked device.
    /// </summary>
    [Serializable]
    public class InputTrigger : Trigger
    {
        public TrackedDeviceProvider TrackedDeviceProvider;
        public StylusButton StylusButton;
        public EButtonEvent InputButtonEvent;

        public InputTrigger() { }

        public InputTrigger(TrackedDeviceProvider trackedDeviceProvider, StylusButton stylusButton, EButtonEvent inputButtonEvent)
        {
            TrackedDeviceProvider = trackedDeviceProvider;
            StylusButton = stylusButton;
            InputButtonEvent = inputButtonEvent;
        }

        public override bool IsValid()
        {
            switch (InputButtonEvent)
            {
                case EButtonEvent.OnButtonDown:
                    return TrackedDeviceProvider.GetOutput().GetButtonDown(StylusButton);
                case EButtonEvent.OnButton:
                    return TrackedDeviceProvider.GetOutput().GetButton(StylusButton);
                case EButtonEvent.OnButtonUp:
                    return TrackedDeviceProvider.GetOutput().GetButtonUp(StylusButton);
                default:
                    Debug.LogErrorFormat("Unknown event type: {0}", InputButtonEvent);
                    return false;
            }
        }
    }
}
