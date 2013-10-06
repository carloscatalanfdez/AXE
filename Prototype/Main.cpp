#include <Polycode.h>
#include "AxeGame.h"
#include "PolycodeView.h"
#include "windows.h"

using namespace Polycode;

int APIENTRY WinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPSTR lpCmdLine, int nCmdShow) {
	PolycodeView *view = new PolycodeView(hInstance, nCmdShow, L"AXE");
	AxeGame *game = new AxeGame(view);
	game->init();

	MSG Msg;
	do {
		if(PeekMessage(&Msg, NULL, 0,0,PM_REMOVE)) {
			TranslateMessage(&Msg);
			DispatchMessage(&Msg);
		}
	} while(game->update());
	return Msg.wParam;
}