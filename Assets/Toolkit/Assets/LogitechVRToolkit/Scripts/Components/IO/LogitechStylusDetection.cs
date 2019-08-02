namespace Logitech.XRToolkit.IO
{
    using Logitech.XRToolkit.Utils;
    using System.Text;
    using UnityEngine;
#if STEAMVR_ENABLED
    using Valve.VR;

    /// <summary>
    /// Identifies the Stylus among SteamVR tracked devices and assigns it to the correct hand.
    /// </summary>
    public class LogitechStylusDetection : MonoBehaviour
    {
        public static bool IsLogitechStylus;
        [SerializeField]
        private string _modelName;

        public bool IsStylusConnectedAndAssigned()
        {
            for (int i = 0; i < OpenVR.k_unMaxTrackedDeviceCount; i++)
            {
                if (!OpenVR.System.IsTrackedDeviceConnected((uint) i))
                {
                    continue;
                }

                var detect = GetControllerProperty(i, ETrackedDeviceProperty.Prop_ModelNumber_String).ToLower();
                IsLogitechStylus = detect.Contains(_modelName.ToLower());
                if (!IsLogitechStylus || !LogitechToolkitManager.Instance.Devices.ContainsKey(Handedness.Primary) ||
                    !LogitechToolkitManager.Instance.Devices.ContainsKey(Handedness.NonDominant))
                {
                    continue;
                }

                int primaryIndex = LogitechToolkitManager.Instance.Devices[Handedness.Primary].Controller.DeviceIndex;
                if (i != primaryIndex)
                {
                    LogitechToolkitManager.Instance.SwapHandedness();
                }
            }

            return IsLogitechStylus;
        }

        public static string GetControllerProperty(int deviceIndex, ETrackedDeviceProperty p)
        {
            var sbType = new StringBuilder(1000);
            ETrackedPropertyError err = ETrackedPropertyError.TrackedProp_Success;
            CVRSystem system = OpenVR.System;
            if (system == null)
            {
                return "SteamVR not yet initialized";
            }
            system.GetStringTrackedDeviceProperty((uint) deviceIndex, p, sbType, 1000, ref err);
            return sbType.ToString();
        }
    }
#endif
}
