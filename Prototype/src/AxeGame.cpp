#include "AxeGame.h"
#include "MenuState.h"

AxeGame::AxeGame(PolycodeView *view) : Game(view, 640, 480) {
}

AxeGame::~AxeGame() {
}

void AxeGame::init() {
	// Parent's init must be the first call in this method
	Game::init();

	CoreServices::getInstance()->getFontManager()->registerFont("GameFont", "Assets/prstart.ttf");
}

bool AxeGame::update() {
	// Parent's update must be the last call in this method
	return Game::update();
}

GameState *AxeGame::createFirstWorld() {
	return new MenuState();
}