#include "Game.h"

Game::Game(PolycodeView *view) : Win32Core(view, 640,480,false, false, 0, 0,60) { 
	CoreServices::getInstance()->getResourceManager()->addArchive("default.pak");
	CoreServices::getInstance()->getResourceManager()->addDirResource("default", false);

	// Automatically becomes the active render target
	Screen* mainLevel = new Screen();
	ScreenLabel *label = new ScreenLabel("PRODUCT FROM THE BADLADNS", 30);
	label->setPosition(320 - label->getWidth()/2.f, 240 - label->getHeight()/2.f);
	mainLevel->addChild(label);
}

Game::~Game() {
    
}

bool Game::update() {
	return updateAndRender();
}