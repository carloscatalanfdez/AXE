#include "Game.h"
#include "PolyScreenManager.h"
#include "GameState.h"

Game::Game(PolycodeView *view, int w, int h) { 
	core = new POLYCODE_CORE(view, w, h, false, false, 0, 0, 60);
	
	CoreServices::getInstance()->getResourceManager()->addArchive("default.pak");
	CoreServices::getInstance()->getResourceManager()->addDirResource("default", false);

	Polycode::String pwd = core->getDefaultWorkingDirectory();

	CoreServices::getInstance()->getRenderer()->setTextureFilteringMode(Renderer::TEX_FILTERING_NEAREST);

	width = w, height = h;
	currentWorld = NULL;

	keepRunning = true;
}

Game::~Game() {
	if (currentWorld) {
		delete currentWorld;
	}
	delete core;
}

void Game::init() {
	setWorld(createFirstWorld());
}

void Game::end() {
	keepRunning = false;
}

bool Game::update() {
	if (!keepRunning)
		return false;
	
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

	setWorld(world);
}

void Game::setWorld(GameState *world) {
	if (world) {
		currentWorld = world;
		currentWorld->enabled = true;
		currentWorld->game = this;
		currentWorld->init();
	}
}