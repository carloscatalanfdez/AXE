#include "AxeGame.h"
#include "GameState.h"

AxeGame::AxeGame(PolycodeView *view) : Game(view, 640, 480) {
	// Change world immediatelly, for testing purposes
	GameState *mainGame = new GameState();
	ScreenLabel *label = new ScreenLabel("PRODUCT FROM THE BADLADNS", 30);
	label->setPosition(width/2.f - label->getWidth()/2.f, height/2.f - label->getHeight()/2.f);
	mainGame->addChild(label);

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