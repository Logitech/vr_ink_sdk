#pragma once
#include "ControllerManager.h"
#include "FilledCircle.h"
#include "HollowCircle.h"

#define NUMBER_FILLED_CIRCLE_FACES 20
#define VR_INK_PRIMARY_OFFSET_HEIGHT 0.009355f
#define VR_INK_OUTLINE_OFFSET_HEIGHT 0.009255f
#define VR_INK_PRIMARY_OFFSET_Z 0.0428163f
#define VR_INK_TOUCHPAD_Z 0.06180988f
#define VR_INK_TOUCHPAD_MAX_OFFSET_X 0.0055f
#define VR_INK_TOUCHPAD_MAX_OFFSET_Y 0.012f
#define VR_INK_PRIMARY_CIRCLE_RADIUS 0.00455f
#define VR_INK_MENU_CIRCLE_RADIUS 0.00295f
#define VR_INK_TOUCHPAD_CIRCLE_RADIUS 0.001f
#define VR_INK_TOUCHPAD_OUTLINE_RADIUS 0.0055f
#define VR_INK_TOUCHPAD_FOCI_DISTANCE 0.0133f
#define VR_INK_GRIP_OFFSET_X 0.0091f
#define VR_INK_GRIP_OFFSET_Z 0.05090677f
#define VR_INK_GRIP_OUTLINE_RADIUS 0.0054f
#define VR_INK_GRIP_FOCI_DISTANCE 0.0131f
#define VR_INK_MENUBUTTON_OFFSET_Z 0.07930984f
#define VR_INK_MENUBUTTON_OFFSET_Y 0.0089f

class VrInkManager :public ControllerManager
{
public:

	typedef struct _DeviceIO {
		float trackpadX = 0;
		float trackpadY = 0;
		bool trackpadTouch = false;
		bool trackpadClick = false;
		float primaryValue = 0;
		bool primaryTouch = false;
		bool primaryClick = false;
		float tipValue = 0;
		bool tipTouch = false;
		bool tipClick = false;
		bool grip_touch = false;
		bool grip_click = false;
		float gripValue = 0;
		bool applicationMenu = false;
		bool system = false;
		float poseMatrixCalibrated[16];
	} InkStatus;

	VrInkManager(uint32_t index);
	~VrInkManager();

	void UpdateControllerStatus();
	void UpdateButtonHighlights();
	InkStatus ControllerStatus;
	std::string RenderModelName = "";

	void Render(Matrix4 matMVP);

private:
	uint32_t m_nDeviceIndex;

	void ConvertPoseMatrix(vr::HmdMatrix34_t matPose, InkStatus &controllerStatus);
	std::string GetStringProperty(vr::TrackedDeviceIndex_t unDevice, vr::TrackedDeviceProperty prop, vr::TrackedPropertyError *peError = NULL);

	FilledCircle *m_pPrimaryFiller;
	FilledCircle *m_pTouchpadPose;

	HollowCircle *m_pLeftGripHollow;
	HollowCircle *m_pRightGripHollow;
	HollowCircle *m_pPrimaryHollow;
	HollowCircle *m_pTouchpadHollow;
	HollowCircle *m_pMenuButtonHollow;
};
