/* Copyright (c) Logitech Corporation. All rights reserved. Licensed under the MIT License.*/

namespace Logitech.Scripts
{
    using System.Linq;
    using System.Text;
    using UnityEngine;
    using Valve.VR;

    /// <summary>Identifies a primary handedness device, such as VR Ink, among SteamVR tracked devices.</summary>
    public class PrimaryDeviceDetection : MonoBehaviour
    {
        /// <summary>Provide the SteamVR_Behaviour_Pose related to the primary controller (VR Ink).</summary>
        /// <example>
        /// <code>
        /// SteamVR_Action_Boolean _touchInput;
        /// if (_touchInput.GetStateDown(LogitechStylusDetection.Instance.PrimaryDeviceBehaviourPose.inputSource))
        /// {
        ///     print("VR Ink input down");
        /// }
        /// </code>
        /// </example>
        [HideInInspector]
        public static SteamVR_Behaviour_Pose PrimaryDeviceBehaviourPose;

        /// <summary>Provide the SteamVR_Input_Source related to the non-dominant controller.</summary>
        [HideInInspector]
        public static SteamVR_Behaviour_Pose NonDominantDeviceBehaviourPose;

        [SerializeField]
        private Transform _primaryDeviceModel;
        [SerializeField]
        private Transform _nonDominantDeviceModel;
        [SerializeField, Tooltip("Model name of the primary device to identify it in OpenVR.")]
        private string _primaryDeviceModelName;

        /// <summary>Assign the SteamVR Behaviour Poses for each hand.</summary>
        /// <remarks>
        /// By default, the right hand is set as the primary device and the left hand is the non-dominant
        /// device to avoid null reference exceptions.
        /// </remarks>
        private void Start()
        {
            PrimaryDeviceBehaviourPose = FindObjectsOfType<SteamVR_Behaviour_Pose>()
                .First(x => x.inputSource == SteamVR_Input_Sources.RightHand);
            NonDominantDeviceBehaviourPose = FindObjectsOfType<SteamVR_Behaviour_Pose>()
                .First(x => x.inputSource == SteamVR_Input_Sources.LeftHand);
        }

        /// <summary>
        /// Assign the correct custom model to a tracked device, or enable its SteamVR_RenderModel if a custom model is
        /// not provided. Set the PrimaryDeviceBehaviourPose or NonDominantDeviceBehaviourPose with the related SteamVR SteamVR_Behaviour_Pose.
        /// </summary>
        /// <remarks>Should generally be used in SteamVR_Behaviour_Pose Unity Events, or called by a SteamVR broadcast message.</remarks>
        public void AssignController(SteamVR_Behaviour_Pose steamVRBehaviourPose)
        {
            int deviceIndex = steamVRBehaviourPose.GetDeviceIndex();
            if (deviceIndex == -1)
            {
                return;
            }

            // Remove any previously parented model to prevent two models from getting parented to one device.
            Transform parentedModel = steamVRBehaviourPose.GetComponentsInChildren<Transform>()
                .FirstOrDefault(x => x == _primaryDeviceModel || x == _nonDominantDeviceModel);
            if (parentedModel != null)
            {
                parentedModel.parent = null;
                parentedModel.gameObject.SetActive(false);
            }

            string controllerModelName = GetControllerProperty(deviceIndex, ETrackedDeviceProperty.Prop_ModelNumber_String).ToLower();
            SteamVR_RenderModel steamVRRenderModel = steamVRBehaviourPose.GetComponentInChildren<SteamVR_RenderModel>(true);

            if (controllerModelName.Contains(_primaryDeviceModelName.ToLower()))
            {
                if (_primaryDeviceModel != null)
                {
                    PrimaryDeviceBehaviourPose = steamVRBehaviourPose;
                    _primaryDeviceModel.position = steamVRBehaviourPose.transform.position;
                    _primaryDeviceModel.rotation = steamVRBehaviourPose.transform.rotation;
                    _primaryDeviceModel.parent = steamVRBehaviourPose.transform;
                    _primaryDeviceModel.gameObject.SetActive(true);
                    steamVRRenderModel.gameObject.SetActive(false);
                }
                else
                {
                    steamVRRenderModel.gameObject.SetActive(true);
                }
            }
            else
            {
                if (_nonDominantDeviceModel != null)
                {
                    NonDominantDeviceBehaviourPose = steamVRBehaviourPose;
                    _nonDominantDeviceModel.position = steamVRBehaviourPose.transform.position;
                    _nonDominantDeviceModel.rotation = steamVRBehaviourPose.transform.rotation;
                    _nonDominantDeviceModel.parent = steamVRBehaviourPose.transform;
                    _nonDominantDeviceModel.gameObject.SetActive(true);
                    steamVRRenderModel.gameObject.SetActive(false);
                }
                else
                {
                    steamVRRenderModel.gameObject.SetActive(true);
                }
            }
        }

        /// <summary>Interrogate OpenVR about a property in the device property Json file. By default we are interested in the Model Number.</summary>
        /// <param name="deviceIndex">Used by OpenVR to identify any tracked device, from the HMD to controllers and Lighthouses.</param>
        /// <param name="trackedDeviceProperty">The property to retrieve from the device property Json file.</param>
        /// <returns> If it exists, a string matching the value of the property in the property Json file of the device.</returns>
        public static string GetControllerProperty(int deviceIndex, ETrackedDeviceProperty trackedDeviceProperty)
        {
            var stringBuilder = new StringBuilder(1000);
            ETrackedPropertyError trackedPropertyError = ETrackedPropertyError.TrackedProp_Success;
            CVRSystem system = OpenVR.System;
            system.GetStringTrackedDeviceProperty((uint)deviceIndex, trackedDeviceProperty, stringBuilder, 1000, ref trackedPropertyError);
            return stringBuilder.ToString();
        }
    }
}
