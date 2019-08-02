/* Copyright (c) Logitech Corporation. All rights reserved. Licensed under the MIT License.*/

namespace LogitechStylus.Scripts
{
    using System;
    using System.Collections;
    using System.Text;
    using UnityEngine;
    using Valve.VR;

    /// <summary>
    ///     This is an example of how to use the GetStringDeviceProperty to detect if the Logitech VR Pen is currently on the
    ///     system.
    /// </summary>
    public class LogitechStylusDetection : MonoBehaviour
    {
        [SerializeField] private SteamVR_TrackedObject _steamVRTrackedObject;
        [SerializeField] private TrackedObject[] _trackedObjectList;

        /// <summary>
        ///     This is a coroutine as the SteamVR api takes a couple of frames to get the proper device index for
        ///     a defined SteamVR_TrackedController
        /// </summary>
        private IEnumerator Start()
        {
            yield return new WaitForSeconds(0.5f);


            foreach (TrackedObject to in _trackedObjectList)
            {
                to.SetActive(false);
            }

            uint trackedObjectIndex = (uint)_steamVRTrackedObject.index;

            foreach (TrackedObject to in _trackedObjectList)
            {
                to.SetActive(IsTrackedObjectConnected(trackedObjectIndex, to));
            }

            yield return null;
        }


        private bool IsTrackedObjectConnected(uint controllerIndex, TrackedObject to)
        {
            string propertyString = GetControllerProperty(controllerIndex, to.PropertyToCheck).ToLower();
            return propertyString.Contains(to.PropertyName.ToLower());
        }

        /// <summary>
        /// Interrogate OpenVR to find the corresponding tracked device index of a connected Logitech VR Ink.
        /// This index will then be used by SteamVR to identify the controller.
        /// Note that by default HMD has index 0. The following indices will be assigned to controllers and lighthouses. The order will change between and during each SteamVR session.
        /// </summary>
        /// <param name="modelName"> The name of the controller for which you want to get the index</param>
        /// <param name="returnDefaultIndex"> When set to true, instead of returning an invalid index return the index of a the last valid steamVR Controller</param>
        /// <returns></returns>//
        public static int GetLogitechStylusIndex(string modelName, bool returnDefaultIndex = false)
        {
            int controllerIndex = 0;
            for (int i = 0; i < OpenVR.k_unMaxTrackedDeviceCount; i++)
            {
                controllerIndex = i;
                if (!OpenVR.System.IsTrackedDeviceConnected((uint)i))
                {
                    continue;
                }
                if (OpenVR.System.GetTrackedDeviceClass((uint)i) != ETrackedDeviceClass.Controller)
                {
                    continue;
                }
                ETrackedControllerRole controllerRole = OpenVR.System.GetControllerRoleForTrackedDeviceIndex((uint)i);
                if (controllerRole != ETrackedControllerRole.LeftHand && controllerRole != ETrackedControllerRole.RightHand)
                {
                    continue;
                }
                string detect = GetControllerProperty((uint)i, ETrackedDeviceProperty.Prop_ModelNumber_String).ToLower();
                bool isLogitechStylus = detect.Contains(modelName.ToLower());
                if (isLogitechStylus)
                {
                    return controllerIndex;
                }
            }

            return returnDefaultIndex ? controllerIndex : -1;
        }

        /// <summary>
        ///     Interrogate OpenVR about the property p in the device property Json file.
        ///     By default we are interested in the Model Number.
        /// </summary>
        /// <param name="deviceIndex"> Controller index, you can see these under SteamVRTrackedObjects. These are used by OpenVR to identify any device, from the HMD to controller and to Lighthouses. </param>
        /// <param name="p"> Define the Property we are interested to retrieve  from, the device property Json file.</param>
        /// <returns> If it exists, a string matching the value of the property in the property Json file of the device.</returns>
        public static string GetControllerProperty(uint deviceIndex, ETrackedDeviceProperty p)
        {
            var sbType = new StringBuilder(1000);
            ETrackedPropertyError err = ETrackedPropertyError.TrackedProp_Success;
            SteamVR.instance.hmd.GetStringTrackedDeviceProperty(deviceIndex, p, sbType, 1000, ref err);
            return sbType.ToString();
        }

        /// <summary>
        ///     Debug statement if you want to print several different property strings from OpenVR
        /// </summary>
        /// <param name="controllerIndex"></param>
        /// <param name="prefix"></param>
        private void ReportOnController(uint controllerIndex, string prefix)
        {
            print(prefix + " MANF STRING IS:  " + GetControllerProperty(controllerIndex,
                      ETrackedDeviceProperty.Prop_ManufacturerName_String));
            print(prefix + " MODEL NO. IS:    " + GetControllerProperty(controllerIndex,
                      ETrackedDeviceProperty.Prop_ModelNumber_String));
            print(prefix + " RENDER MODEL IS: " + GetControllerProperty(controllerIndex,
                      ETrackedDeviceProperty.Prop_RenderModelName_String));
            print(prefix + " SERIAL NUMBER IS: " + GetControllerProperty(controllerIndex,
                      ETrackedDeviceProperty.Prop_SerialNumber_String));
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                ReportOnController((uint)_steamVRTrackedObject.index, this.gameObject.name);
            }
        }
    }

    /// <summary>
    ///     This class allow to store a model with a property string we want to compare against as well as the value of that
    ///     property for the defined 3D model.
    /// </summary>
    [Serializable]
    public class TrackedObject
    {
        public string PropertyName;
        public ETrackedDeviceProperty PropertyToCheck = ETrackedDeviceProperty.Prop_ModelNumber_String;
        public GameObject RenderModel;

        public void SetActive(bool isActive)
        {
            if (RenderModel == null)
            {
                return;
            }
            RenderModel.SetActive(isActive);
        }
    }
}
