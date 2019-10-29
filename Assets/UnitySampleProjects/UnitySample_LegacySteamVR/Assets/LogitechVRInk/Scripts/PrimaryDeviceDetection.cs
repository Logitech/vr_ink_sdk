/* Copyright (c) Logitech Corporation. All rights reserved. Licensed under the MIT License.*/

namespace Logitech.Scripts
{
    using System.Text;
    using UnityEngine;
    using Valve.VR;
    using System.Linq;

    /// <summary>Identifies a primary handedness device, such as VR Ink, among SteamVR tracked devices.</summary>
    public class PrimaryDeviceDetection : MonoBehaviour
    {

        /// <summary>Provide the SteamVR Index related to the primary controller (VR Ink).</summary>
        /// <example>
        /// <code>
        /// if (SteamVR_Controller.Input((int)TrackedDevice.PrimaryIndex).GetPressDown(EVRButtonId.k_EButton_SteamVR_Touchpad))
        /// {
        ///     Debug.Log("Clicked trackpad on primary device");
        /// }
        /// </code>
        /// </example>
        public static int PrimaryIndex = -1;

        [SerializeField, Tooltip("Model name of the primary device to identify it in OpenVR.")]
        private string _primaryDeviceName;

        [Header("Model Prefabs")]
        [SerializeField]
        private GameObject _primaryModel;
        [SerializeField]
        private GameObject _nonDominantModel;

        /// <summary>
        /// Assign the correct custom model to a tracked device, or enable its SteamVR_RenderModel if a custom model is
        /// not provided. Set the Primary Index if the relevant controller is found.
        /// </summary>
        /// <remarks>
        /// This method is called by Steam_VR_ControllerManager in Unity via Broadcast Message. As such this script has
        /// to be a child of the CameraRig, for convenience it is attached to the Controller(Right/Left).
        /// </remarks>
        public void SetDeviceIndex(int index)
        {
            ClearChildren();

            string controllerModelName = GetControllerProperty(index, ETrackedDeviceProperty.Prop_ModelNumber_String).ToLower();
            print(controllerModelName + " " + gameObject.name);
            SteamVR_RenderModel steamVRRenderModel = GetComponentInChildren<SteamVR_RenderModel>(true);
            steamVRRenderModel.gameObject.SetActive(false);

            if (controllerModelName.Contains(_primaryDeviceName.ToLower()))
            {
                PrimaryIndex = index;
                if (_primaryModel == null)
                {
                    steamVRRenderModel.gameObject.SetActive(true);
                    return;
                }

                _primaryModel.SetActive(true);
                _primaryModel.transform.SetParent(this.transform);
                _primaryModel.transform.localPosition = Vector3.zero;
                _primaryModel.transform.localEulerAngles = Vector3.zero;
            }
            else
            {
                if (_nonDominantModel == null)
                {
                    steamVRRenderModel.gameObject.SetActive(true);
                    return;
                }
                _nonDominantModel.SetActive(true);
                _nonDominantModel.transform.SetParent(this.transform);
                _nonDominantModel.transform.localPosition = Vector3.zero;
                _nonDominantModel.transform.localEulerAngles = Vector3.zero;
            }
        }

        /// <summary>Remove any previously parented models after detection.</summary>
        private void ClearChildren()
        {
            Transform parentedModel = null;
            if (parentedModel == null && _primaryModel != null)
            {
                parentedModel = GetComponentsInChildren<Transform>().FirstOrDefault(x => x == _primaryModel.transform);
            }
            if (parentedModel == null && _nonDominantModel != null)
            {
                parentedModel = GetComponentsInChildren<Transform>().FirstOrDefault(x => x == _nonDominantModel.transform);
            }

            if (parentedModel == null)
            {
                return;
            }

            parentedModel.parent = null;
            parentedModel.gameObject.SetActive(false);
        }

        /// <summary>Interrogate OpenVR about a property in the device property Json file. By default we are interested in the Model Number.</summary>
        /// <param name="deviceIndex">Used by OpenVR to identify any tracked device, from the HMD to controllers and Lighthouses.</param>
        /// <param name="trackedDeviceProperty">The property to retrieve from the device property Json file.</param>
        /// <returns>If it exists, a string matching the value of the property in the property Json file of the device.</returns>
        public static string GetControllerProperty(int deviceIndex, ETrackedDeviceProperty trackedDeviceProperty)
        {
            var stringBuilder = new StringBuilder(1000);
            ETrackedPropertyError trackedPropertyError = ETrackedPropertyError.TrackedProp_Success;
            CVRSystem system = OpenVR.System;
            system.GetStringTrackedDeviceProperty((uint)deviceIndex, trackedDeviceProperty, stringBuilder, 1000, ref trackedPropertyError);
            return stringBuilder.ToString();
        }

        /// <summary>Helper function to make it easier to get input from the primary device.</summary>
        /// <returns>Returns the SteamVR_Controller.Device associated with the primary tracked object.</returns>
        public static SteamVR_Controller.Device GetPrimaryInput()
        {
            if (PrimaryIndex < 0)
            {
                return null;
            }
            return SteamVR_Controller.Input(PrimaryIndex);
        }
    }
}
