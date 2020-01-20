#pragma once
#include "ControllerManager.h"
#include <vr_ink_api.h>

class VrInkManagerApi :public ControllerManager
{
public:
	VrInkManagerApi(std::string renderModelName);
	~VrInkManagerApi();
	void UpdateControllerStatus();
	void UpdateButtonHighlights();
	VrInkApi::InkStatus ControllerStatus;
};
