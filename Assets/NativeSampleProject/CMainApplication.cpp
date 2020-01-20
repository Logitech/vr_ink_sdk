#include "CMainApplication.h"
#include "LogiOGlTools.h"
#include <sstream>
#include <cctype>
#include <algorithm>

#define APP_VERSION_MAJOR "1"
#define APP_VERSION_MINOR "0"
#define DEVICE_ACTIONS_FILE "actions.json"

static bool g_bPrintf = true;
//-----------------------------------------------------------------------------
// Purpose: Outputs a set of optional arguments to debugging output, using
//          the printf format setting specified in fmt*.
//-----------------------------------------------------------------------------
void dprintf(const char *fmt, ...)
{
	va_list args;
	char buffer[2048];

	va_start(args, fmt);
	vsprintf_s(buffer, fmt, args);
	va_end(args);

	if (g_bPrintf)
		printf("%s", buffer);

	OutputDebugStringA(buffer);
}

CMainApplication::CMainApplication(int argc, char * argv[], SceneRenderer * sceneRenderer) :
	m_pCompanionWindow(NULL),
	m_pContext(NULL),
	m_nCompanionWindowWidth(640),
	m_nCompanionWindowHeight(320),
	m_unCompanionWindowProgramID(0),
	m_pHMD(NULL),
	m_bDebugOpenGL(false),
	m_bVblank(false),
	m_bGlFinishHack(true),
	m_bUseVRInkApi(false)
{
	m_pSceneRenderer = sceneRenderer;
	for (int i = 1; i < argc; i++)
	{
		if (!stricmp(argv[i], "-gldebug"))
		{
			m_bDebugOpenGL = true;
		}
		else if (!stricmp(argv[i], "-novblank"))
		{
			m_bVblank = false;
		}
		else if (!stricmp(argv[i], "-noglfinishhack"))
		{
			m_bGlFinishHack = false;
		}
		else if (!stricmp(argv[i], "-noprintf"))
		{
			g_bPrintf = false;
		}
	}
}

//-----------------------------------------------------------------------------
// Purpose: Destructor
//-----------------------------------------------------------------------------
CMainApplication::~CMainApplication()
{
	// work is done in Shutdown
	dprintf("Shutdown");
}

//-----------------------------------------------------------------------------
// Purpose: Helper to get a string from a tracked device property and turn it
//			into a std::string
//-----------------------------------------------------------------------------
std::string GetTrackedDeviceString(vr::TrackedDeviceIndex_t unDevice, vr::TrackedDeviceProperty prop, vr::TrackedPropertyError *peError = NULL)
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

bool CMainApplication::BInit()
{
	if (SDL_Init(SDL_INIT_VIDEO | SDL_INIT_TIMER) < 0)
	{
		printf("%s - SDL could not initialize! SDL Error: %s\n", __FUNCTION__, SDL_GetError());
		return false;
	}

	// Loading the SteamVR Runtime
	vr::EVRInitError eError = vr::VRInitError_None;
	m_pHMD = vr::VR_Init(&eError, vr::VRApplication_Scene);

	if (eError != vr::VRInitError_None)
	{
		m_pHMD = NULL;
		char buf[1024];
		sprintf_s(buf, sizeof(buf), "Unable to init VR runtime: %s", vr::VR_GetVRInitErrorAsEnglishDescription(eError));
		SDL_ShowSimpleMessageBox(SDL_MESSAGEBOX_ERROR, "VR_Init Failed", buf, NULL);
		return false;
	}

	int nWindowPosX = 700;
	int nWindowPosY = 100;
	Uint32 unWindowFlags = SDL_WINDOW_OPENGL | SDL_WINDOW_SHOWN;

	SDL_GL_SetAttribute(SDL_GL_CONTEXT_MAJOR_VERSION, 4);
	SDL_GL_SetAttribute(SDL_GL_CONTEXT_MINOR_VERSION, 1);
	SDL_GL_SetAttribute(SDL_GL_CONTEXT_PROFILE_MASK, SDL_GL_CONTEXT_PROFILE_CORE);

	SDL_GL_SetAttribute(SDL_GL_MULTISAMPLEBUFFERS, 0);
	SDL_GL_SetAttribute(SDL_GL_MULTISAMPLESAMPLES, 0);
	if (m_bDebugOpenGL)
		SDL_GL_SetAttribute(SDL_GL_CONTEXT_FLAGS, SDL_GL_CONTEXT_DEBUG_FLAG);

	m_pCompanionWindow = SDL_CreateWindow("VR Ink", nWindowPosX, nWindowPosY, m_nCompanionWindowWidth, m_nCompanionWindowHeight, unWindowFlags);
	if (m_pCompanionWindow == NULL)
	{
		printf("%s - Window could not be created! SDL Error: %s\n", __FUNCTION__, SDL_GetError());
		return false;
	}

	m_pContext = SDL_GL_CreateContext(m_pCompanionWindow);
	if (m_pContext == NULL)
	{
		printf("%s - OpenGL context could not be created! SDL Error: %s\n", __FUNCTION__, SDL_GetError());
		return false;
	}

	glewExperimental = GL_TRUE;
	GLenum nGlewError = glewInit();
	if (nGlewError != GLEW_OK)
	{
		printf("%s - Error initializing GLEW! %s\n", __FUNCTION__, glewGetErrorString(nGlewError));
		return false;
	}
	glGetError(); // to clear the error caused deep in GLEW

	if (SDL_GL_SetSwapInterval(m_bVblank ? 1 : 0) < 0)
	{
		printf("%s - Warning: Unable to set VSync! SDL Error: %s\n", __FUNCTION__, SDL_GetError());
		return false;
	}

	m_strDriver = "No Driver";
	m_strDisplay = "No Display";

	m_strDriver = GetTrackedDeviceString(vr::k_unTrackedDeviceIndex_Hmd, vr::Prop_TrackingSystemName_String);
	m_strDisplay = GetTrackedDeviceString(vr::k_unTrackedDeviceIndex_Hmd, vr::Prop_SerialNumber_String);

	m_sWindowTitle = "VR Ink Sample App V" + std::string(APP_VERSION_MAJOR) + "." + std::string(APP_VERSION_MINOR);
	SDL_SetWindowTitle(m_pCompanionWindow, m_sWindowTitle.c_str());

	if (!BInitGL())
	{
		printf("%s - Unable to initialize OpenGL!\n", __FUNCTION__);
		return false;
	}

	if (!BInitCompositor())
	{
		printf("%s - Failed to initialize VR Compositor!\n", __FUNCTION__);
		return false;
	}

	return true;
}

