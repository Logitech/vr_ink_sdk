#include "VrInkManagerApi.h"

VrInkManagerApi::VrInkManagerApi(std::string renderModelName) :ControllerManager(renderModelName)
{
}

VrInkManagerApi::~VrInkManagerApi()
{
}

void VrInkManagerApi::UpdateControllerStatus()
{
	VrInkApi::GetDeviceStatus(ControllerStatus);
	m_PoseMatrix.set(ControllerStatus.poseMatrix);
	UpdateButtonHighlights();
}

void VrInkManagerApi::UpdateButtonHighlights()
{
	if (m_ModelComponents.size() > 0) {
		UpdateButton({ "button_trigger", "button_primary" }, ControllerStatus.primaryValue);
		UpdateButton({ "button_nib", "button_tip" }, ControllerStatus.tipValue);
		UpdateButton({ "button_left_grip" }, ControllerStatus.gripClick);
		UpdateButton({ "button_right_grip" }, ControllerStatus.gripClick);
		UpdateButton({ "button_menu" }, ControllerStatus.applicationMenu);
		UpdateButton({ "button_touch", "button_touchstrip" }, ControllerStatus.touchstripClick);
	}
}