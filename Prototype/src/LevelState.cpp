#include "LevelState.h"
#include "Game.h" 

using namespace Polycode;

LevelState::LevelState() {
	image = new ScreenImage("Assets/knights.png");
	addChild(image);
}

LevelState::~LevelState() {
	delete image;
}

void LevelState::init() {
	image->setPosition(game->getWidth()/2.f - image->getWidth()/2.f, game->getHeight()/2.f - image->getHeight()/2.f);
}

void LevelState::Update() {
	Polycode::CoreInput *input = game->core->getInput();
	if (input->getKeyState(Polycode::KEY_ESCAPE))
		game->end();
	else if (input->getKeyState(Polycode::KEY_LEFT))
		image->setRotation(image->getRotation() - 60*game->core->getElapsed());
	else if (input->getKeyState(Polycode::KEY_RIGHT))
		image->setRotation(image->getRotation() + 60*game->core->getElapsed());
}

void LevelState::handleEvent(Event* e) {
}