#include "SceneRenderer.h"
#include "LogiOGlTools.h"

SceneRenderer::SceneRenderer() :
	m_unSceneProgramID(0),
	m_unRenderModelProgramID(0),
	m_nSceneMatrixLocation(-1),
	m_nRenderModelMatrixLocation(-1)
{
}

SceneRenderer::~SceneRenderer()
{
	delete m_pVrInkManager;
	delete m_pVrInkManagerApi;
	delete m_pViveControllerManager;

	if (m_unSceneProgramID)
	{
		glDeleteProgram(m_unSceneProgramID);
	}
	if (m_unRenderModelProgramID)
	{
		glDeleteProgram(m_unRenderModelProgramID);
	}
}

bool SceneRenderer::CompileShaders()
{
	m_unSceneProgramID = LogiOGlTools::CompileGLShader(
		"Scene",

		// Vertex Shader
		"#version 410\n"
		"uniform mat4 matrix;\n"
		"layout(location = 0) in vec4 position;\n"
		"layout(location = 1) in vec2 v2UVcoordsIn;\n"
		"layout(location = 2) in vec3 v3NormalIn;\n"
		"out vec2 v2UVcoords;\n"
		"void main()\n"
		"{\n"
		"	v2UVcoords = v2UVcoordsIn;\n"
		"	gl_Position = matrix * position;\n"
		"}\n",

		// Fragment Shader
		"#version 410 core\n"
		"uniform sampler2D mytexture;\n"
		"in vec2 v2UVcoords;\n"
		"out vec4 outputColor;\n"
		"void main()\n"
		"{\n"
		"   outputColor = texture(mytexture, v2UVcoords);\n"
		"}\n"
	);
	m_nSceneMatrixLocation = glGetUniformLocation(m_unSceneProgramID, "matrix");
	if (m_nSceneMatrixLocation == -1)
	{
		printf("Unable to find matrix uniform in scene shader\n");
		return false;
	}

	m_unRenderModelProgramID = LogiOGlTools::CompileGLShader(
		"render model",

		// vertex shader
		"#version 410\n"
		"uniform mat4 matrix;\n"
		"layout(location = 0) in vec4 position;\n"
		"layout(location = 1) in vec3 v3NormalIn;\n"
		"layout(location = 2) in vec2 v2TexCoordsIn;\n"
		"out vec2 v2TexCoord;\n"
		"void main()\n"
		"{\n"
		"	v2TexCoord = v2TexCoordsIn;\n"
		"	gl_Position = matrix * vec4(position.xyz, 1);\n"
		"}\n",

		//fragment shader
		"#version 410 core\n"
		"uniform sampler2D diffuse;\n"
		"uniform vec4 tintColor;\n"
		"uniform float colorIntensity;\n"
		"in vec2 v2TexCoord;\n"
		"out vec4 outputColor;\n"
		"void main()\n"
		"{\n"
		"   outputColor = mix(texture( diffuse, v2TexCoord) , tintColor, colorIntensity);\n"
		"   //outputColor = texture( diffuse, v2TexCoord) * tintColor;\n"
		"}\n"

	);
	m_nRenderModelMatrixLocation = glGetUniformLocation(m_unRenderModelProgramID, "matrix");
	if (m_nRenderModelMatrixLocation == -1)
	{
		printf("Unable to find matrix uniform in render model shader\n");
		return false;
	}

	m_nRenderModelTintColorLocation = glGetUniformLocation(m_unRenderModelProgramID, "tintColor");
	if (m_nRenderModelTintColorLocation == -1)
	{
		printf("Unable to find tintColor uniform in render model shader\n");
		return false;
	}

	m_nRenderModelColorIntensityLocation = glGetUniformLocation(m_unRenderModelProgramID, "colorIntensity");
	if (m_nRenderModelColorIntensityLocation == -1)
	{
		printf("Unable to find colorIntensity uniform in render model shader\n");
		return false;
	}

	return m_unSceneProgramID != 0 && m_unRenderModelProgramID != 0;
}

void SceneRenderer::SetupScene(DeviceHandlesInfo devicesHandles[], vr::VRActionSetHandle_t actionSet, uint32_t inkIndex, uint32_t viveIndex, bool bUseVRInkApi)
{
	m_bUseVRInkApi = bUseVRInkApi;
	CompileShaders();

	if (m_bUseVRInkApi)
	{
		m_pVrInkManagerApi = new VrInkManagerApi("{logitech_stylus}/rendermodels/Logitech_VRInk");
		if (m_pVrInkManagerApi->GetRenderModelComponentNames().size() == 0) {
			//if no render model was found, try with sharpie 5 render model (installed with VR Ink driver 2.15)
			delete m_pVrInkManagerApi;
			m_pVrInkManagerApi = new VrInkManagerApi("{logitech_stylus}/rendermodels/Logitech_VRInk");
		}
		m_pVrInkManagerApi->SetShaderParameters(m_unRenderModelProgramID, m_nRenderModelMatrixLocation, m_nRenderModelTintColorLocation,
			m_nRenderModelColorIntensityLocation);
	}
	else
	{
		m_pVrInkManager = new VrInkManager(inkIndex);
		m_pVrInkManager->deviceHandles = devicesHandles[EControllerRole::kStylus];
		m_pVrInkManager->actionSetDriver = actionSet;
		m_pVrInkManager->SetShaderParameters(m_unRenderModelProgramID, m_nRenderModelMatrixLocation, m_nRenderModelTintColorLocation,
			m_nRenderModelColorIntensityLocation);
	}
	m_pViveControllerManager = new ViveControllerManager(viveIndex);
	m_pViveControllerManager->deviceHandles = devicesHandles[EControllerRole::kNonDominent];
	m_pViveControllerManager->actionSetDriver = actionSet;
	m_pViveControllerManager->SetShaderParameters(m_unRenderModelProgramID, m_nRenderModelMatrixLocation, m_nRenderModelTintColorLocation,
		m_nRenderModelColorIntensityLocation);
}

void SceneRenderer::UpdateScene(Matrix4 hmdPose)
{
	m_mHmdPose = hmdPose;
	if (m_bUseVRInkApi)
	{
		m_pVrInkManagerApi->UpdateControllerStatus();
	}
	else
	{
		m_pVrInkManager->UpdateControllerStatus();
	}
	m_pViveControllerManager->UpdateControllerStatus();
}

void SceneRenderer::RenderScene(Matrix4 viewProjectionMatrix)
{
	glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
	glEnable(GL_DEPTH_TEST);

	//Render VRInk
	if (m_bUseVRInkApi)
	{
		m_pVrInkManagerApi->Render(viewProjectionMatrix * m_pVrInkManagerApi->GetPoseMatrix());
	}
	else
	{
		m_pVrInkManager->Render(viewProjectionMatrix * m_pVrInkManager->GetPoseMatrix());
	}
	//Render vive controller
	m_pViveControllerManager->Render(viewProjectionMatrix * m_pViveControllerManager->GetPoseMatrix());

	glUseProgram(0);
}