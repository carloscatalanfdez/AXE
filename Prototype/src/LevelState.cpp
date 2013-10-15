#include "LevelState.h"
#include "Player.h"
#include "Game.h"

using namespace Polycode;

LevelState::LevelState() {
}

LevelState::~LevelState() {
}

void LevelState::init() {
	GameState::init();
}

void LevelState::Update() {
	Polycode::CoreInput *input = game->core->getInput();
	if (input->getKeyState(Polycode::KEY_ESCAPE))
		game->end();

	GameState::Update();
}

void LevelState::completeLevel() {
	completeLevel(id + 1);
}

void LevelState::completeLevel(int nextLevel) {
	LevelState *nextState = new LevelState();
	// Reuse players
	nextState->loadLevel(nextLevel, players);
}

void LevelState::loadLevel(int levelId) {
	// Assume we have been given the players already
	loadLevel(levelId, players);
}

void LevelState::loadLevel(int levelId, Player* currentPlayers[]) {
	// Load xml or whatever here
	id = levelId;
	int levelWidth = 640;
	int levelHeight = 480;

	for (int i = 0; i < 2; i++) {
		players[i] = currentPlayers[i];
		if (players[i]) {
			addChild(players[i]);
		}
	}

	// HARDCODED LEVELS
	switch (id) {
	case 1:
		playerOne->setPosition(levelWidth - 2*playerOne->getWidth() - 20, levelHeight/2.f - playerOne->getHeight()/2.f);
		break;
	default:
		playerOne->setPosition(20, levelHeight/2.f - playerOne->getHeight()/2.f);
	}
}