#include "LogoState.h"
#include "Axegame.h"

LogoState::LogoState() {
	logo = new ScreenImage("Assets/badladns_banner.png");
	addChild(logo);
}


LogoState::~LogoState() {
}

void LogoState::init() {
	renderer->clearColor = Color(0, 0, 0, 1);
	logo->setPosition(game->getWidth()/2.f - logo->getWidth()/2.f, game->getHeight()/2.f - logo->getHeight()/2.f);

	GameState::init();
}

void LogoState::Update() {
	Polycode::CoreInput *input = game->core->getInput();
	if (input->getKeyState(Polycode::KEY_RETURN)) {
		AxeGame *axeGame = dynamic_cast<AxeGame*>(game);
		if (axeGame) {
			axeGame->launchMenu();
		}
		return;
	}

	GameState::Update();
}