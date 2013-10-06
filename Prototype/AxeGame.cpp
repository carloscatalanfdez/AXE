#include "AxeGame.h"
#include "MenuState.h"

AxeGame::AxeGame(PolycodeView *view) : Game(view, 640, 480) {
}

AxeGame::~AxeGame() {
}

void AxeGame::init() {
	// Parent's init must be the first call in this method
	Game::init();

	// Change world immediatelly, for testing purposes
	GameState *world = new GameState();
	ScreenLabel *label = new ScreenLabel("BLANK", 30);
	label->setPosition(320 - label->getWidth()/2.f, 240 - label->getHeight()/2.f);
	world->addChild(label);

	changeWorld(world);
}

bool AxeGame::update() {
	// Parent's update must be the last call in this method
	return Game::update();
}

GameState *AxeGame::createFirstWorld() {
	return new MenuState();
}