//-----------------------------------------------------------------------------
// Purpose: Outputs the string in message to debugging output.
//          All other parameters are ignored.
//          Does not return any meaningful value or reference.
//-----------------------------------------------------------------------------
void APIENTRY DebugCallback(GLenum source, GLenum type, GLuint id, GLenum severity, GLsizei length, const char* message, const void* userParam)
{
	dprintf("GL Error: %s\n", message);
}

//-----------------------------------------------------------------------------
// Purpose: Initialize OpenGL. Returns true if OpenGL has been successfully
//          initialized, false if shaders could not be created.
//          If failure occurred in a module other than shaders, the function
//          may return true or throw an error.
//-----------------------------------------------------------------------------
bool CMainApplication::BInitGL()
{
	m_fNearClip = 0.1f;
	m_fFarClip = 30.0f;

	if (m_bDebugOpenGL)
	{
		glDebugMessageCallback((GLDEBUGPROC)DebugCallback, nullptr);
		glDebugMessageControl(GL_DONT_CARE, GL_DONT_CARE, GL_DONT_CARE, 0, nullptr, GL_TRUE);
		glEnable(GL_DEBUG_OUTPUT_SYNCHRONOUS);
	}

	if (!CreateAllShaders())
	{
		return false;
	}

	if (m_pHMD) {
		uint32_t vrInkIndex = GetTrackedDeviceVRIndex("logitech_stylus");
		uint32_t viveIndex = GetTrackedDeviceVRIndex("vive controller");
		InitSteamVRInput(vrInkIndex, viveIndex);

		m_pSceneRenderer->SetupScene(m_aDevicesHandles, m_pActionSetDriver, vrInkIndex, viveIndex, m_bUseVRInkApi);
	}

	SetupCameras();
	SetupStereoRenderTargets();
	SetupCompanionWindow();

	return true;
}

//-----------------------------------------------------------------------------
// Purpose: Initialize Compositor. Returns true if the compositor was
//          successfully initialized, false otherwise.
//-----------------------------------------------------------------------------
bool CMainApplication::BInitCompositor()
{
	vr::EVRInitError peError = vr::VRInitError_None;

	if (!vr::VRCompositor())
	{
		printf("Compositor initialization failed. See log file for details\n");
		return false;
	}

	return true;
}

void CMainApplication::Shutdown()
{
	if (m_pHMD)
	{
		vr::VR_Shutdown();
		m_pHMD = NULL;
	}

	if (m_pSceneRenderer)
	{
		delete m_pSceneRenderer;
	}

	if (m_pContext)
	{
		if (m_bDebugOpenGL)
		{
			glDebugMessageControl(GL_DONT_CARE, GL_DONT_CARE, GL_DONT_CARE, 0, nullptr, GL_FALSE);
			glDebugMessageCallback(nullptr, nullptr);
		}

		if (m_unCompanionWindowProgramID)
		{
			glDeleteProgram(m_unCompanionWindowProgramID);
		}

		glDeleteRenderbuffers(1, &leftEyeDesc.m_nDepthBufferId);
		glDeleteTextures(1, &leftEyeDesc.m_nRenderTextureId);
		glDeleteFramebuffers(1, &leftEyeDesc.m_nRenderFramebufferId);
		glDeleteTextures(1, &leftEyeDesc.m_nResolveTextureId);
		glDeleteFramebuffers(1, &leftEyeDesc.m_nResolveFramebufferId);

		glDeleteRenderbuffers(1, &rightEyeDesc.m_nDepthBufferId);
		glDeleteTextures(1, &rightEyeDesc.m_nRenderTextureId);
		glDeleteFramebuffers(1, &rightEyeDesc.m_nRenderFramebufferId);
		glDeleteTextures(1, &rightEyeDesc.m_nResolveTextureId);
		glDeleteFramebuffers(1, &rightEyeDesc.m_nResolveFramebufferId);

		if (m_unCompanionWindowVAO != 0)
		{
			glDeleteVertexArrays(1, &m_unCompanionWindowVAO);
		}
	}

	if (m_pCompanionWindow)
	{
		SDL_DestroyWindow(m_pCompanionWindow);
		m_pCompanionWindow = NULL;
	}

	SDL_Quit();
}

