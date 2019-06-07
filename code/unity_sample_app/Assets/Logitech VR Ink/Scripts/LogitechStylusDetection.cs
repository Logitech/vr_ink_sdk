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
        [SerializeField] private SteamVR_TrackedObject _steamVrTrackedObject;
        [SerializeField] private TrackedObject[] _trackedObjectList;

        /// <summary>
        ///     This is a coroutine as the the SteamVR api takes a couple of frames to get the proper device index for
        ///     a defined SteamVR_TrackedController
        /// </summary>
        private IEnumerator Start()
        {
            yield return new WaitForSeconds(0.5f);


            foreach (var to in _trackedObjectList)
            {
                to.SetActive(false);
            }

            //Replace here with the tracked controller you have chosen
            var trackedObjectIndex = (uint)_steamVrTrackedObject.index;

            foreach (var to in _trackedObjectList)
            {
                to.SetActive(IsTrackedObjectConnected(trackedObjectIndex, to));
            }

            yield return null;
        }


        private void Update()
        {
            if (_steamVrTrackedObject == null)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.P))
            {
                ReportOnController((uint)_steamVrTrackedObject.index,
                    "Controller index " + (uint)_steamVrTrackedObject.index + " ");
            }
        }

        private bool IsTrackedObjectConnected(uint controllerIndex, TrackedObject to)
        {
            var propertyString = GetControllerProperty(controllerIndex, to.PropertyToCheck).ToLower();
            return propertyString.Contains(to.PropertyName.ToLower());
        }

        /// <summary>
        ///     Get the property string value from OpenVR
        /// </summary>
        /// <param name="deviceIndex"> controller index, you can see theese under SteamVRTrackedObjects </param>
        /// <param name="p">Property type</param>
        /// <returns>property string</returns>
        private string GetControllerProperty(uint deviceIndex, ETrackedDeviceProperty p)
        {
            var sbType = new StringBuilder(1000);
            var err = ETrackedPropertyError.TrackedProp_Success;
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
    }

    /// <summary>
    ///     This class allow to store a model with a property string we want to compare against as well as the value of that
    ///     property for the defined 3D model.
    /// </summary>
    [Serializable]
    public class TrackedObject
    {
        public string PropertyName;
        public ETrackedDeviceProperty PropertyToCheck = ETrackedDeviceProperty.Prop_ManufacturerName_String;
        public GameObject RenderModel;

        public void SetActive(bool val)
        {
            if (RenderModel == null)
            {
                return;
            }
            RenderModel.SetActive(val);
        }
    }
}