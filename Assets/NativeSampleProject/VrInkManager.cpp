#include "VrInkManager.h"
#include "pathtools.h"

VrInkManager::VrInkManager(uint32_t index)
{
	m_nDeviceIndex = index;
	Vector3 color = VR_INK_COLOR;
	m_pPrimaryFiller = new FilledCircle(color, VR_INK_PRIMARY_CIRCLE_RADIUS, 25);
	m_pTouchpadPose = new FilledCircle(color, VR_INK_TOUCHPAD_CIRCLE_RADIUS, 10);

	m_pLeftGripHollow = new HollowCircle(color, VR_INK_GRIP_OUTLINE_RADIUS, 100, true, VR_INK_GRIP_FOCI_DISTANCE);
	m_pRightGripHollow = new HollowCircle(color, VR_INK_GRIP_OUTLINE_RADIUS, 100, true, VR_INK_GRIP_FOCI_DISTANCE);

	m_pTouchpadHollow = new HollowCircle(color, VR_INK_TOUCHPAD_OUTLINE_RADIUS, 100, false, VR_INK_TOUCHPAD_FOCI_DISTANCE);
	m_pPrimaryHollow = new HollowCircle(color, VR_INK_PRIMARY_CIRCLE_RADIUS, 100, false);
	m_pMenuButtonHollow = new HollowCircle(color, VR_INK_MENU_CIRCLE_RADIUS, 100, false);
}

VrInkManager::~VrInkManager()
{
}