bool CMainApplication::HandleInput()
{
	SDL_Event sdlEvent;
	bool bReturn = false;

	while (SDL_PollEvent(&sdlEvent) != 0)
	{
		if (sdlEvent.type == SDL_QUIT)
		{
			bReturn = true;
		}
		else if (sdlEvent.type == SDL_KEYDOWN)
		{
			if (sdlEvent.key.keysym.sym == SDLK_ESCAPE
				|| sdlEvent.key.keysym.sym == SDLK_q)
			{
				bReturn = true;
			}
		}
	}

	// Process SteamVR events
	vr::VREvent_t event;
	while (m_pHMD->PollNextEvent(&event, sizeof(event)))
	{
		ProcessVREvent(event);
	}

	return bReturn;
}

void CMainApplication::RunMainLoop()
{
	bool bQuit = false;

	SDL_StartTextInput();
	SDL_ShowCursor(SDL_DISABLE);

	while (!bQuit)
	{
		bQuit = HandleInput();
		m_pSceneRenderer->UpdateScene(m_mHmdPose);
		RenderFrame();
		std::string windowTitle = m_sWindowTitle;
		SDL_SetWindowTitle(m_pCompanionWindow, windowTitle.c_str());
	}
	SDL_StopTextInput();
}

void CMainApplication::ProcessVREvent(const vr::VREvent_t & event)
{
	switch (event.eventType)
	{
	case vr::VREvent_TrackedDeviceDeactivated:
	{
		dprintf("Device %u detached.\n", event.trackedDeviceIndex);
	}
	break;
	case vr::VREvent_TrackedDeviceUpdated:
	{
		dprintf("Device %u updated.\n", event.trackedDeviceIndex);
	}
	break;
	}
}

void CMainApplication::RenderFrame()
{
	// for now as fast as possible
	if (m_pHMD)
	{
		RenderStereoTargets();
		RenderCompanionWindow();

		vr::Texture_t leftEyeTexture = { (void*)(uintptr_t)leftEyeDesc.m_nResolveTextureId, vr::TextureType_OpenGL, vr::ColorSpace_Gamma };
		vr::VRCompositor()->Submit(vr::Eye_Left, &leftEyeTexture);
		vr::Texture_t rightEyeTexture = { (void*)(uintptr_t)rightEyeDesc.m_nResolveTextureId, vr::TextureType_OpenGL, vr::ColorSpace_Gamma };
		vr::VRCompositor()->Submit(vr::Eye_Right, &rightEyeTexture);
	}

	if (m_bVblank && m_bGlFinishHack)
	{
		//$ HACKHACK. From gpuview profiling, it looks like there is a bug where two renders and a present
		// happen right before and after the vsync causing all kinds of jittering issues. This glFinish()
		// appears to clear that up. Temporary fix while I try to get Nvidia to investigate this problem.
		// 1/29/2014 mikesart
		glFinish();
	}

	// SwapWindow
	{
		SDL_GL_SwapWindow(m_pCompanionWindow);
	}

	// Clear
	{
		// We want to make sure the glFinish waits for the entire present to complete, not just the submission
		// of the command. So, we do a clear right here so the glFinish will wait fully for the swap.
		glClearColor(0, 0, 0, 1);
		glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
	}

	// Flush and wait for swap.
	if (m_bVblank)
	{
		glFlush();
		glFinish();
	}

	UpdateHMDMatrixPose();
}

bool CMainApplication::CreateAllShaders()
{
	m_unCompanionWindowProgramID = LogiOGlTools::CompileGLShader(
		"CompanionWindow",

		// vertex shader
		"#version 410 core\n"
		"layout(location = 0) in vec4 position;\n"
		"layout(location = 1) in vec2 v2UVIn;\n"
		"noperspective out vec2 v2UV;\n"
		"void main()\n"
		"{\n"
		"	v2UV = v2UVIn;\n"
		"	gl_Position = position;\n"
		"}\n",

		// fragment shader
		"#version 410 core\n"
		"uniform sampler2D mytexture;\n"
		"noperspective in vec2 v2UV;\n"
		"out vec4 outputColor;\n"
		"void main()\n"
		"{\n"
		"		outputColor = texture(mytexture, v2UV);\n"
		"}\n"
	);

	return m_unCompanionWindowProgramID != 0;
	return true;
}

