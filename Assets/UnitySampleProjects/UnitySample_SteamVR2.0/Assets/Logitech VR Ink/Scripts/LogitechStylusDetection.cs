/* Copyright (c) Logitech Corporation. All rights reserved. Licensed under the MIT License.*/

namespace LogitechStylus.Scripts
{
    using System.Collections;
    using System.Linq;
    using System.Text;
    using UnityEngine;
    using Valve.VR;

    /// <summary>Identifies VR Ink among SteamVR tracked devices.</summary>
    /// <remarks>If this class cannot find VR Ink, avoid using VRInkInputSource or NonDominantInputSource elsewhere.</remarks>
    public class LogitechStylusDetection : SingletonBehaviour<LogitechStylusDetection>
    {
        /// <summary>
        /// Provide the SteamVR_Input_Source related to VR Ink.
        /// </summary>
        /// <example>
        /// <code>
        /// SteamVR_Action_Boolean _touchInput;
        /// if (_touchInput.GetStateDown(LogitechStylusDetection.Instance.VRInkInputSourc))
        /// {
        ///     print("VR Ink input down");
        /// }
        /// </code>
        /// </example>
        [HideInInspector]
        public SteamVR_Input_Sources VRInkInputSource;

        /// <summary>
        /// Provide the SteamVR_Input_Source related to the other controller.
        /// </summary>
        [HideInInspector]
        public SteamVR_Input_Sources NonDominantInputSource;

        [SerializeField]
        private Transform _vrInkModel;
        [SerializeField]
        private Transform _nonDominantDeviceModel;

        [SerializeField, Tooltip("Model name of VR Ink to help identify it in OpenVR.")]
        private string _stylusModelName;

        private SteamVR_Behaviour_Pose _leftSteamVRBehaviourPose;
        private SteamVR_Behaviour_Pose _rightSteamVRBehaviourPose;
        private SteamVR_RenderModel _leftSteamVRRenderModel;
        private SteamVR_RenderModel _rightSteamVRRenderModel;

        /// <summary>Get the SteamVR Behaviour Poses and Render Models for each hand, and assign the controllers after a delay.</summary>
        /// <remarks>This is a coroutine as the SteamVR API takes a couple of frames to get the proper device index for a controller.</remarks>
        private IEnumerator Start()
        {
            _leftSteamVRBehaviourPose = FindObjectsOfType<SteamVR_Behaviour_Pose>()
                .First(x => x.inputSource == SteamVR_Input_Sources.LeftHand);
            _rightSteamVRBehaviourPose = FindObjectsOfType<SteamVR_Behaviour_Pose>()
                .First(x => x.inputSource == SteamVR_Input_Sources.RightHand);
            _leftSteamVRRenderModel = _leftSteamVRBehaviourPose.GetComponentInChildren<SteamVR_RenderModel>();
            _rightSteamVRRenderModel = _rightSteamVRBehaviourPose.GetComponentInChildren<SteamVR_RenderModel>();
            yield return new WaitForSeconds(0.5f);
            AssignControllers();
        }

        /// <summary>
        /// Find all connected OpenVR devices with the controller TrackedDeviceClass and assign the left and right hand
        /// devices. Uses <see cref="SetControllerRole(ref SteamVR_Input_Sources, SteamVR_Input_Sources, Transform)"/>.
        /// </summary>
        private void AssignControllers()
        {
            bool assignedVRInk = false;
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
                        SetControllerRole(ref VRInkInputSource, SteamVR_Input_Sources.RightHand, _vrInkModel);
                        assignedVRInk = true;
                    }
                    else if (controllerRole == ETrackedControllerRole.LeftHand)
                    {
                        SetControllerRole(ref VRInkInputSource, SteamVR_Input_Sources.LeftHand, _vrInkModel);
                        assignedVRInk = true;
                    }
                }
                else
                {
                    if (controllerRole == ETrackedControllerRole.RightHand)
                    {
                        SetControllerRole(ref NonDominantInputSource, SteamVR_Input_Sources.RightHand, _nonDominantDeviceModel);
                    }
                    else if (controllerRole == ETrackedControllerRole.LeftHand)
                    {
                        SetControllerRole(ref NonDominantInputSource, SteamVR_Input_Sources.LeftHand, _nonDominantDeviceModel);
                    }
                }
            }

            if (!assignedVRInk)
            {
                VRInkInputSource = SteamVR_Input_Sources.Any;
                if (_vrInkModel != null)
                {
                    _vrInkModel.parent = null;
                }
            }
        }

        /// <summary>
        /// Assign the correct custom model to a tracked device, or enable its SteamVR_RenderModel if a custom model is not
        /// provided. Set the VRInkInputSource or NonDominantInputSource with the SteamVR assigned input source.
        /// </summary>
        /// <param name="targetInputSource">The VRInkInputSource or NonDominantInputSource to assign.</param>
        /// <param name="inputSource">The input source of the device which is set by SteamVR.</param>
        /// <param name="customModel">A custom model to override the SteamVR_RenderModel.</param>
        private void SetControllerRole(ref SteamVR_Input_Sources targetInputSource, SteamVR_Input_Sources inputSource, Transform customModel)
        {
            targetInputSource = inputSource;
            SteamVR_Behaviour_Pose steamVRBehaviourPose;
            SteamVR_RenderModel rightSteamVRRenderModel;

            if (inputSource == SteamVR_Input_Sources.LeftHand)
            {
                steamVRBehaviourPose = _leftSteamVRBehaviourPose;
                rightSteamVRRenderModel = _leftSteamVRRenderModel;
            }
            else // if (inputSource == SteamVR_Input_Sources.RightHand)
            {
                steamVRBehaviourPose = _rightSteamVRBehaviourPose;
                rightSteamVRRenderModel = _rightSteamVRRenderModel;
            }

            if (customModel != null)
            {
                customModel.position = steamVRBehaviourPose.transform.position;
                customModel.rotation = steamVRBehaviourPose.transform.rotation;
                customModel.parent = steamVRBehaviourPose.transform;
                rightSteamVRRenderModel.gameObject.SetActive(false);
            }
            else
            {
                rightSteamVRRenderModel.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Interrogate OpenVR about the property in the device property Json file. By default we are interested in the
        /// Model Number.
        /// </summary>
        /// <param name="deviceIndex">Used by OpenVR to identify any tracked device, from the HMD to controllers and Lighthouses.</param>
        /// <param name="trackedDeviceProperty">The property to retrieve from the device property Json file.</param>
        /// <returns> If it exists, a string matching the value of the property in the property Json file of the device.</returns>
        public static string GetControllerProperty(int deviceIndex, ETrackedDeviceProperty trackedDeviceProperty)
        {
            var stringBuilder = new StringBuilder(1000);
            ETrackedPropertyError trackedPropertyError = ETrackedPropertyError.TrackedProp_Success;
            CVRSystem system = OpenVR.System;
            if (system == null)
            {
                return "SteamVR not yet initialized";
            }
            system.GetStringTrackedDeviceProperty((uint) deviceIndex, trackedDeviceProperty, stringBuilder, 1000, ref trackedPropertyError);
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Assign the controllers. Should be used in SteamVR_Behaviour_Pose Unity Events, or called by a SteamVR broadcast message (legacy).
        /// </summary>
        public void SetDeviceIndex()
        {
            AssignControllers();
        }
    }
}
