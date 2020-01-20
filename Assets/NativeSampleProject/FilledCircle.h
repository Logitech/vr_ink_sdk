#pragma once
#include <GL/glew.h>
#define _USE_MATH_DEFINES
#include <math.h>
#include <vector>
#include "Matrices.h"
#include "LogiOGlTools.h"

#define MINIMUM_CIRCLE_SIZE_MODIFIER 0.2f

class FilledCircle
{
public:
	FilledCircle(Vector3 color, float radius, float numFaces);
	~FilledCircle();

	void Render(Matrix4 mvpMatrix, Matrix4 circlePos, GLfloat circleSizeModifier = 1);

private:

	bool CompileShader();
	void SetupGeometry(float x, float y, float z, float radius, int numberOfSides);

	GLuint m_unFilledCircleID;
	GLint m_nColorLocation;
	Vector3 m_aCircleColor;
	GLuint m_unVAO;
	GLuint m_unVBO;
	GLuint m_unIBO;
	GLint m_nPrimaryValLocation;
	std::vector<GLfloat> m_vFanVertex;
	int m_nNumberOfVerticies;
	std::vector<GLuint> m_vIndices;

	GLuint m_nMatrixLocation;
};