void CMainApplication::SetupCameras()
{
	m_mat4ProjectionLeft = GetHMDMatrixProjectionEye(vr::Eye_Left);
	m_mat4ProjectionRight = GetHMDMatrixProjectionEye(vr::Eye_Right);
	m_mat4eyePosLeft = GetHMDMatrixPoseEye(vr::Eye_Left);
	m_mat4eyePosRight = GetHMDMatrixPoseEye(vr::Eye_Right);
}

//-----------------------------------------------------------------------------
// Purpose: Creates a frame buffer. Returns true if the buffer was set up.
//          Returns false if the setup failed.
//-----------------------------------------------------------------------------
bool CMainApplication::CreateFrameBuffer(int nWidth, int nHeight, FramebufferDesc & framebufferDesc)
{
	glGenFramebuffers(1, &framebufferDesc.m_nRenderFramebufferId);
	glBindFramebuffer(GL_FRAMEBUFFER, framebufferDesc.m_nRenderFramebufferId);

	glGenRenderbuffers(1, &framebufferDesc.m_nDepthBufferId);
	glBindRenderbuffer(GL_RENDERBUFFER, framebufferDesc.m_nDepthBufferId);
	glRenderbufferStorageMultisample(GL_RENDERBUFFER, 4, GL_DEPTH_COMPONENT, nWidth, nHeight);
	glFramebufferRenderbuffer(GL_FRAMEBUFFER, GL_DEPTH_ATTACHMENT, GL_RENDERBUFFER, framebufferDesc.m_nDepthBufferId);

	glGenTextures(1, &framebufferDesc.m_nRenderTextureId);
	glBindTexture(GL_TEXTURE_2D_MULTISAMPLE, framebufferDesc.m_nRenderTextureId);
	glTexImage2DMultisample(GL_TEXTURE_2D_MULTISAMPLE, 4, GL_RGBA8, nWidth, nHeight, true);
	glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D_MULTISAMPLE, framebufferDesc.m_nRenderTextureId, 0);

	glGenFramebuffers(1, &framebufferDesc.m_nResolveFramebufferId);
	glBindFramebuffer(GL_FRAMEBUFFER, framebufferDesc.m_nResolveFramebufferId);

	glGenTextures(1, &framebufferDesc.m_nResolveTextureId);
	glBindTexture(GL_TEXTURE_2D, framebufferDesc.m_nResolveTextureId);
	glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
	glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAX_LEVEL, 0);
	glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA8, nWidth, nHeight, 0, GL_RGBA, GL_UNSIGNED_BYTE, nullptr);
	glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, framebufferDesc.m_nResolveTextureId, 0);

	// check FBO status
	GLenum status = glCheckFramebufferStatus(GL_FRAMEBUFFER);
	if (status != GL_FRAMEBUFFER_COMPLETE)
	{
		return false;
	}

	glBindFramebuffer(GL_FRAMEBUFFER, 0);

	return true;
}

bool CMainApplication::SetupStereoRenderTargets()
{
	if (!m_pHMD)
		return false;

	m_pHMD->GetRecommendedRenderTargetSize(&m_nRenderWidth, &m_nRenderHeight);

	CreateFrameBuffer(m_nRenderWidth, m_nRenderHeight, leftEyeDesc);
	CreateFrameBuffer(m_nRenderWidth, m_nRenderHeight, rightEyeDesc);

	return true;
}

