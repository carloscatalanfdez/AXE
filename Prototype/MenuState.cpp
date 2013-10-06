#include "MenuState.h"
#include "Game.h" 
#include "Player.h"
#include "LevelState.h"

MenuState::MenuState() {
	label = new ScreenLabel("PRODUCT FROM THE BADLADNS", 30);
	addChild(label);
}

MenuState::~MenuState() {
}

void MenuState::init() {
	label->setPosition(game->getWidth()/2.f - label->getWidth()/2.f, game->getHeight()/2.f - label->getHeight()/2.f);

	game->core->getInput()->addEventListener(this, InputEvent::EVENT_KEYDOWN);
}

void MenuState::Update() {
	GameState::Update();
}

void MenuState::handleEvent(Event *e) {
 	if(e->getDispatcher() == game->core->getInput()) {
		InputEvent *inputEvent = (InputEvent*)e;
		
		switch(e->getEventCode()) {
			case InputEvent::EVENT_KEYDOWN:
				switch (inputEvent->keyCode()) {
				case KEY_RETURN:
						game->changeWorld(new LevelState());
					break;
					case KEY_RIGHT:
					
					break;
				}
			break;		
		}
	}
}