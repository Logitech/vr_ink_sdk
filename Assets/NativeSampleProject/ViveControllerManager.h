#pragma once
#include "ControllerManager.h"

class ViveControllerManager :public ControllerManager
{
public:
	typedef struct _DeviceIO {
		float trackpadX = 0;
		float trackpadY = 0;
		bool trackpadTouch = false;
		bool trackpadClick = false;
		float triggerValue = 0;
		bool triggerTouch = false;
		bool triggerClick = false;
		bool grip_touch = false;
		bool grip_click = false;
		bool applicationMenu = false;
		bool system = false;
		float poseMatrixCalibrated[16];
	} ViveControllerStatus;

	ViveControllerManager(uint32_t index);
	~ViveControllerManager();

	void UpdateControllerStatus();
	void UpdateButtonHighlights();
	ViveControllerStatus ControllerStatus;
	std::string RenderModelName = "";

private:
	uint32_t m_nDeviceIndex;

	void ConvertPoseMatrix(vr::HmdMatrix34_t matPose, ViveControllerStatus &controllerStatus);
	std::string GetStringProperty(vr::TrackedDeviceIndex_t unDevice, vr::TrackedDeviceProperty prop, vr::TrackedPropertyError *peError = NULL);
};
