#include "LevelState.h"
#include "Player.h"
#include "Game.h"

using namespace Polycode;

LevelState::LevelState() {
	player = new Player();
	addChild(player);
}

LevelState::~LevelState() {
}

void LevelState::init() {
	player->setPosition(game->getWidth()/2.f - player->getWidth()/2.f, game->getHeight()/2.f - player->getHeight()/2.f);

	GameState::init();
}

void LevelState::Update() {
	Polycode::CoreInput *input = game->core->getInput();
	if (input->getKeyState(Polycode::KEY_ESCAPE))
		game->end();

	GameState::Update();
}