#include <stdio.h>
#include <iostream>
#include "CMainApplication.h"

int main(int argc, char *argv[])
{
	SceneRenderer *sceneRenderer = new SceneRenderer();
	CMainApplication *pMainApplication = new CMainApplication(argc, argv, sceneRenderer);

	if (!pMainApplication->BInit())
	{
		pMainApplication->Shutdown();
		return 1;
	}

	pMainApplication->RunMainLoop();

	pMainApplication->Shutdown();

	return 0;
}