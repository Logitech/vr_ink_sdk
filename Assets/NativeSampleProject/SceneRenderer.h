#pragma once
#include <GL/glew.h>
#include <SDL_opengl.h>
#include <stdio.h>
#include <string>
#include <cstdlib>
#include <openvr.h>
#include "Matrices.h"
#include "Matrices.h"
#include "ViveControllerManager.h"
#include "VrInkManager.h"
#include "VrInkManagerApi.h"

class SceneRenderer
{
public:
	SceneRenderer();
	~SceneRenderer();

	void SetupScene(DeviceHandlesInfo devicesHandles[], vr::VRActionSetHandle_t actionSet, uint32_t inkIndex, uint32_t viveIndex, bool bUseVRInkApi);
	void RenderScene(Matrix4 viewProjectionMatrix);
	void UpdateScene(Matrix4 hmdPose);

private:
	GLuint m_unSceneProgramID;
	GLint m_nSceneMatrixLocation;
	GLuint m_unRenderModelProgramID;
	GLint m_nRenderModelMatrixLocation;
	GLint m_nRenderModelTintColorLocation;
	GLint m_nRenderModelColorIntensityLocation;

	bool m_bUseVRInkApi;

	bool CompileShaders();

	Matrix4 m_mHmdPose;

	VrInkManager *m_pVrInkManager;
	VrInkManagerApi *m_pVrInkManagerApi;
	ViveControllerManager *m_pViveControllerManager;
};
