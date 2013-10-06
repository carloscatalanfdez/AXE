#include "AxeGame.h"
#include "MenuState.h"

AxeGame::AxeGame(PolycodeView *view) : Game(view, 640, 480) {
	// Change world immediatelly, for testing purposes
	MenuState *mainGame = new MenuState();
	changeWorld(mainGame);
}

AxeGame::~AxeGame() {
}

GameState *AxeGame::initFirstWorld() {
	GameState *world = new GameState();
	ScreenLabel *label = new ScreenLabel("BLANK", 30);
	label->setPosition(320 - label->getWidth()/2.f, 240 - label->getHeight()/2.f);
	world->addChild(label);

	return world;
}