#include "AxeGame.h"
#include "MenuState.h"
#include "LevelState.h"
#include "LogoState.h"
#include "Player.h"

AxeGame::AxeGame(PolycodeView *view) : Game(view, 640, 480) {
}

AxeGame::~AxeGame() {
	delete players[0];
	delete players[1];
}

void AxeGame::init() {
	// Parent's init must be the first call in this method
	Game::init();

	CoreServices::getInstance()->getFontManager()->registerFont("GameFont", "Assets/prstart.ttf");
}

bool AxeGame::update() {
	Polycode::CoreInput *input = core->getInput();
	if (input->getKeyState(Polycode::KEY_ESCAPE)) {
		end();
	}

	// Parent's update must be the last call in this method
	return Game::update();
}

GameState *AxeGame::createFirstWorld() {
	return new LogoState();
}

void AxeGame::launchMenu() {
	changeWorld(new MenuState());
}

void AxeGame::launchFirstLevel() {
	LevelState *firstLevel = new LevelState();
	// Hardcoded, these should be set up in the menu screen
	players[0] = new Player();
	players[1] = NULL;
	// Pass references to level for commodity
	firstLevel->playerOne = new Player();
	firstLevel->playerTwo = NULL;
	// Harcoding first level id
	firstLevel->loadLevel(1);

	changeWorld(firstLevel);
}