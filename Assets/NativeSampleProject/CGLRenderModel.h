#pragma once
#include <string>
#include <openvr.h>
#include <GL/glew.h>
class CGLRenderModel
{
public:
	CGLRenderModel(const std::string & sRenderModelName, const std::string componentName);
	~CGLRenderModel();

	bool BInit(const vr::RenderModel_t & vrModel, const vr::RenderModel_TextureMap_t & vrDiffuseTexture);
	void Cleanup();
	void Draw();
	const std::string & GetName() const { return m_sComponentName; }
	float highlightValue = 0;
private:
	GLuint m_glVertBuffer;
	GLuint m_glIndexBuffer;
	GLuint m_glVertArray;
	GLuint m_glTexture;
	GLsizei m_unVertexCount;
	std::string m_sModelName;
	std::string m_sComponentName;
};
