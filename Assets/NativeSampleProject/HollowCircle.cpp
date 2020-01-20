#include "HollowCircle.h"

HollowCircle::HollowCircle(Vector3 color, float radius, float numFaces, bool isGrabButton, float fociDistance)
{
	m_aCircleColor = color;
	CompileShader();
	SetupGeometry(0.0f, 0.0f, 0.0f, radius, numFaces, isGrabButton, fociDistance);
}

HollowCircle::~HollowCircle()
{
}

bool HollowCircle::CompileShader()
{
	m_unHollowCircleID = LogiOGlTools::CompileGLShader(
		"filledCircle",

		// vertex shader
		"#version 410 core\n"
		"uniform mat4 matrix;\n"
		"layout(location = 0) in vec3 position;\n"
		"void main()\n"
		"{\n"
		"	gl_Position = matrix * vec4(position.xyz, 1);\n"
		"}\n",
		// Fragment Shader
		"#version 410 core\n"
		"uniform vec3 inputColor;\n"
		"out vec4 hollowColor;\n"
		"void main()\n"
		"{\n"
		"   hollowColor = vec4(inputColor.xyz,1);\n"
		"}\n"
	);

	m_nColorLocation = glGetUniformLocation(m_unHollowCircleID, "inputColor");
	if (m_nColorLocation == -1)
	{
		return false;
	}
	m_nMatrixLocation = glGetUniformLocation(m_unHollowCircleID, "matrix");
	if (m_nMatrixLocation == -1)
	{
		printf("Unable to find matrix uniform in render model shader\n");
		return false;
	}

	return false;
}

void HollowCircle::Render(Matrix4 mvpMatrix, Matrix4 circlePos)
{
	glBindVertexArray(m_unVAO);
	glUseProgram(m_unHollowCircleID);

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

	// Draw the triangles in fan!
	//glDrawElements(GL_TRIANGLE_FAN, m_nNumberOfVerticies, GL_UNSIGNED_INT, NULL);
	glDrawElements(GL_LINE_STRIP, m_nNumberOfVerticies, GL_UNSIGNED_INT, NULL);

	glDisableVertexAttribArray(0);
}

void HollowCircle::SetupGeometry(float x, float y, float z, float radius, int numberOfSides, bool isGrabButton, float fociDistance)
{
	m_nNumberOfVerticies = (numberOfSides + 1) * 2;
	GLfloat doublePi = 2.0f * M_PI;

	// VBO data - Set vertexes
	if (fociDistance == 0)
	{
		// Draw a circle
		for (int i = 0; i < m_nNumberOfVerticies / 2; i++)
		{
			m_vStripVertex.push_back(x + (radius * cos(i * doublePi / numberOfSides)));
			m_vStripVertex.push_back(y + (radius * sin(i * doublePi / numberOfSides)));
			m_vStripVertex.push_back(z);
		}
		for (int i = 0; i < m_nNumberOfVerticies / 2; i++)
		{
			m_vStripVertex.push_back(x + ((radius - 0.0001f) * cos(i * doublePi / numberOfSides)));
			m_vStripVertex.push_back(y + ((radius - 0.0001f)* sin(i * doublePi / numberOfSides)));
			m_vStripVertex.push_back(z);
		}
	}
	else
	{
		// Draw a capsule
		m_vStripVertex.push_back(x + (radius * cos(0)));
		m_vStripVertex.push_back(y - (fociDistance / 2) + (radius * sin(0)));
		if (isGrabButton)
		{
			m_vStripVertex.push_back(z - 0.0015);
		}
		else
		{
			m_vStripVertex.push_back(z);
		}

		for (int i = 0; i <= m_nNumberOfVerticies / 4; i++)
		{
			m_vStripVertex.push_back(x + (radius * cos(i * doublePi / numberOfSides)));
			m_vStripVertex.push_back(y + (fociDistance / 2) + (radius * sin(i * doublePi / numberOfSides)));
			if (isGrabButton  && i <= m_nNumberOfVerticies / 8)
			{
				float max = m_nNumberOfVerticies / 8;
				float ratio = i / max;
				float offset = 0.0015 * (1 - ratio);
				m_vStripVertex.push_back(z - offset);
			}
			else
			{
				m_vStripVertex.push_back(z);
			}
		}
		for (int i = m_nNumberOfVerticies / 4; i <= m_nNumberOfVerticies / 2; i++)
		{
			m_vStripVertex.push_back(x + (radius * cos(i * doublePi / numberOfSides)));
			m_vStripVertex.push_back(y - (fociDistance / 2) + (radius * sin(i * doublePi / numberOfSides)));
			if (isGrabButton && i >= (m_nNumberOfVerticies * 3) / 8)
			{
				float max = m_nNumberOfVerticies / 8;
				float iZero = i - m_nNumberOfVerticies / 4;
				float ratio = iZero / max;
				float offset = 0.0015 * (1 - ratio);
				m_vStripVertex.push_back(z + offset);
			}
			else
			{
				m_vStripVertex.push_back(z);
			}
		}

		m_vStripVertex.push_back(x + ((radius - 0.0001f) * cos(0)));
		m_vStripVertex.push_back(y - (fociDistance / 2) + ((radius - 0.0001f)* sin(0)));
		if (isGrabButton)
		{
			m_vStripVertex.push_back(z - 0.0015);
		}
		else
		{
			m_vStripVertex.push_back(z);
		}

		for (int i = 0; i <= m_nNumberOfVerticies / 4; i++)
		{
			m_vStripVertex.push_back(x + ((radius - 0.0001f) * cos(i * doublePi / numberOfSides)));
			m_vStripVertex.push_back(y + (fociDistance / 2) + ((radius - 0.0001f)* sin(i * doublePi / numberOfSides)));
			if (isGrabButton && i <= m_nNumberOfVerticies / 8)
			{
				float max = m_nNumberOfVerticies / 8;
				float ratio = i / max;
				float offset = 0.0015 * (1 - ratio);
				m_vStripVertex.push_back(z - offset);
			}
			else
			{
				m_vStripVertex.push_back(z);
			}
		}

		for (int i = m_nNumberOfVerticies / 4; i <= m_nNumberOfVerticies / 2; i++)
		{
			m_vStripVertex.push_back(x + ((radius - 0.0001f) * cos(i * doublePi / numberOfSides)));
			m_vStripVertex.push_back(y - (fociDistance / 2) + ((radius - 0.0001f)* sin(i * doublePi / numberOfSides)));
			if (isGrabButton && i >= (m_nNumberOfVerticies * 3) / 8)
			{
				float max = m_nNumberOfVerticies / 8;
				float iZero = i - m_nNumberOfVerticies / 4;
				float ratio = iZero / max;
				float offset = 0.0015 * (1 - ratio);
				m_vStripVertex.push_back(z + offset);
			}
			else
			{
				m_vStripVertex.push_back(z);
			}
		}

		m_nNumberOfVerticies += 6;
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
	glBufferData(GL_ARRAY_BUFFER, m_vStripVertex.size() * sizeof(GLfloat), &m_vStripVertex[0], GL_STATIC_DRAW);

	//Create IBO
	glGenBuffers(1, &m_unIBO);
	glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, m_unIBO);
	glBufferData(GL_ELEMENT_ARRAY_BUFFER, m_vIndices.size() * sizeof(GLuint), &m_vIndices[0], GL_STATIC_DRAW);
}