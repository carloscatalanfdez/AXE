#include "Game.h"
#include "PolyScreenManager.h"
#include "GameState.h"

Game::Game(PolycodeView *view, int w, int h) { 
	core = new POLYCODE_CORE(view, w, h, false, false, 0, 0, 60);
	CoreServices::getInstance()->getResourceManager()->addArchive("default.pak");
	CoreServices::getInstance()->getResourceManager()->addDirResource("default", false);

	width = w, height = h;
	currentWorld = NULL;
}

Game::~Game() {
	if (currentWorld) {
		delete currentWorld;
	}
	delete core;
}

void Game::init() {
	currentWorld = createFirstWorld();
}

bool Game::update() {
	return core->updateAndRender();
}

GameState *Game::createFirstWorld() {
	return NULL;
}

void Game::changeWorld(GameState *world) {
	if (currentWorld) {
		CoreServices::getInstance()->getScreenManager()->removeScreen(currentWorld);
		delete currentWorld;
	}

	currentWorld = world;
	currentWorld->enabled = true;
	currentWorld->game = this;
	currentWorld->init();
}