void CMainApplication::SetupCompanionWindow()
{
	if (!m_pHMD)
		return;

	std::vector<VertexDataWindow> vVerts;

	// left eye vertices's
	vVerts.push_back(VertexDataWindow(Vector2(-1, -1), Vector2(0, 0))); //0
	vVerts.push_back(VertexDataWindow(Vector2(0, -1), Vector2(1, 0)));//1
	vVerts.push_back(VertexDataWindow(Vector2(-1, 1), Vector2(0, 1)));//2
	vVerts.push_back(VertexDataWindow(Vector2(0, 1), Vector2(1, 1)));//3

	// right eye vertices's
	vVerts.push_back(VertexDataWindow(Vector2(0, -1), Vector2(0, 0)));//4
	vVerts.push_back(VertexDataWindow(Vector2(1, -1), Vector2(1, 0)));//5
	vVerts.push_back(VertexDataWindow(Vector2(0, 1), Vector2(0, 1)));//6
	vVerts.push_back(VertexDataWindow(Vector2(1, 1), Vector2(1, 1)));//7

	GLushort vIndices[] = { 0, 1, 2,   2, 1, 3,   4, 5, 6,   6, 5, 7 };
	m_uiCompanionWindowIndexSize = _countof(vIndices);

	glGenVertexArrays(1, &m_unCompanionWindowVAO);
	glBindVertexArray(m_unCompanionWindowVAO);

	glGenBuffers(1, &m_glCompanionWindowIDVertBuffer);
	glBindBuffer(GL_ARRAY_BUFFER, m_glCompanionWindowIDVertBuffer);
	glBufferData(GL_ARRAY_BUFFER, vVerts.size() * sizeof(VertexDataWindow), &vVerts[0], GL_STATIC_DRAW);

	glGenBuffers(1, &m_glCompanionWindowIDIndexBuffer);
	glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, m_glCompanionWindowIDIndexBuffer);
	glBufferData(GL_ELEMENT_ARRAY_BUFFER, m_uiCompanionWindowIndexSize * sizeof(GLushort), &vIndices[0], GL_STATIC_DRAW);

	glEnableVertexAttribArray(0);
	glVertexAttribPointer(0, 2, GL_FLOAT, GL_FALSE, sizeof(VertexDataWindow), (void *)offsetof(VertexDataWindow, position));

	glEnableVertexAttribArray(1);
	glVertexAttribPointer(1, 2, GL_FLOAT, GL_FALSE, sizeof(VertexDataWindow), (void *)offsetof(VertexDataWindow, texCoord));

	glBindVertexArray(0);

	glDisableVertexAttribArray(0);
	glDisableVertexAttribArray(1);

	glBindBuffer(GL_ARRAY_BUFFER, 0);
	glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);
}

void CMainApplication::RenderStereoTargets()
{
	glClearColor(0.0f, 0.0f, 0.0f, 1.0f);
	glEnable(GL_MULTISAMPLE);

	// Left Eye
	glBindFramebuffer(GL_FRAMEBUFFER, leftEyeDesc.m_nRenderFramebufferId);
	glViewport(0, 0, m_nRenderWidth, m_nRenderHeight);
	RenderScene(vr::Eye_Left);
	glBindFramebuffer(GL_FRAMEBUFFER, 0);

	glDisable(GL_MULTISAMPLE);

	glBindFramebuffer(GL_READ_FRAMEBUFFER, leftEyeDesc.m_nRenderFramebufferId);
	glBindFramebuffer(GL_DRAW_FRAMEBUFFER, leftEyeDesc.m_nResolveFramebufferId);

	glBlitFramebuffer(0, 0, m_nRenderWidth, m_nRenderHeight, 0, 0, m_nRenderWidth, m_nRenderHeight,
		GL_COLOR_BUFFER_BIT,
		GL_LINEAR);

	glBindFramebuffer(GL_READ_FRAMEBUFFER, 0);
	glBindFramebuffer(GL_DRAW_FRAMEBUFFER, 0);

	glEnable(GL_MULTISAMPLE);

	// Right Eye
	glBindFramebuffer(GL_FRAMEBUFFER, rightEyeDesc.m_nRenderFramebufferId);
	glViewport(0, 0, m_nRenderWidth, m_nRenderHeight);
	RenderScene(vr::Eye_Right);
	glBindFramebuffer(GL_FRAMEBUFFER, 0);

	glDisable(GL_MULTISAMPLE);

	glBindFramebuffer(GL_READ_FRAMEBUFFER, rightEyeDesc.m_nRenderFramebufferId);
	glBindFramebuffer(GL_DRAW_FRAMEBUFFER, rightEyeDesc.m_nResolveFramebufferId);

	glBlitFramebuffer(0, 0, m_nRenderWidth, m_nRenderHeight, 0, 0, m_nRenderWidth, m_nRenderHeight,
		GL_COLOR_BUFFER_BIT,
		GL_LINEAR);

	glBindFramebuffer(GL_READ_FRAMEBUFFER, 0);
	glBindFramebuffer(GL_DRAW_FRAMEBUFFER, 0);
}

//-----------------------------------------------------------------------------
// Purpose: Renders a scene with respect to nEye.
//-----------------------------------------------------------------------------
void CMainApplication::RenderScene(vr::Hmd_Eye nEye)
{
	if (m_pSceneRenderer) m_pSceneRenderer->RenderScene(GetCurrentViewProjectionMatrix(nEye));
}

void CMainApplication::RenderCompanionWindow()
{
	glDisable(GL_DEPTH_TEST);
	glViewport(0, 0, m_nCompanionWindowWidth, m_nCompanionWindowHeight);

	glBindVertexArray(m_unCompanionWindowVAO);
	glUseProgram(m_unCompanionWindowProgramID);

	// render left eye (first half of index array )
	glBindTexture(GL_TEXTURE_2D, leftEyeDesc.m_nResolveTextureId);
	glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE);
	glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_EDGE);
	glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
	glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
	glDrawElements(GL_TRIANGLES, m_uiCompanionWindowIndexSize / 2, GL_UNSIGNED_SHORT, 0);

	// render right eye (second half of index array )
	glBindTexture(GL_TEXTURE_2D, rightEyeDesc.m_nResolveTextureId);
	glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE);
	glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_EDGE);
	glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
	glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
	glDrawElements(GL_TRIANGLES, m_uiCompanionWindowIndexSize / 2, GL_UNSIGNED_SHORT, (const void *)(uintptr_t)(m_uiCompanionWindowIndexSize));

	glBindVertexArray(0);
	glUseProgram(0);
}

