/* Copyright (c) Logitech Corporation. All rights reserved. Licensed under the MIT License.*/

namespace LogiPen.Scripts
{
	using System;
	using System.Collections;
	using UnityEngine;
	using Valve.VR;

	/// <summary>
	/// This is an example of how to use the GetStringDeviceProperty to detect if the Logitech VR Pen is currently on the system. 
	/// </summary>
	public class LogiPenDetection : MonoBehaviour
	{


		[SerializeField] private TrackedObject[] _trackedObjectList;
	
		/// <summary>
		/// This is a coroutine as the the SteamVR api takes a couple of frames to get the proper device index for 
		/// a defined SteamVR_TrackedController 
		/// </summary>
		private IEnumerator Start ()
		{
			foreach (var to in _trackedObjectList)
			{
				to.SetACtive(false);
			}
			
			yield return new WaitForSeconds(0.1f);

			//Replace here with the tracked controler you have chosen
			var penIndex = LogiPen.Instance.Controller.controllerIndex;
			
			foreach (var to in _trackedObjectList)
			{
				to.SetACtive(IsTrackedObjectConnected(penIndex,to));
			}
			
			yield return null;
		}


		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.P))
			{
				ReportOnController(LogiPen.Instance.Controller.controllerIndex, "Vive Pen");
			}
		}

		private bool IsTrackedObjectConnected(uint controllerIndex,TrackedObject to)
		{
			var propertyString = GetControllerProperty(controllerIndex, to.PropertyToCheck);
			return propertyString.Contains(to.PropertyName);
		}
		
		/// <summary>
		 /// Get the property string value from OpenVR
		 /// </summary>
		 /// <param name="deviceIndex"> controller index, you can see theese under SteamVRTrackedObjects </param>
		 /// <param name="p">Property type</param>
		 /// <returns>property string</returns>
		private string GetControllerProperty(uint deviceIndex, Valve.VR.ETrackedDeviceProperty p) {
			System.Text.StringBuilder sbType = new System.Text.StringBuilder(1000);
			Valve.VR.ETrackedPropertyError err = Valve.VR.ETrackedPropertyError.TrackedProp_Success;
			SteamVR.instance.hmd.GetStringTrackedDeviceProperty(deviceIndex, p, sbType, 1000, ref err);
			return sbType.ToString();
		}
		
        /// <summary>
        /// Debug statement if you want to print several different property strings from OpenVR
        /// </summary>
        /// <param name="controllerIndex"></param>
        /// <param name="prefix"></param>
		private void ReportOnController(uint controllerIndex, string prefix) {
			print(prefix+" MANF STRING IS:  "+GetControllerProperty(controllerIndex, Valve.VR.ETrackedDeviceProperty.Prop_ManufacturerName_String));
			print(prefix+" MODEL NO. IS:    "+GetControllerProperty(controllerIndex, Valve.VR.ETrackedDeviceProperty.Prop_ModelNumber_String     ));
			print(prefix+" RENDER MODEL IS: "+GetControllerProperty(controllerIndex, Valve.VR.ETrackedDeviceProperty.Prop_RenderModelName_String ));
	        print(prefix+" SERIAL NUMBER IS: "+GetControllerProperty(controllerIndex, Valve.VR.ETrackedDeviceProperty.Prop_SerialNumber_String ));
		}
        
	}

	/// <summary>
	/// This class allow to store a model with a property string we want to compare against as well as the value of that 
	/// property for the defined 3D model. 
	/// </summary>
	[System.Serializable]
	public class TrackedObject
	{
		public GameObject RenderModel;
		public ETrackedDeviceProperty PropertyToCheck = Valve.VR.ETrackedDeviceProperty.Prop_ManufacturerName_String;
		public string PropertyName;

		public void SetACtive(bool val)
		{
			if (RenderModel == null) return;
			RenderModel.SetActive(val);
		}
	}
}
