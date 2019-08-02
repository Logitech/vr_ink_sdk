/* Copyright (c) Logitech Corporation. All rights reserved. Licensed under the MIT License.*/

#include "LogitechVRInkLibrary.h"
#include "HeadMountedDisplay.h"
#include "Engine.h"
#include "../../OpenVRSDK/headers/openvr.h"
#include "SteamVRFunctionLibrary.h"

DEFINE_LOG_CATEGORY(Logitech);

bool ULogitechVRInkLibrary::IsVRInk(const UMotionControllerComponent* motionController)
{
	
	//Tracked device ID set by the internal OpenVR system. This will help identify the Logitech VR Ink
	int logitechStylusIndex = -1;

	vr::IVRSystem* SteamVRSystem;
	SteamVRSystem = vr::VRSystem();

	char stringBuffer[512];
	
	for (int i = 0; i < vr::k_unMaxTrackedDeviceCount; i++)
	{
		/*
		The Following checks ensure that you will only get 2 controllers from the OpenVR System
		The two controllers are either the Left or the Right hand
		*/
		if (!SteamVRSystem->IsTrackedDeviceConnected(i))
		{
			continue;
		}
		if (SteamVRSystem->GetTrackedDeviceClass(i) != vr::ETrackedDeviceClass::TrackedDeviceClass_Controller)
		{
			continue;
		}
		vr::ETrackedControllerRole controllerRole = SteamVRSystem->GetControllerRoleForTrackedDeviceIndex(i);
		if (controllerRole != vr::ETrackedControllerRole::TrackedControllerRole_LeftHand && controllerRole != vr::ETrackedControllerRole::TrackedControllerRole_RightHand)
		{
			continue;
		}
		
		uint32 StringBytes = SteamVRSystem->GetStringTrackedDeviceProperty(i, vr::ETrackedDeviceProperty::Prop_ModelNumber_String, stringBuffer, sizeof(stringBuffer));
		FString stringCache = *FString(UTF8_TO_TCHAR(stringBuffer));
		FString controllerRoleString = controllerRole == vr::ETrackedControllerRole::TrackedControllerRole_LeftHand ? TEXT("Left_Hand") : TEXT("Right_Hand");

		UE_LOG(Logitech, Warning, TEXT("Found OpenVr controller id %i | Model is %s | Role is %s "), i, *stringCache, *controllerRoleString);

		if (stringCache.Contains("logitech_"))
		{
			logitechStylusIndex = i;
		}
	}

	if (logitechStylusIndex == -1)
	{
		UE_LOG(Logitech, Warning, TEXT("Their are no Logitech VR Ink currently paired with the system"));
		return false;
	}

	/*
	 When a Logitech VR Ink is decteted we need to find which of the Unreal Engine Motion Controller is the stylus.
	 The UMotionControllerComponent doesn't offer a simple way to relate a SteamVR Device ID to a itself.
	 The idea is to compare the location of the MotionController component using U4 interface with the location of the 
	 Logitech VR Ink using SteamVR's API. 
	*/

	FRotator logitechVRInkRotation_SteamVR;
	FVector logitechVRInkPosition_SteamVR;

	USteamVRFunctionLibrary::GetTrackedDevicePositionAndOrientation(logitechStylusIndex, logitechVRInkPosition_SteamVR, logitechVRInkRotation_SteamVR);

	FVector motionControllerPosition = motionController->GetRelativeTransform().GetLocation();

	FVector delta = logitechVRInkPosition_SteamVR - motionControllerPosition;

	/*
	Depending on the latency on your system this delta value typically range from [0.005 - 0]
	*/
	if (delta.Size() < 0.001f)
	{
		return true;
	}

	return false;
}