//-----------------------------------------------------------------------------
// Purpose: Gets a Matrix Projection Eye with respect to nEye.
//-----------------------------------------------------------------------------
Matrix4 CMainApplication::GetHMDMatrixProjectionEye(vr::Hmd_Eye nEye)
{
	if (!m_pHMD)
		return Matrix4();

	vr::HmdMatrix44_t mat = m_pHMD->GetProjectionMatrix(nEye, m_fNearClip, m_fFarClip);

	return Matrix4(
		mat.m[0][0], mat.m[1][0], mat.m[2][0], mat.m[3][0],
		mat.m[0][1], mat.m[1][1], mat.m[2][1], mat.m[3][1],
		mat.m[0][2], mat.m[1][2], mat.m[2][2], mat.m[3][2],
		mat.m[0][3], mat.m[1][3], mat.m[2][3], mat.m[3][3]
	);
}

//-----------------------------------------------------------------------------
// Purpose: Gets an HMDMatrixPoseEye with respect to nEye.
//-----------------------------------------------------------------------------
Matrix4 CMainApplication::GetHMDMatrixPoseEye(vr::Hmd_Eye nEye)
{
	if (!m_pHMD)
		return Matrix4();

	vr::HmdMatrix34_t matEyeRight = m_pHMD->GetEyeToHeadTransform(nEye);
	Matrix4 matrixObj(
		matEyeRight.m[0][0], matEyeRight.m[1][0], matEyeRight.m[2][0], 0.0,
		matEyeRight.m[0][1], matEyeRight.m[1][1], matEyeRight.m[2][1], 0.0,
		matEyeRight.m[0][2], matEyeRight.m[1][2], matEyeRight.m[2][2], 0.0,
		matEyeRight.m[0][3], matEyeRight.m[1][3], matEyeRight.m[2][3], 1.0f
	);

	return matrixObj.invert();
}

//-----------------------------------------------------------------------------
// Purpose: Gets a Current View Projection Matrix with respect to nEye,
//          which may be an Eye_Left or an Eye_Right.
//-----------------------------------------------------------------------------
Matrix4 CMainApplication::GetCurrentViewProjectionMatrix(vr::Hmd_Eye nEye)
{
	Matrix4 matMVP;
	if (nEye == vr::Eye_Left)
	{
		matMVP = m_mat4ProjectionLeft * m_mat4eyePosLeft * m_mat4HMDPose;
	}
	else if (nEye == vr::Eye_Right)
	{
		matMVP = m_mat4ProjectionRight * m_mat4eyePosRight *  m_mat4HMDPose;
	}

	return matMVP;
}

void CMainApplication::UpdateHMDMatrixPose()
{
	if (!m_pHMD)
		return;

	vr::VRCompositor()->WaitGetPoses(m_rTrackedDevicePose, vr::k_unMaxTrackedDeviceCount, NULL, 0);

	if (m_rTrackedDevicePose[vr::k_unTrackedDeviceIndex_Hmd].bPoseIsValid)
	{
		m_rmat4DevicePose[vr::k_unTrackedDeviceIndex_Hmd] = ConvertSteamVRMatrixToMatrix4(m_rTrackedDevicePose[vr::k_unTrackedDeviceIndex_Hmd].mDeviceToAbsoluteTracking);
		m_mat4HMDPose = m_rmat4DevicePose[vr::k_unTrackedDeviceIndex_Hmd];
		m_mHmdPose = m_mat4HMDPose;
		m_mat4HMDPose.invert();
	}
}

//-----------------------------------------------------------------------------
// Purpose: Converts a SteamVR matrix to our local matrix class
//-----------------------------------------------------------------------------
Matrix4 CMainApplication::ConvertSteamVRMatrixToMatrix4(const vr::HmdMatrix34_t & matPose)
{
	Matrix4 matrixObj(
		matPose.m[0][0], matPose.m[1][0], matPose.m[2][0], 0.0,
		matPose.m[0][1], matPose.m[1][1], matPose.m[2][1], 0.0,
		matPose.m[0][2], matPose.m[1][2], matPose.m[2][2], 0.0,
		matPose.m[0][3], matPose.m[1][3], matPose.m[2][3], 1.0f
	);
	return matrixObj;
}

