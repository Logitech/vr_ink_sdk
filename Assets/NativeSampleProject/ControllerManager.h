#pragma once
#include <openvr.h>
#include <map>
#include <string>
#include <vector>
#include "Matrices.h"
#include "CGLRenderModel.h"

#define VR_INK_COLOR Vector3(0.7294f, 0.8274f, 1.0f)

enum EControllerRole
{
	kStylus = 0,
	kNonDominent = 1
};

typedef struct _DeviceHandlesInfo {
	vr::VRInputValueHandle_t sourceHandle;
	vr::VRActionHandle_t actionPose;

	vr::VRActionHandle_t actionTrackpadAnalogValue;
	vr::VRActionHandle_t actionTrackpadDigitalTouch;
	vr::VRActionHandle_t actionTrackpadDigitalClick;

	vr::VRActionHandle_t actionPrimaryAnalogValue;
	vr::VRActionHandle_t actionPrimaryDigitalTouch;
	vr::VRActionHandle_t actionPrimaryDigitalClick;

	vr::VRActionHandle_t actionTipAnalogValue;
	vr::VRActionHandle_t actionTipDigitalTouch;
	vr::VRActionHandle_t actionTipDigitalClick;

	vr::VRActionHandle_t actionGripDigitalTouch;
	vr::VRActionHandle_t actionGripDigitalClick;
	vr::VRActionHandle_t actionGripAnalogValue;
	vr::VRActionHandle_t actionApplicationMenuDigitalClick;
	vr::VRActionHandle_t actionSystemDigitalClick;

	vr::VRActionHandle_t actionHaptics;
} DeviceHandlesInfo;

class ControllerManager
{
public:

	ControllerManager(std::string renderModelName);
	ControllerManager();
	~ControllerManager();
	void Render(Matrix4 matMVP);
	void SetShaderParameters(GLuint shaderId, GLint mvpMatrixLocation, GLint tintColorLocation, GLint intensityColorLocation);
	virtual void UpdateControllerStatus();
	virtual void UpdateButtonHighlights();
	Matrix4 GetPoseMatrix();
	std::vector<std::string>GetRenderModelComponentNames();

	vr::VRActionSetHandle_t actionSetDriver = vr::k_ulInvalidActionSetHandle;
	vr::EVRInputError vrInputError = vr::EVRInputError::VRInputError_NoSteam;
	DeviceHandlesInfo deviceHandles;

protected:
	std::string m_RenderModelName;
	std::vector<std::string> m_vRenderModelComponentNames;
	Matrix4 m_PoseMatrix;
	std::map<std::string, CGLRenderModel *>m_ModelComponents;
	GLuint m_shaderId;
	GLint m_mvpMatrixLocation;
	GLint m_tintColorLocation;
	GLint m_nColorIntensityLocation;
	void LoadRenderModel(std::string renderModelName);
	void UpdateButton(std::vector<std::string> alternativeNames, float value);
};
