#include "ViveControllerManager.h"
#include "pathtools.h"

ViveControllerManager::ViveControllerManager(uint32_t index)
{
	m_nDeviceIndex = index;
}

ViveControllerManager::~ViveControllerManager()
{
}

void ViveControllerManager::ConvertPoseMatrix(vr::HmdMatrix34_t matPose, ViveControllerStatus &controllerStatus) {
	controllerStatus.poseMatrixCalibrated[0] = matPose.m[0][0];
	controllerStatus.poseMatrixCalibrated[1] = matPose.m[1][0];
	controllerStatus.poseMatrixCalibrated[2] = matPose.m[2][0];
	controllerStatus.poseMatrixCalibrated[3] = 0;

	controllerStatus.poseMatrixCalibrated[4] = matPose.m[0][1];
	controllerStatus.poseMatrixCalibrated[5] = matPose.m[1][1];
	controllerStatus.poseMatrixCalibrated[6] = matPose.m[2][1];
	controllerStatus.poseMatrixCalibrated[7] = 0;

	controllerStatus.poseMatrixCalibrated[8] = matPose.m[0][2];
	controllerStatus.poseMatrixCalibrated[9] = matPose.m[1][2];
	controllerStatus.poseMatrixCalibrated[10] = matPose.m[2][2];
	controllerStatus.poseMatrixCalibrated[11] = 0;

	controllerStatus.poseMatrixCalibrated[12] = matPose.m[0][3];
	controllerStatus.poseMatrixCalibrated[13] = matPose.m[1][3];
	controllerStatus.poseMatrixCalibrated[14] = matPose.m[2][3];
	controllerStatus.poseMatrixCalibrated[15] = 1.0f;

	m_PoseMatrix.set(controllerStatus.poseMatrixCalibrated);
}

std::string ViveControllerManager::GetStringProperty(vr::TrackedDeviceIndex_t unDevice, vr::TrackedDeviceProperty prop, vr::TrackedPropertyError *peError)
{
	uint32_t unRequiredBufferLen = vr::VRSystem()->GetStringTrackedDeviceProperty(unDevice, prop, NULL, 0, peError);
	if (unRequiredBufferLen == 0)
		return "";

	char *pchBuffer = new char[unRequiredBufferLen];
	unRequiredBufferLen = vr::VRSystem()->GetStringTrackedDeviceProperty(unDevice, prop, pchBuffer, unRequiredBufferLen, peError);
	std::string sResult = pchBuffer;
	delete[] pchBuffer;
	return sResult;
}