uint32_t CMainApplication::GetTrackedDeviceVRIndex(std::string modelName)
{
	for (uint32_t i = 0; i < vr::k_unMaxTrackedDeviceCount; i++)
	{
		if (!m_pHMD->IsTrackedDeviceConnected(i))
		{
			continue;
		}
		vr::ETrackedPropertyError eError = vr::TrackedProp_Success;
		vr::ETrackedDeviceProperty eProperty = vr::Prop_ModelNumber_String;
		char deviceName[32];
		m_pHMD->GetStringTrackedDeviceProperty(i, eProperty, deviceName, 1000, &eError);
		std::string deviceNameStr(deviceName);
		// We remove any dot to deal seamlessly between a Vive and a Vive pro controller.
		deviceNameStr.erase(std::remove(deviceNameStr.begin(), deviceNameStr.end(), '.'), deviceNameStr.end());
		std::transform(deviceNameStr.begin(), deviceNameStr.end(), deviceNameStr.begin(), [](unsigned char c)
		{
			return std::tolower(c);
		});
		if (deviceNameStr.find(modelName) != std::string::npos)
		{
			return i;
		}
	}
	return -1;
}

void CMainApplication::InitSteamVRInput(uint32_t inkIndex, uint32_t viveIndex)
{
	// Prepare Action manifest file
	std::string sExecutableDirectory = Path_StripFilename(Path_GetExecutablePath());
	std::string pathToActionManifest = Path_MakeAbsolute(DEVICE_ACTIONS_FILE, sExecutableDirectory);

	std::cout << "Path to action manifest: " << pathToActionManifest << std::endl;
	vrInputError = vr::VRInput()->SetActionManifestPath(pathToActionManifest.c_str());
	std::cout << "vr::VRInput()->SetActionManifestPath: " << vrInputError << std::endl;
	if (vrInputError != vr::EVRInputError::VRInputError_None) return;

	// Action Handle for the new IVRInput
	vrInputError = vr::VRInput()->GetActionSetHandle("/actions/controller", &m_pActionSetDriver);
	std::cout << "vr::VRInput()->GetActionSetHandle: " << vrInputError << std::endl;
	if (vrInputError != vr::EVRInputError::VRInputError_None) return;

	// Assign source Handle for both hand
	AssignHandHandle(inkIndex);

	for (int deviceIndex = 0; deviceIndex < kControllerNumber; ++deviceIndex)
	{
		if (m_bUseVRInkApi)
		{
			continue;
		}
		vr::InputOriginInfo_t originInfo;
		if (vr::VRInput()->GetOriginTrackedDeviceInfo(m_aDevicesHandles[deviceIndex].sourceHandle, &originInfo, sizeof(originInfo))
			== vr::VRInputError_None && originInfo.trackedDeviceIndex != vr::k_unTrackedDeviceIndexInvalid &&
			m_pHMD->IsTrackedDeviceConnected(originInfo.trackedDeviceIndex))
		{
			const ::vr::ETrackedDeviceClass deviceType =
				m_pHMD->GetTrackedDeviceClass(originInfo.trackedDeviceIndex);
			if (deviceType != ::vr::TrackedDeviceClass_Controller &&
				deviceType != ::vr::TrackedDeviceClass_GenericTracker) {
				continue;
			}

			//Pose
			std::string actionName = "/actions/controller/in/pose";
			vrInputError = vr::VRInput()->GetActionHandle("/actions/controller/in/pose", &m_aDevicesHandles[deviceIndex].actionPose);
			std::cout << "vr::VRInput()->GetActionHandle " << actionName << " : " << vrInputError << std::endl;
			if (vrInputError != vr::EVRInputError::VRInputError_None) return;

			//Trackpad
			actionName = "/actions/controller/in/touchstrip_position";
			vrInputError = vr::VRInput()->GetActionHandle(actionName.c_str(), &m_aDevicesHandles[deviceIndex].actionTrackpadAnalogValue);
			std::cout << "vr::VRInput()->GetActionHandle " << actionName << " : " << vrInputError << std::endl;
			if (vrInputError != vr::EVRInputError::VRInputError_None) return;

			actionName = "/actions/controller/in/touchstrip_touch";
			vrInputError = vr::VRInput()->GetActionHandle(actionName.c_str(), &m_aDevicesHandles[deviceIndex].actionTrackpadDigitalTouch);
			std::cout << "vr::VRInput()->GetActionHandle " << actionName << " : " << vrInputError << std::endl;
			if (vrInputError != vr::EVRInputError::VRInputError_None) return;

			actionName = "/actions/controller/in/touchstrip_click";
			vrInputError = vr::VRInput()->GetActionHandle(actionName.c_str(), &m_aDevicesHandles[deviceIndex].actionTrackpadDigitalClick);
			std::cout << "vr::VRInput()->GetActionHandle " << actionName << " : " << vrInputError << std::endl;
			if (vrInputError != vr::EVRInputError::VRInputError_None) return;

			//Trigger
			actionName = "/actions/controller/in/primary_value";
			vrInputError = vr::VRInput()->GetActionHandle(actionName.c_str(), &m_aDevicesHandles[deviceIndex].actionPrimaryAnalogValue);
			std::cout << "vr::VRInput()->GetActionHandle " << actionName << " : " << vrInputError << std::endl;
			if (vrInputError != vr::EVRInputError::VRInputError_None) return;

			actionName = "/actions/controller/in/primary_touch";
			vrInputError = vr::VRInput()->GetActionHandle(actionName.c_str(), &m_aDevicesHandles[deviceIndex].actionPrimaryDigitalTouch);
			std::cout << "vr::VRInput()->GetActionHandle " << actionName << " : " << vrInputError << std::endl;
			if (vrInputError != vr::EVRInputError::VRInputError_None) return;

			actionName = "/actions/controller/in/primary_click";
			vrInputError = vr::VRInput()->GetActionHandle(actionName.c_str(), &m_aDevicesHandles[deviceIndex].actionPrimaryDigitalClick);
			std::cout << "vr::VRInput()->GetActionHandle " << actionName << " : " << vrInputError << std::endl;
			if (vrInputError != vr::EVRInputError::VRInputError_None) return;

			//tip
			actionName = "/actions/controller/in/tip_value";
			vrInputError = vr::VRInput()->GetActionHandle(actionName.c_str(), &m_aDevicesHandles[deviceIndex].actionTipAnalogValue);
			std::cout << "vr::VRInput()->GetActionHandle " << actionName << " : " << vrInputError << std::endl;
			if (vrInputError != vr::EVRInputError::VRInputError_None) return;

			actionName = "/actions/controller/in/tip_touch";
			vrInputError = vr::VRInput()->GetActionHandle(actionName.c_str(), &m_aDevicesHandles[deviceIndex].actionTipDigitalTouch);
			std::cout << "vr::VRInput()->GetActionHandle " << actionName << " : " << vrInputError << std::endl;
			if (vrInputError != vr::EVRInputError::VRInputError_None) return;

			actionName = "/actions/controller/in/tip_click";
			vrInputError = vr::VRInput()->GetActionHandle(actionName.c_str(), &m_aDevicesHandles[deviceIndex].actionTipDigitalClick);
			std::cout << "vr::VRInput()->GetActionHandle " << actionName << " : " << vrInputError << std::endl;
			if (vrInputError != vr::EVRInputError::VRInputError_None) return;

			//Grip
			actionName = "/actions/controller/in/grip";
			vrInputError = vr::VRInput()->GetActionHandle(actionName.c_str(), &m_aDevicesHandles[deviceIndex].actionGripDigitalClick);
			std::cout << "vr::VRInput()->GetActionHandle " << actionName << " : " << vrInputError << std::endl;
			if (vrInputError != vr::EVRInputError::VRInputError_None) return;

			//Application menu
			actionName = "/actions/controller/in/application_menu";
			vrInputError = vr::VRInput()->GetActionHandle(actionName.c_str(), &m_aDevicesHandles[deviceIndex].actionApplicationMenuDigitalClick);
			std::cout << "vr::VRInput()->GetActionHandle " << actionName << " : " << vrInputError << std::endl;
			if (vrInputError != vr::EVRInputError::VRInputError_None) return;

			//System button
			actionName = "/actions/controller/in/system";
			vrInputError = vr::VRInput()->GetActionHandle(actionName.c_str(), &m_aDevicesHandles[deviceIndex].actionSystemDigitalClick);
			std::cout << "vr::VRInput()->GetActionHandle " << actionName << " : " << vrInputError << std::endl;
			if (vrInputError != vr::EVRInputError::VRInputError_None) return;
		}
	}
}

