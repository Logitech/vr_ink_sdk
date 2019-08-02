/* Copyright (c) Logitech Corporation. All rights reserved. Licensed under the MIT License.*/

namespace LogitechStylus.Scripts
{
    using System.Collections;
    using System.Linq;
    using System.Text;
    using UnityEngine;
#if STEAMVR_ENABLED
    using Valve.VR;

    /// <summary>
    /// Identifies the VR Ink among SteamVR tracked devices and assigns it to the correct hand.
    /// </summary>
    public class LogitechStylusDetection : SingletonBehaviour<LogitechStylusDetection>
    {
        [SerializeField]
        private Transform _primaryDeviceModel;
        [SerializeField]
        private Transform _nonDominantDeviceModel;
        [SerializeField]
        private string _stylusModelName;

        [HideInInspector]
        public SteamVR_Input_Sources VRInkInputSource;

        /// <summary>
        /// This is a coroutine as the SteamVR API takes a couple of frames to get the proper device index for a controller.
        /// </summary>
        private IEnumerator Start()
        {
            yield return new WaitForSeconds(0.5f);
            AssignControllers();
        }

        public void AssignControllers()
        {
            for (int i = 0; i < OpenVR.k_unMaxTrackedDeviceCount; i++)
            {
                if (!OpenVR.System.IsTrackedDeviceConnected((uint) i)||
                    OpenVR.System.GetTrackedDeviceClass((uint) i) != ETrackedDeviceClass.Controller)
                {
                    continue;
                }

                ETrackedControllerRole controllerRole = OpenVR.System.GetControllerRoleForTrackedDeviceIndex((uint)i);
                string controllerModelName = GetControllerProperty(i, ETrackedDeviceProperty.Prop_ModelNumber_String).ToLower();
                if (controllerModelName.Contains(_stylusModelName.ToLower()))
                {
                    if (controllerRole == ETrackedControllerRole.RightHand)
                    {
                        VRInkInputSource = SteamVR_Input_Sources.RightHand;
                        Transform controller = FindObjectsOfType<SteamVR_Behaviour_Pose>()
                            .First(x => x.inputSource == SteamVR_Input_Sources.RightHand).transform;
                        _primaryDeviceModel.transform.position = controller.position;
                        _primaryDeviceModel.transform.rotation = controller.rotation;
                        _primaryDeviceModel.parent = controller;
                    }
                    else if (controllerRole == ETrackedControllerRole.LeftHand)
                    {
                        VRInkInputSource = SteamVR_Input_Sources.LeftHand;
                        Transform controller = FindObjectsOfType<SteamVR_Behaviour_Pose>()
                            .First(x => x.inputSource == SteamVR_Input_Sources.LeftHand).transform;
                        _primaryDeviceModel.transform.position = controller.position;
                        _primaryDeviceModel.transform.rotation = controller.rotation;
                        _primaryDeviceModel.parent = controller;
                    }
                }
                else
                {
                    if (controllerRole == ETrackedControllerRole.RightHand)
                    {
                        Transform controller = FindObjectsOfType<SteamVR_Behaviour_Pose>()
                            .First(x => x.inputSource == SteamVR_Input_Sources.RightHand).transform;
                        _nonDominantDeviceModel.transform.position = controller.position;
                        _nonDominantDeviceModel.transform.rotation = controller.rotation;
                        _nonDominantDeviceModel.parent = controller;
                    }
                    else if (controllerRole == ETrackedControllerRole.LeftHand)
                    {
                        Transform controller = FindObjectsOfType<SteamVR_Behaviour_Pose>()
                            .First(x => x.inputSource == SteamVR_Input_Sources.LeftHand).transform;
                        _nonDominantDeviceModel.transform.position = controller.position;
                        _nonDominantDeviceModel.transform.rotation = controller.rotation;
                        _nonDominantDeviceModel.parent = controller;
                    }
                }
            }
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

        /// <summary>
        /// SetDeviceIndex is a listener of a broadcast message sent by SteamVR_Behaviour_Pose when the Device ID changes.
        /// </summary>
        public void SetDeviceIndex()
        {
            AssignControllers();
        }
    }
#endif
}