void ViveControllerManager::UpdateControllerStatus()
{
	// Process SteamVR action state
	// UpdateActionState is called each frame to update the state of the actions themselves. The application
	// controls which action sets are active with the provided array of VRActiveActionSet_t structs.
	vr::VRActiveActionSet_t actionSet = { 0 };
	actionSet.ulActionSet = actionSetDriver;
	actionSet.ulRestrictedToDevice;
	vr::EVRInputError eError = vr::VRInput()->UpdateActionState(&actionSet, sizeof(actionSet), 1);
	if (eError != vr::VRInputError_None) {
		std::cout << "UpdateActionState error: " << eError << std::endl;
		return;
	}

	vr::InputPoseActionData_t poseData;
	float predictedSecondsFromNow = 0;

	eError = vr::VRInput()->GetPoseActionDataForNextFrame(deviceHandles.actionPose, vr::TrackingUniverseStanding, &poseData, sizeof(poseData), deviceHandles.sourceHandle);
	if (eError == vr::VRInputError_None && poseData.bActive && poseData.pose.bPoseIsValid)
	{
		ConvertPoseMatrix(poseData.pose.mDeviceToAbsoluteTracking, ControllerStatus);

		vr::InputOriginInfo_t originInfo;
		if (vr::VRInput()->GetOriginTrackedDeviceInfo(poseData.activeOrigin, &originInfo, sizeof(originInfo)) == vr::VRInputError_None
			&& originInfo.trackedDeviceIndex != vr::k_unTrackedDeviceIndexInvalid)
		{
			std::string sRenderModelName = GetStringProperty(m_nDeviceIndex, vr::Prop_RenderModelName_String);
			if (sRenderModelName != RenderModelName) {
				RenderModelName = sRenderModelName;
				LoadRenderModel(RenderModelName);
			}
		}
	}
	else {
		return;
	}

	//Trackpad position
	vr::InputAnalogActionData_t analogData;
	eError = vr::VRInput()->GetAnalogActionData(deviceHandles.actionTrackpadAnalogValue, &analogData, sizeof(analogData), deviceHandles.sourceHandle);
	if (eError == vr::VRInputError_None && analogData.bActive)
	{
		ControllerStatus.trackpadX = analogData.x;
		ControllerStatus.trackpadY = analogData.y;
	}

	//Trackpad Touch
	vr::InputDigitalActionData_t digitalData;
	eError = vr::VRInput()->GetDigitalActionData(deviceHandles.actionTrackpadDigitalTouch, &digitalData, sizeof(digitalData), deviceHandles.sourceHandle);
	if (eError == vr::VRInputError_None && digitalData.bActive)
	{
		ControllerStatus.trackpadTouch = digitalData.bState;
	}

	//Trackpad Click
	eError = vr::VRInput()->GetDigitalActionData(deviceHandles.actionTrackpadDigitalClick, &digitalData, sizeof(digitalData), deviceHandles.sourceHandle);
	if (eError == vr::VRInputError_None && digitalData.bActive)
	{
		ControllerStatus.trackpadClick = digitalData.bState;
	}

	//Trigger Value
	vr::InputAnalogActionData_t triggerData;
	eError = vr::VRInput()->GetAnalogActionData(deviceHandles.actionPrimaryAnalogValue, &triggerData, sizeof(triggerData), deviceHandles.sourceHandle);
	if (eError == vr::VRInputError_None && triggerData.bActive)
	{
		ControllerStatus.triggerValue = triggerData.x;
	}

	//Trigger Touch
	eError = vr::VRInput()->GetDigitalActionData(deviceHandles.actionPrimaryDigitalTouch, &digitalData, sizeof(digitalData), deviceHandles.sourceHandle);
	if (eError == vr::VRInputError_None && digitalData.bActive)
	{
		ControllerStatus.triggerTouch = digitalData.bState;
	}

	//Trigger Click
	eError = vr::VRInput()->GetDigitalActionData(deviceHandles.actionPrimaryDigitalClick, &digitalData, sizeof(digitalData), deviceHandles.sourceHandle);
	if (eError == vr::VRInputError_None && digitalData.bActive)
	{
		ControllerStatus.triggerClick = digitalData.bState;
	}

	//Grip Click
	eError = vr::VRInput()->GetDigitalActionData(deviceHandles.actionGripDigitalClick, &digitalData, sizeof(digitalData), deviceHandles.sourceHandle);
	if (eError == vr::VRInputError_None && digitalData.bActive)
	{
		ControllerStatus.grip_click = digitalData.bState;
	}

	//Grip Touch
	eError = vr::VRInput()->GetDigitalActionData(deviceHandles.actionGripDigitalTouch, &digitalData, sizeof(digitalData), deviceHandles.sourceHandle);
	if (eError == vr::VRInputError_None && digitalData.bActive)
	{
		ControllerStatus.grip_touch = digitalData.bState;
	}

	//Application Menu
	eError = vr::VRInput()->GetDigitalActionData(deviceHandles.actionApplicationMenuDigitalClick, &digitalData, sizeof(digitalData), deviceHandles.sourceHandle);
	if (eError == vr::VRInputError_None && digitalData.bActive)
	{
		ControllerStatus.applicationMenu = digitalData.bState;
	}

	//System button
	eError = vr::VRInput()->GetDigitalActionData(deviceHandles.actionSystemDigitalClick, &digitalData, sizeof(digitalData), deviceHandles.sourceHandle);
	if (eError == vr::VRInputError_None)
	{
		ControllerStatus.system = digitalData.bState;
	}

	UpdateButtonHighlights();
}

void ViveControllerManager::UpdateButtonHighlights()
{
	if (m_ModelComponents.size() > 0) {
		UpdateButton({ "trigger" }, ControllerStatus.triggerValue);
		UpdateButton({ "lgrip" }, ControllerStatus.grip_click);
		UpdateButton({ "rgrip" }, ControllerStatus.grip_click);
		UpdateButton({ "button" }, ControllerStatus.applicationMenu);
		UpdateButton({ "trackpad_touch" }, ControllerStatus.trackpadTouch);
	}
}