#include "FilledCircle.h"

FilledCircle::FilledCircle(Vector3 color, float radius, float numFaces)
{
	m_aCircleColor = color;
	CompileShader();
	SetupGeometry(0.0f, 0.0f, 0.0f, radius, numFaces);
}

FilledCircle::~FilledCircle()
{
}

bool FilledCircle::CompileShader()
{
	m_unFilledCircleID = LogiOGlTools::CompileGLShader(
		"filledCircle",

		// vertex shader
		"#version 410 core\n"
		"uniform float circleSizeModifier;\n"
		"uniform mat4 matrix;\n"
		"layout(location = 0) in vec3 position;\n"
		"void main()\n"
		"{\n"
		"	gl_Position =  matrix * vec4(position.xyz * circleSizeModifier, 1);\n"
		"}\n",
		// Fragment Shader
		"#version 410 core\n"
		"uniform vec3 inputColor;\n"
		"out vec4 fillerColor;\n"
		"void main()\n"
		"{\n"
		"   fillerColor = vec4(inputColor.xyz,1);\n"
		"}\n"
	);

	m_nColorLocation = glGetUniformLocation(m_unFilledCircleID, "inputColor");
	if (m_nColorLocation == -1)
	{
		return false;
	}
	m_nMatrixLocation = glGetUniformLocation(m_unFilledCircleID, "matrix");
	if (m_nMatrixLocation == -1)
	{
		printf("Unable to find matrix uniform in render model shader\n");
		return false;
	}
	m_nPrimaryValLocation = glGetUniformLocation(m_unFilledCircleID, "circleSizeModifier");
	if (m_nPrimaryValLocation == -1)
	{
		return false;
	}

	return false;
}

void FilledCircle::Render(Matrix4 mvpMatrix, Matrix4 circlePos, GLfloat circleSizeModifier)
{
	glBindVertexArray(m_unVAO);
	glUseProgram(m_unFilledCircleID);

	//glUniformMatrix4fv(m_nMatrixLocation, 1, GL_FALSE, (mvpMatrix).get());
	glEnableVertexAttribArray(0);

	glBindBuffer(GL_ARRAY_BUFFER, m_unVBO);
	glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, m_unIBO);

	glVertexAttribPointer(
		0,                  // attribute 0. No particular reason for 0, but must match the layout in the shader.
		3,					// size
		GL_FLOAT,           // type
		GL_FALSE,           // normalized?
		0,                  // stride
		(void*)0            // array buffer offset
	);

	glUniformMatrix4fv(m_nMatrixLocation, 1, GL_FALSE, (mvpMatrix*circlePos).get());
	glUniform3fv(m_nColorLocation, 1, &m_aCircleColor[0]);

	// The start size should be visible from a little less than arms length.
	if (circleSizeModifier < MINIMUM_CIRCLE_SIZE_MODIFIER)
	{
		circleSizeModifier = MINIMUM_CIRCLE_SIZE_MODIFIER;
	}
	glUniform1f(m_nPrimaryValLocation, circleSizeModifier);

	// Draw the triangles in fan!
	glDrawElements(GL_TRIANGLE_FAN, m_nNumberOfVerticies, GL_UNSIGNED_INT, NULL);

	glDisableVertexAttribArray(0);
}

void FilledCircle::SetupGeometry(float x, float y, float z, float radius, int numberOfSides)
{
	m_nNumberOfVerticies = numberOfSides + 2;
	GLfloat doublePi = 2.0f * M_PI;

	// VBO data - Set vertexes
	m_vFanVertex.push_back(x);
	m_vFanVertex.push_back(y);
	m_vFanVertex.push_back(z);

	for (int i = 1; i < m_nNumberOfVerticies; i++)
	{
		m_vFanVertex.push_back(x + (radius * cos(i * doublePi / numberOfSides)));
		m_vFanVertex.push_back(y + (radius * sin(i * doublePi / numberOfSides)));
		m_vFanVertex.push_back(z);
	}

	// IBO data - Set indexes
	for (int i = 0; i < m_nNumberOfVerticies; i++)
	{
		m_vIndices.push_back(i);
	}

	glGenVertexArrays(1, &m_unVAO);
	glBindVertexArray(m_unVAO);

	glGenBuffers(1, &m_unVBO);
	glBindBuffer(GL_ARRAY_BUFFER, m_unVBO);
	glBufferData(GL_ARRAY_BUFFER, m_vFanVertex.size() * sizeof(GLfloat), &m_vFanVertex[0], GL_STATIC_DRAW);

	//Create IBO
	glGenBuffers(1, &m_unIBO);
	glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, m_unIBO);
	glBufferData(GL_ELEMENT_ARRAY_BUFFER, m_vIndices.size() * sizeof(GLuint), &m_vIndices[0], GL_STATIC_DRAW);
}