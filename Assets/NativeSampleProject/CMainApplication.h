#pragma once
#include <SDL.h>
#include <GL/glew.h>
#include <SDL_opengl.h>
#include <GL/glu.h>
#include <stdio.h>
#include <string>
#include <cstdlib>
#include <openvr.h>

#include "Matrices.h"
#include "pathtools.h"

#include "SceneRenderer.h"
#include "ControllerManager.h"

#if defined(POSIX)
#include "unistd.h"
#endif

#ifndef _WIN32
#define APIENTRY
#endif

#ifndef _countof
#define _countof(x) (sizeof(x)/sizeof((x)[0]))
#endif

constexpr int kControllerNumber = 2;

class CMainApplication
{
public:
	CMainApplication(int argc, char *argv[], SceneRenderer *sceneRenderer);
	~CMainApplication();
	bool BInit();
	bool BInitGL();
	bool BInitCompositor();

	void Shutdown();

	void RunMainLoop();
	bool HandleInput();
	void ProcessVREvent(const vr::VREvent_t & event);
	void RenderFrame();

	bool SetupStereoRenderTargets();
	void SetupCompanionWindow();
	void SetupCameras();

	void RenderStereoTargets();
	void RenderCompanionWindow();
	void RenderScene(vr::Hmd_Eye nEye);

	Matrix4 GetHMDMatrixProjectionEye(vr::Hmd_Eye nEye);
	Matrix4 GetHMDMatrixPoseEye(vr::Hmd_Eye nEye);
	Matrix4 GetCurrentViewProjectionMatrix(vr::Hmd_Eye nEye);
	void UpdateHMDMatrixPose();

	Matrix4 ConvertSteamVRMatrixToMatrix4(const vr::HmdMatrix34_t &matPose);
	bool CreateAllShaders();

private:
	bool m_bDebugOpenGL;
	bool m_bVblank;
	bool m_bGlFinishHack;

	bool m_bUseVRInkApi;

	vr::IVRSystem *m_pHMD;
	std::string m_strDriver;
	std::string m_strDisplay;
	vr::TrackedDevicePose_t m_rTrackedDevicePose[vr::k_unMaxTrackedDeviceCount];
	Matrix4 m_rmat4DevicePose[vr::k_unMaxTrackedDeviceCount];
	uint32_t GetTrackedDeviceVRIndex(std::string modelName);

private: // SDL bookkeeping
	SDL_Window *m_pCompanionWindow;
	uint32_t m_nCompanionWindowWidth;
	uint32_t m_nCompanionWindowHeight;

	SDL_GLContext m_pContext;

private: // OpenGL bookkeeping
	float m_fNearClip;
	float m_fFarClip;

	GLuint m_unCompanionWindowVAO;
	GLuint m_glCompanionWindowIDVertBuffer;
	GLuint m_glCompanionWindowIDIndexBuffer;
	unsigned int m_uiCompanionWindowIndexSize;

	Matrix4 m_mat4HMDPose;
	Matrix4 m_mHmdPose;
	Matrix4 m_mat4eyePosLeft;
	Matrix4 m_mat4eyePosRight;

	Matrix4 m_mat4ProjectionCenter;
	Matrix4 m_mat4ProjectionLeft;
	Matrix4 m_mat4ProjectionRight;

	struct VertexDataWindow
	{
		Vector2 position;
		Vector2 texCoord;
		VertexDataWindow(const Vector2 & pos, const Vector2 tex) : position(pos), texCoord(tex) {	}
	};

	GLuint m_unCompanionWindowProgramID;

	struct FramebufferDesc
	{
		GLuint m_nDepthBufferId;
		GLuint m_nRenderTextureId;
		GLuint m_nRenderFramebufferId;
		GLuint m_nResolveTextureId;
		GLuint m_nResolveFramebufferId;
	};
	FramebufferDesc leftEyeDesc;
	FramebufferDesc rightEyeDesc;

	bool CreateFrameBuffer(int nWidth, int nHeight, FramebufferDesc &framebufferDesc);

	uint32_t m_nRenderWidth;
	uint32_t m_nRenderHeight;

	SceneRenderer *m_pSceneRenderer;
	std::string m_sWindowTitle;

	void InitSteamVRInput(uint32_t inkIndex, uint32_t viveIndex);

	void AssignHandHandle(uint32_t inkIndex);

	DeviceHandlesInfo m_aDevicesHandles[kControllerNumber];

	vr::VRActionSetHandle_t m_pActionSetDriver = vr::k_ulInvalidActionSetHandle;
	vr::EVRInputError vrInputError = vr::EVRInputError::VRInputError_NoSteam;
};
