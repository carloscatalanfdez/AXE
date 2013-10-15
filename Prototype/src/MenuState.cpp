#include "MenuState.h"
#include "AxeGame.h" 
#include "Player.h"
#include "LevelState.h"

MenuState::MenuState() {
	label = new ScreenLabel("PRODUCT FROM THE BADLADNS", 32);
	addChild(label);
}

MenuState::~MenuState() {
}

void MenuState::init() {
	label->setPosition(game->getWidth()/2.f - label->getWidth()/2.f, game->getHeight()/2.f - label->getHeight()/2.f);

	GameState::init();
}

void MenuState::Update() {
	Polycode::CoreInput *input = game->core->getInput();
	if (input->getKeyState(Polycode::KEY_ESCAPE)) {
		game->end();
	} else if (input->getKeyState(Polycode::KEY_RETURN)) {
		AxeGame *axeGame = dynamic_cast<AxeGame*>(game);
		if (axeGame) {
			axeGame->startFirstLevel();
		}
		return;
	}

	GameState::Update();
}