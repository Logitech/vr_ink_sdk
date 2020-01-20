#pragma once
#include <GL/glew.h>
#include <stdio.h>
#include "pathtools.h"
namespace LogiOGlTools {
	//-----------------------------------------------------------------------------
	// Purpose: Compiles a GL shader program and returns the handle. Returns 0 if
	//			the shader couldn't be compiled for some reason.
	//-----------------------------------------------------------------------------
	static GLuint CompileGLShader(const char *pchShaderName, const char *pchVertexShader, const char *pchFragmentShader)
	{
		GLuint unProgramID = glCreateProgram();

		GLuint nSceneVertexShader = glCreateShader(GL_VERTEX_SHADER);
		glShaderSource(nSceneVertexShader, 1, &pchVertexShader, NULL);
		glCompileShader(nSceneVertexShader);

		GLint vShaderCompiled = GL_FALSE;
		glGetShaderiv(nSceneVertexShader, GL_COMPILE_STATUS, &vShaderCompiled);
		if (vShaderCompiled != GL_TRUE)
		{
			printf("%s - Unable to compile vertex shader %d!\n", pchShaderName, nSceneVertexShader);
			glDeleteProgram(unProgramID);
			glDeleteShader(nSceneVertexShader);
			return 0;
		}
		glAttachShader(unProgramID, nSceneVertexShader);
		glDeleteShader(nSceneVertexShader); // the program hangs onto this once it's attached

		GLuint  nSceneFragmentShader = glCreateShader(GL_FRAGMENT_SHADER);
		glShaderSource(nSceneFragmentShader, 1, &pchFragmentShader, NULL);
		glCompileShader(nSceneFragmentShader);

		GLint fShaderCompiled = GL_FALSE;
		glGetShaderiv(nSceneFragmentShader, GL_COMPILE_STATUS, &fShaderCompiled);
		if (fShaderCompiled != GL_TRUE)
		{
			printf("%s - Unable to compile fragment shader %d!\n", pchShaderName, nSceneFragmentShader);
			glDeleteProgram(unProgramID);
			glDeleteShader(nSceneFragmentShader);
			return 0;
		}

		glAttachShader(unProgramID, nSceneFragmentShader);
		glDeleteShader(nSceneFragmentShader); // the program hangs onto this once it's attached

		glLinkProgram(unProgramID);

		GLint programSuccess = GL_TRUE;
		glGetProgramiv(unProgramID, GL_LINK_STATUS, &programSuccess);
		if (programSuccess != GL_TRUE)
		{
			printf("%s - Error linking program %d!\n", pchShaderName, unProgramID);
			glDeleteProgram(unProgramID);
			return 0;
		}

		glUseProgram(unProgramID);
		glUseProgram(0);

		return unProgramID;
	}

	static GLuint SetupGLTexture(std::string pngFilename, unsigned int &nImageWidth, unsigned int &nImageHeight)
	{
		GLuint iTexture = 0;
		std::string sExecutableDirectory = Path_StripFilename(Path_GetExecutablePath());
		std::string strFullPath = Path_MakeAbsolute(pngFilename, sExecutableDirectory);

		glGenTextures(1, &iTexture);
		glBindTexture(GL_TEXTURE_2D, iTexture);

		glGenerateMipmap(GL_TEXTURE_2D);

		glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE);
		glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_EDGE);
		glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
		glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR_MIPMAP_LINEAR);

		GLfloat fLargest;
		glGetFloatv(GL_MAX_TEXTURE_MAX_ANISOTROPY_EXT, &fLargest);
		glTexParameterf(GL_TEXTURE_2D, GL_TEXTURE_MAX_ANISOTROPY_EXT, fLargest);

		glBindTexture(GL_TEXTURE_2D, 0);

		return iTexture;
	}
}
