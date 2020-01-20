#include "ControllerManager.h"
#include <openvr.h>
#include <map>
#include <string>
#include <vector>
#include "Matrices.h"
#include "CGLRenderModel.h"

ControllerManager::ControllerManager(std::string renderModelName)
{
	LoadRenderModel(renderModelName);
}

ControllerManager::ControllerManager()
{
}

ControllerManager::~ControllerManager()
{
	m_ModelComponents.clear();
}

void ControllerManager::SetShaderParameters(GLuint shaderId, GLint mvpMatrixLocation, GLint tintColorLocation, GLint intensityColorLocation)
{
	m_shaderId = shaderId;
	m_mvpMatrixLocation = mvpMatrixLocation;
	m_tintColorLocation = tintColorLocation;
	m_nColorIntensityLocation = intensityColorLocation;
}

void ControllerManager::UpdateControllerStatus()
{
}

void ControllerManager::UpdateButtonHighlights()
{
}

Matrix4 ControllerManager::GetPoseMatrix()
{
	return m_PoseMatrix;
}

std::vector<std::string> ControllerManager::GetRenderModelComponentNames()
{
	return m_vRenderModelComponentNames;
}

void ControllerManager::LoadRenderModel(std::string renderModelName)
{
	m_ModelComponents.clear();
	m_vRenderModelComponentNames.clear();
	uint32_t components = vr::VRRenderModels()->GetComponentCount(renderModelName.c_str());

	for (uint32_t i = 0; i < components; i++) {
		char componentName[255];
		vr::VRRenderModels()->GetComponentName(renderModelName.c_str(), i, componentName, 255);
		std::string component(componentName);
		m_vRenderModelComponentNames.push_back(component);
		char componentRenderModelName[255];
		vr::VRRenderModels()->GetComponentRenderModelName(renderModelName.c_str(), componentName, componentRenderModelName, 255);
		CGLRenderModel *renderModel = new CGLRenderModel(componentRenderModelName, component);
		m_ModelComponents[component] = renderModel;
	}
}

void ControllerManager::UpdateButton(std::vector<std::string> alternativeNames, float value)
{
	for (const auto&name : alternativeNames) {
		if (m_ModelComponents.find(name) != m_ModelComponents.end()) {
			m_ModelComponents[name]->highlightValue = value;
		}
	}
}

void ControllerManager::Render(Matrix4 matMVP)
{
	//Render VR Ink
	glUseProgram(m_shaderId);
	glUniformMatrix4fv(m_mvpMatrixLocation, 1, GL_FALSE, matMVP.get());

	std::map<std::string, CGLRenderModel *>::iterator it;

	for (it = m_ModelComponents.begin(); it != m_ModelComponents.end(); ++it) {
		Vector4 tintColor(VR_INK_COLOR.x, VR_INK_COLOR.y, VR_INK_COLOR.z, 1.0f);
		float intensity = it->second->highlightValue;
		// The default tip of VR Ink should show the current active colour.
		if (it->first == "button_tip")
		{
			intensity = 1.0f;
		}
		tintColor = Vector4(1, 1, 1, 1)*(1.0f - intensity) + tintColor * intensity;
		glUniform4fv(m_tintColorLocation, 1, &tintColor[0]);
		glUniform1f(m_nColorIntensityLocation, intensity);
		it->second->Draw();
	}
}