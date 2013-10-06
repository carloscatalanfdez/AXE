#include "MenuState.h"
#include "Game.h" 
#include "Player.h"

MenuState::MenuState() {
	label = new ScreenLabel("PRODUCT FROM THE BADLADNS", 30);

	addChild(label);
}

MenuState::~MenuState() {
}

void MenuState::init() {
	label->setPosition(game->getWidth()/2.f - label->getWidth()/2.f, game->getHeight()/2.f - label->getHeight()/2.f);
}