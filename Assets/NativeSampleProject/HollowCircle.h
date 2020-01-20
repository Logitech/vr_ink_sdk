#pragma once
#include <GL/glew.h>
#define _USE_MATH_DEFINES
#include <math.h>
#include <vector>
#include "Matrices.h"
#include "LogiOGlTools.h"

class HollowCircle
{
public:
	HollowCircle(Vector3 color, float radius, float numFaces, bool isGrabButton, float fociDistance = 0);
	~HollowCircle();

	void Render(Matrix4 mvpMatrix, Matrix4 circlePos);

private:

	bool CompileShader();
	void SetupGeometry(float x, float y, float z, float radius, int numberOfSides, bool isGrabButton, float fociDistance);

	GLuint m_unHollowCircleID;
	GLint m_nColorLocation;
	Vector3 m_aCircleColor;
	GLuint m_unVAO;
	GLuint m_unVBO;
	GLuint m_unIBO;

	std::vector<GLfloat> m_vStripVertex;
	int m_nNumberOfVerticies;
	std::vector<GLuint> m_vIndices;

	GLuint m_nMatrixLocation;
};