//-----------------------------------------------------------------------------
// Purpose: Make sure the handle assignment is made on the correct hand,
// regardless if the Ink start on the left or right side.
//-----------------------------------------------------------------------------
void CMainApplication::AssignHandHandle(uint32_t inkIndex)
{
	uint32_t index = m_pHMD->GetTrackedDeviceIndexForControllerRole(vr::ETrackedControllerRole::TrackedControllerRole_RightHand);
	if (index == inkIndex)
	{
		if (vr::VRInput()->GetInputSourceHandle("/user/hand/left", &m_aDevicesHandles[EControllerRole::kNonDominent].sourceHandle) !=
			vr::VRInputError_None)
		{
			return;
		}
		if (vr::VRInput()->GetInputSourceHandle("/user/hand/right", &m_aDevicesHandles[EControllerRole::kStylus].sourceHandle) !=
			vr::VRInputError_None || !m_bUseVRInkApi) {
			return;
		}
	}
	else
	{
		if (vr::VRInput()->GetInputSourceHandle("/user/hand/right", &m_aDevicesHandles[EControllerRole::kNonDominent].sourceHandle) !=
			vr::VRInputError_None)
		{
			return;
		}
		if (vr::VRInput()->GetInputSourceHandle("/user/hand/left", &m_aDevicesHandles[EControllerRole::kStylus].sourceHandle) !=
			vr::VRInputError_None || !m_bUseVRInkApi) {
			return;
		}
	}
}