void VrInkManager::ConvertPoseMatrix(vr::HmdMatrix34_t matPose, InkStatus &controllerStatus) {
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

std::string VrInkManager::GetStringProperty(vr::TrackedDeviceIndex_t unDevice, vr::TrackedDeviceProperty prop, vr::TrackedPropertyError *peError)
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

void VrInkManager::UpdateControllerStatus()
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

	//Tip Value
	vr::InputAnalogActionData_t tipData;
	eError = vr::VRInput()->GetAnalogActionData(deviceHandles.actionTipAnalogValue, &tipData, sizeof(tipData), deviceHandles.sourceHandle);
	if (eError == vr::VRInputError_None && tipData.bActive)
	{
		ControllerStatus.tipValue = tipData.x;
	}

	//Tip Touch
	eError = vr::VRInput()->GetDigitalActionData(deviceHandles.actionTipDigitalTouch, &digitalData, sizeof(digitalData), deviceHandles.sourceHandle);
	if (eError == vr::VRInputError_None && digitalData.bActive)
	{
		ControllerStatus.tipTouch = digitalData.bState;
	}

	//Tip Click
	eError = vr::VRInput()->GetDigitalActionData(deviceHandles.actionTipDigitalClick, &digitalData, sizeof(digitalData), deviceHandles.sourceHandle);
	if (eError == vr::VRInputError_None && digitalData.bActive)
	{
		ControllerStatus.tipClick = digitalData.bState;
	}

	//Primary Value
	vr::InputAnalogActionData_t triggerData;
	eError = vr::VRInput()->GetAnalogActionData(deviceHandles.actionPrimaryAnalogValue, &triggerData, sizeof(triggerData), deviceHandles.sourceHandle);
	if (eError == vr::VRInputError_None && triggerData.bActive)
	{
		ControllerStatus.primaryValue = triggerData.x;
	}

	//Primary Touch
	eError = vr::VRInput()->GetDigitalActionData(deviceHandles.actionPrimaryDigitalTouch, &digitalData, sizeof(digitalData), deviceHandles.sourceHandle);
	if (eError == vr::VRInputError_None && digitalData.bActive)
	{
		ControllerStatus.primaryTouch = digitalData.bState;
	}

	//Primary Click
	eError = vr::VRInput()->GetDigitalActionData(deviceHandles.actionPrimaryDigitalClick, &digitalData, sizeof(digitalData), deviceHandles.sourceHandle);
	if (eError == vr::VRInputError_None && digitalData.bActive)
	{
		ControllerStatus.primaryClick = digitalData.bState;
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

	//Grip Analog Value
	vr::InputAnalogActionData_t gripData;
	eError = vr::VRInput()->GetAnalogActionData(deviceHandles.actionGripAnalogValue, &gripData, sizeof(gripData), deviceHandles.sourceHandle);
	if (eError == vr::VRInputError_None && gripData.bActive)
	{
		ControllerStatus.gripValue = gripData.x;
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

void VrInkManager::UpdateButtonHighlights()
{
	if (m_ModelComponents.size() > 0) {
		//UpdateButton({ "button_trigger", "button_primary" }, ControllerStatus.primaryValue);
		UpdateButton({ "button_nib", "button_tip" }, ControllerStatus.tipValue);
		UpdateButton({ "button_left_grip" }, ControllerStatus.grip_click);
		UpdateButton({ "button_right_grip" }, ControllerStatus.grip_click);
		UpdateButton({ "button_menu" }, ControllerStatus.applicationMenu);
		UpdateButton({ "button_touch", "button_touchstrip" }, ControllerStatus.trackpadClick);
	}
}

void VrInkManager::Render(Matrix4 matMVP)
{
	ControllerManager::Render(matMVP);
	Matrix4 circlePos;

	if (ControllerStatus.primaryValue > 0)
	{
		circlePos = Matrix4();
		circlePos.rotateX(-90.0f);
		circlePos.translate(0.0f, VR_INK_PRIMARY_OFFSET_HEIGHT, VR_INK_PRIMARY_OFFSET_Z);
		m_pPrimaryFiller->Render(matMVP, circlePos, ControllerStatus.primaryValue);
	}
	if (ControllerStatus.trackpadTouch)
	{
		circlePos = Matrix4();
		circlePos.rotateX(-90.0f);
		circlePos.translate(ControllerStatus.trackpadX * VR_INK_TOUCHPAD_MAX_OFFSET_X, VR_INK_PRIMARY_OFFSET_HEIGHT, VR_INK_TOUCHPAD_Z - (ControllerStatus.trackpadY * VR_INK_TOUCHPAD_MAX_OFFSET_Y));
		m_pTouchpadPose->Render(matMVP, circlePos);
	}

	// Render the buttons outline
	Matrix4 ellipisePos;

	ellipisePos = Matrix4();
	ellipisePos.rotateX(-90.0f);
	ellipisePos.translate(0.0f, VR_INK_OUTLINE_OFFSET_HEIGHT, VR_INK_PRIMARY_OFFSET_Z);
	m_pPrimaryHollow->Render(matMVP, ellipisePos);

	ellipisePos = Matrix4();
	ellipisePos.rotateX(-90.0f);
	ellipisePos.translate(0.0f, VR_INK_MENUBUTTON_OFFSET_Y, VR_INK_MENUBUTTON_OFFSET_Z);
	m_pMenuButtonHollow->Render(matMVP, ellipisePos);

	ellipisePos = Matrix4();
	ellipisePos.rotateX(-90.0f);
	ellipisePos.translate(0.0f, VR_INK_OUTLINE_OFFSET_HEIGHT, VR_INK_TOUCHPAD_Z);
	m_pTouchpadHollow->Render(matMVP, ellipisePos);

	ellipisePos = Matrix4();
	ellipisePos.rotateZ(-90.0f);
	ellipisePos.rotateY(90.0f);
	ellipisePos.translate(VR_INK_GRIP_OFFSET_X, 0.0f, VR_INK_GRIP_OFFSET_Z);
	m_pRightGripHollow->Render(matMVP, ellipisePos);

	ellipisePos = Matrix4();
	ellipisePos.rotateZ(-90.0f);
	ellipisePos.rotateY(-90.0f);
	ellipisePos.translate(-VR_INK_GRIP_OFFSET_X, 0.0f, VR_INK_GRIP_OFFSET_Z);
	m_pLeftGripHollow->Render(matMVP, ellipisePos);
}